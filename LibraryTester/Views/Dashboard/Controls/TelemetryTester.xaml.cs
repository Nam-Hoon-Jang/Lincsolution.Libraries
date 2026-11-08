using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Lincsolution.Core.Telemetry;
using Lincsolution.Core.Telemetry.Models;
using Microsoft.Data.Sqlite;

namespace LibraryTester.Views.Dashboard.Controls
{
    /// <summary>
    /// 실제 장비 없이 수집 목록 API와 자동 저장 수명주기를 검증하는 테스트 화면.
    /// 수집은 UI Timer에서 가상 값 생성만 수행하며 DB I/O는 Recorder/작업 스레드에서 실행.
    /// 테스트 전용이므로 화면 상태와 이벤트 처리는 이 코드 비하인드 안에 둔다.
    /// </summary>
    public partial class TelemetryTester : UserControl
    {
        private readonly DispatcherTimer collectionTimer = new DispatcherTimer();
        private readonly ObservableCollection<SensorRow> sensors = new();
        private readonly ObservableCollection<string> messages = new();
        private TelemetryRecorder? recorder;
        private TelemetrySession? session;
        private Window? hostWindow;
        private Task operation = Task.CompletedTask;
        private Task<bool>? stopTask;
        private TelemetryEndReason? endingReason;
        private bool busy;
        private bool viewLoaded;
        private bool closing;
        private long frameCount;
        private long eventCount;
        private long rejectedCount;
        private string databasePath = string.Empty;

        public TelemetryTester()
        {
            InitializeComponent();
            SensorGrid.ItemsSource = sensors;
            LogList.ItemsSource = messages;
            collectionTimer.Tick += OnCollect;
        }

        /// <summary>화면에 연결된 Window의 닫기를 감시하여 마지막 저장을 기다릴 수 있도록 구성.</summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(this)) return;
            viewLoaded = true;
            var window = Window.GetWindow(this);
            if(hostWindow != window)
            {
                if(hostWindow != null) hostWindow.Closing -= OnWindowClosing;
                hostWindow = window;
                if(hostWindow != null) hostWindow.Closing += OnWindowClosing;
            }
            UpdateButtons();
        }

        /// <summary>탭/화면 이동 시 숨은 Timer가 계속 기록하지 않도록 취소 종료. 오류면 재시도 상태 유지.</summary>
        private async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            viewLoaded = false;
            collectionTimer.Stop();
            await operation;
            bool ended = await StopAsync(TelemetryEndReason.Cancelled);
            if(ended && !viewLoaded && hostWindow != null)
            {
                hostWindow.Closing -= OnWindowClosing;
                hostWindow = null;
            }
        }

        /// <summary>닫기를 한 번 보류하고 저장 종료 후 다시 닫음. 저장 실패 시 창과 미저장 데이터를 유지.</summary>
        private async void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            if(recorder is null && !busy) return;
            e.Cancel = true;
            if(closing) return;
            closing = true;
            collectionTimer.Stop();
            await operation;
            bool ended = await StopAsync(TelemetryEndReason.Cancelled);
            closing = false;
            if(ended && sender is Window window)
            {
                window.Closing -= OnWindowClosing;
                window.Close();
            }
        }

        private async void OnStart(object sender, RoutedEventArgs e)
        {
            if(busy || recorder != null) return;
            operation = StartAsync();
            await operation;
        }

        /// <summary>입력 검사 후 새 경로를 만들고 DB 초기화. 파일 생성 작업도 UI를 막지 않도록 분리.</summary>
        private async Task StartAsync()
        {
            if(!ReadInteger(SensorCountBox, 1, 1000, out int count) ||
               !ReadInteger(IntervalBox, 10, 60000, out int interval) ||
               !ReadInteger(SaveIntervalBox, 10, 60000, out int saveInterval))
            {
                Log("설정 오류: 센서 1~1000개, 수집/저장 주기 10~60000ms를 입력하세요.");
                return;
            }
            busy = true;
            UpdateButtons();
            StateText.Text = "DB 준비 중";
            try
            {
                sensors.Clear();
                var channels = new List<TelemetryChannel>();
                for(int id = 1; id <= count; id++)
                {
                    // 일부 채널은 정수/Boolean으로 구성하여 세 자료형을 함께 검증.
                    var kind = id % 10 == 0 ? TelemetryValueType.Boolean :
                        id % 5 == 0 ? TelemetryValueType.Int : TelemetryValueType.Real;
                    string name = "Sensor " + id.ToString("D3");
                    channels.Add(new TelemetryChannel(id, name, kind == TelemetryValueType.Real ? "℃" : "",
                        kind, kind == TelemetryValueType.Real ? 3 : 0));
                    sensors.Add(new SensorRow(id, name, kind));
                }
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Lincsolution", "LibraryTester", "Telemetry", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss-fff") + " " + Guid.NewGuid().ToString("N").Substring(0, 6));
                databasePath = Path.Combine(folder, "telemetry.db");
                PathBox.Text = databasePath;
                var newRecorder = new TelemetryRecorder(saveInterval);
                newRecorder.StorageFailed += OnStorageFailed;
                try
                {
                    session = await Task.Run(() => newRecorder.StartSession(databasePath, "TelemetryTester.job", channels));
                    recorder = newRecorder;
                }
                catch
                {
                    newRecorder.StorageFailed -= OnStorageFailed;
                    newRecorder.Dispose();
                    throw;
                }
                frameCount = eventCount = rejectedCount = 0;
                endingReason = null;
                DatabaseResultText.Text = "수집 중 · 실제 저장 건수는 종료 후 확인하세요.";
                collectionTimer.Interval = TimeSpan.FromMilliseconds(interval);
                if(viewLoaded && !closing) collectionTimer.Start();
                StateText.Text = "수집 중 · " + count + "채널 / " + interval + "ms";
                Log("Session 시작: " + session.SessionId);
                UpdateStatistics();
            }
            catch(Exception error) { ShowError("시작 실패", error); }
            finally { busy = false; UpdateButtons(); }
        }

        /// <summary>수집 묶음에 공통 시각을 붙이고 센서별 입력 목록만 Recorder에 전달.</summary>
        private void OnCollect(object? sender, EventArgs e)
        {
            if(busy || recorder is null || session is null) return;
            try
            {
                var timestamp = recorder.CaptureTimestamp();
                double seconds = timestamp.Elapsed.TotalSeconds;
                var readings = new List<TelemetryReading>(sensors.Count);
                foreach(var row in sensors)
                {
                    bool missing = MissingBox.IsChecked == true && row.Id % 17 == 0;
                    var status = missing ? TelemetryDataStatus.Missing : TelemetryDataStatus.Good;
                    long? integer = null;
                    double? real = null;
                    bool? boolean = null;
                    if(!missing)
                    {
                        if(row.Kind == TelemetryValueType.Int) integer = (long)(seconds * 10) + row.Id;
                        else if(row.Kind == TelemetryValueType.Boolean) boolean = ((long)seconds + row.Id) % 2 == 0;
                        else real = 24 + row.Id * 0.1 + Math.Sin(seconds + row.Id * 0.2) * 3;
                    }
                    readings.Add(new TelemetryReading(row.Id, integer, real, boolean, status));
                    row.SetValue(real?.ToString("F3", CultureInfo.InvariantCulture) ?? integer?.ToString() ?? boolean?.ToString() ?? "—", status.ToString());
                }
                if(recorder.TryRecordReadings(session.SessionId, readings, timestamp)) frameCount++;
                else
                {
                    rejectedCount++;
                    collectionTimer.Stop();
                    StateText.Text = "대기열 포화 · 수집 중단";
                    Log("이번 묶음은 접수되지 않았습니다. 저장 / 재시도 후 수집을 재개하세요.");
                }
                UpdateStatistics();
            }
            catch(Exception error)
            {
                collectionTimer.Stop();
                ShowError("수집 중단", error);
                UpdateButtons();
            }
        }

        /// <summary>라이브러리 오류는 작업 스레드에서 오므로 UI Dispatcher에 비동기로 전달.</summary>
        private void OnStorageFailed(object? sender, TelemetryStorageErrorEventArgs e)
        {
            if(Dispatcher.HasShutdownStarted) return;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if(!ReferenceEquals(sender, recorder) || session?.SessionId != e.SessionId) return;
                collectionTimer.Stop();
                ShowError("자동 저장 실패 · 원인 해결 후 재시도", e.Exception);
                UpdateStatistics();
                UpdateButtons();
            }));
        }

        private void OnRecordEvent(object sender, RoutedEventArgs e)
        {
            if(recorder is null || session is null || busy) return;
            try
            {
                bool accepted = recorder.TryRecordEvent(session.SessionId,
                    new TelemetryEvent(recorder.CaptureTimestamp(), "ManualMarker", "TelemetryTester",
                        message: "사용자가 이벤트 기록 버튼을 눌렀습니다."));
                if(accepted) { eventCount++; Log("ManualMarker 이벤트 접수"); }
                else { rejectedCount++; Log("이벤트 접수 거부: 대기열 포화"); }
                UpdateStatistics();
            }
            catch(Exception error) { ShowError("이벤트 접수 실패", error); }
        }

        private async void OnFlush(object sender, RoutedEventArgs e)
        {
            if(busy || recorder is null || session is null) return;
            operation = FlushAsync();
            await operation;
        }

        private async Task FlushAsync()
        {
            busy = true;
            UpdateButtons();
            try
            {
                var activeRecorder = recorder!;
                var id = session!.SessionId;
                int saved = await Task.Run(() => activeRecorder.Flush(id));
                Log("수동 저장 완료: " + saved + "개 Frame/Event (자동 저장분 제외)");
                if(viewLoaded && !closing && endingReason is null)
                {
                    collectionTimer.Start();
                    StateText.Text = "수집 중 · 저장 재시도 성공";
                }
                UpdateStatistics();
            }
            catch(Exception error) { collectionTimer.Stop(); ShowError("저장 실패", error); }
            finally { busy = false; UpdateButtons(); }
        }

        private async void OnStop(object sender, RoutedEventArgs e) { if(!busy) await StopAsync(TelemetryEndReason.Completed); }
        private async void OnCancel(object sender, RoutedEventArgs e) { if(!busy) await StopAsync(TelemetryEndReason.Cancelled); }

        /// <summary>탭 이동/종료 버튼/창 닫기가 겹쳐도 하나의 종료 작업을 공유.</summary>
        private Task<bool> StopAsync(TelemetryEndReason reason)
        {
            if(stopTask != null && !stopTask.IsCompleted) return stopTask;
            stopTask = StopCoreAsync(reason);
            return stopTask;
        }

        private async Task<bool> StopCoreAsync(TelemetryEndReason reason)
        {
            collectionTimer.Stop();
            if(recorder is null || session is null) return true;
            busy = true;
            endingReason ??= reason;
            StateText.Text = "남은 데이터 저장 및 종료 중";
            UpdateButtons();
            try
            {
                var ended = await recorder.EndSessionAsync(session.SessionId, endingReason.Value);
                recorder.StorageFailed -= OnStorageFailed;
                recorder.Dispose();
                recorder = null;
                session = null;
                StateText.Text = "종료 완료 · " + endingReason + " · " + ended.Elapsed.TotalSeconds.ToString("F1") + "초";
                Log(StateText.Text);
                UpdateStatistics();
                return true;
            }
            catch(Exception error)
            {
                ShowError("종료 실패 · 데이터를 보존했습니다. 정상 종료 버튼으로 재시도", error);
                return false;
            }
            finally { busy = false; UpdateButtons(); }
        }

        private async void OnInspect(object sender, RoutedEventArgs e)
        {
            if(busy || recorder != null || !File.Exists(databasePath)) return;
            operation = InspectAsync();
            await operation;
        }

        /// <summary>Recorder 카운터가 아닌 실제 DB의 행 수와 종료 정보를 읽어 저장 결과 검증.</summary>
        private async Task InspectAsync()
        {
            busy = true;
            UpdateButtons();
            string path = databasePath;
            try
            {
                string result = await Task.Run(() =>
                {
                    var builder = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false };
                    using var connection = new SqliteConnection(builder.ToString());
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = "SELECT (SELECT COUNT(*) FROM Frames), (SELECT COUNT(*) FROM Samples), (SELECT COUNT(*) FROM Events), EndReason FROM Session WHERE Id = 1;";
                    using var reader = command.ExecuteReader();
                    if(!reader.Read()) throw new InvalidOperationException("Session 정보가 없습니다.");
                    long frames = reader.GetInt64(0), samples = reader.GetInt64(1), events = reader.GetInt64(2);
                    bool match = frames == frameCount && samples == frameCount * sensors.Count && events == eventCount;
                    return $"DB 검증 {(match ? "일치" : "불일치")} · Frames {frames:N0} · Samples {samples:N0} · Events {events:N0} · 종료 {(reader.IsDBNull(3) ? "미완료" : reader.GetString(3))} · {new FileInfo(path).Length / 1024.0:N1} KB";
                });
                DatabaseResultText.Text = result;
                Log(result);
            }
            catch(Exception error) { ShowError("DB 조회 실패", error); }
            finally { busy = false; UpdateButtons(); }
        }

        private void OnOpenFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                string? directory = Path.GetDirectoryName(databasePath);
                if(directory != null && Directory.Exists(directory))
                    Process.Start(new ProcessStartInfo(directory) { UseShellExecute = true });
            }
            catch(Exception error) { ShowError("폴더 열기 실패", error); }
        }

        private void UpdateButtons()
        {
            bool active = recorder != null;
            StartButton.IsEnabled = !busy && !active;
            SensorCountBox.IsEnabled = IntervalBox.IsEnabled = SaveIntervalBox.IsEnabled = !busy && !active;
            StopButton.IsEnabled = !busy && active;
            CancelButton.IsEnabled = !busy && active && endingReason is null;
            EventButton.IsEnabled = !busy && recorder?.IsRecording == true;
            FlushButton.IsEnabled = !busy && active && endingReason is null;
            InspectButton.IsEnabled = !busy && !active && File.Exists(databasePath);
            FolderButton.IsEnabled = File.Exists(databasePath);
        }

        private void UpdateStatistics() => StatisticsText.Text =
            $"Frame 접수 {frameCount:N0} · Event 접수 {eventCount:N0} · 대기 {recorder?.PendingRecordCount ?? 0:N0} · 포화 거부 {rejectedCount:N0}";

        private static bool ReadInteger(TextBox box, int min, int max, out int value) =>
            int.TryParse(box.Text, out value) && value >= min && value <= max;

        private void ShowError(string title, Exception error)
        {
            StateText.Text = title;
            Log(title + ": " + error.Message);
        }

        private void Log(string text)
        {
            messages.Insert(0, DateTime.Now.ToString("HH:mm:ss.fff") + "  " + text);
            while(messages.Count > 200) messages.RemoveAt(messages.Count - 1);
        }

        /// <summary>화면 표시용 센서 한 행. 저장 데이터와 독립적으로 최근 값만 보관.</summary>
        public sealed class SensorRow : INotifyPropertyChanged
        {
            public int Id { get; }
            public string Name { get; }
            public TelemetryValueType Kind { get; }
            public string Value { get; private set; } = "—";
            public string Status { get; private set; } = "대기";
            public event PropertyChangedEventHandler? PropertyChanged;
            public SensorRow(int id, string name, TelemetryValueType kind) { Id = id; Name = name; Kind = kind; }
            public void SetValue(string value, string status)
            {
                Value = value; Status = status;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }
    }
}
