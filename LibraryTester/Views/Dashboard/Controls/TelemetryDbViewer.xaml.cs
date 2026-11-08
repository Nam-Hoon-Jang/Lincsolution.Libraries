using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using Lincsolution.Core.Telemetry.Models;

namespace LibraryTester.Views.Dashboard.Controls
{
    /// <summary>
    /// Telemetry SchemaVersion 1 파일을 읽기 전용으로 조회하는 테스트용 화면.
    /// 파일 이력은 메모리가 아닌 테스트 저장 폴더에서 다시 읽으므로 View가 재생성되어도 확인 가능.
    /// SQL과 파일 I/O는 작업 스레드에서 실행하고 화면에는 최대 200행씩 전달.
    /// </summary>
    public partial class TelemetryDbViewer : UserControl
    {
        private const int PageSize = 200;
        private bool busy;
        private bool suppressSelection;
        private string? currentPath;
        private int? selectedChannel;
        private long fromTicks;
        private long toTicks = long.MaxValue;
        private int samplePage;
        private int eventPage;
        private bool moreSamples;
        private bool moreEvents;

        public TelemetryDbViewer() { InitializeComponent(); }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(this) || busy) return;
            await RefreshFilesAsync();
        }

        private async void OnRefresh(object sender, RoutedEventArgs e) { if(!busy) await RefreshFilesAsync(); }

        /// <summary>최신 테스트 파일 목록을 복원. 읽을 파일이 있으면 현재 파일 또는 최신 파일 표시.</summary>
        private async Task RefreshFilesAsync()
        {
            SetBusy(true);
            try
            {
                string root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Lincsolution", "LibraryTester", "Telemetry");
                var files = await Task.Run(() => Directory.Exists(root)
                    ? Directory.EnumerateFiles(root, "telemetry.db", SearchOption.AllDirectories)
                        .Select(p => new FileInfo(p)).OrderByDescending(f => f.LastWriteTimeUtc).Take(100)
                        .Select(f => new FileEntry(f.FullName, f.Directory!.Name, $"{f.Length / 1024.0:N1} KB")).ToArray()
                    : Array.Empty<FileEntry>());
                suppressSelection = true;
                FilesList.ItemsSource = files;
                FilesList.SelectedItem = files.FirstOrDefault(f => f.Path == currentPath) ?? files.FirstOrDefault();
                suppressSelection = false;
                ActivityText.Text = files.Length == 0 ? "기록 없음 · DB 파일 열기로 다른 위치를 선택할 수 있습니다." : "파일 목록 갱신 완료";
            }
            catch(Exception error) { ActivityText.Text = "파일 목록 오류: " + error.Message; }
            finally { suppressSelection = false; SetBusy(false); }
            if(FilesList.SelectedItem is FileEntry file) await OpenDatabaseAsync(file.Path);
        }

        private async void OnFileSelected(object sender, SelectionChangedEventArgs e)
        {
            if(!busy && !suppressSelection && FilesList.SelectedItem is FileEntry file)
                await OpenDatabaseAsync(file.Path);
        }

        private async void OnOpen(object sender, RoutedEventArgs e)
        {
            if(busy) return;
            var dialog = new OpenFileDialog { Title = "Telemetry SQLite DB 선택", Filter = "SQLite DB|*.db;*.sqlite;*.sqlite3|모든 파일|*.*", CheckFileExists = true };
            if(dialog.ShowDialog() == true) await OpenDatabaseAsync(dialog.FileName);
        }

        /// <summary>다른 파일 선택 시 필터와 페이지 초기화. 실패한 파일에 이전 조회 결과를 표시하지 않음.</summary>
        private async Task OpenDatabaseAsync(string path)
        {
            currentPath = path;
            PathBox.Text = path;
            samplePage = eventPage = 0;
            selectedChannel = null;
            fromTicks = 0; toTicks = long.MaxValue;
            FromBox.Text = "0"; ToBox.Text = string.Empty;
            ChannelBox.ItemsSource = null;
            await QueryAsync(true);
        }

        private async void OnReload(object sender, RoutedEventArgs e) { if(!busy && currentPath != null) await QueryAsync(false); }

        private async void OnApply(object sender, RoutedEventArgs e)
        {
            if(busy || currentPath is null) return;
            try
            {
                long start = ParseSeconds(FromBox.Text, false);
                long end = ParseSeconds(ToBox.Text, true);
                if(end < start) throw new ArgumentException("종료 시간이 시작 시간보다 작습니다.");
                fromTicks = start; toTicks = end;
                selectedChannel = (ChannelBox.SelectedItem as ChannelOption)?.Id;
                samplePage = eventPage = 0;
                await QueryAsync(false);
            }
            catch(Exception error) { ActivityText.Text = "필터 오류: " + error.Message; }
        }

        /// <summary>초 입력을 Tick으로 변환. 빈 종료 값은 제한 없음. 음수/범위 초과 입력은 거부.</summary>
        private static long ParseSeconds(string text, bool allowEmpty)
        {
            if(allowEmpty && string.IsNullOrWhiteSpace(text)) return long.MaxValue;
            if(!decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value) || value < 0)
                throw new ArgumentException("경과 시간은 0 이상의 숫자(소수점 .)로 입력하세요.");
            return checked((long)(value * TimeSpan.TicksPerSecond));
        }

        private void OnDataTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ReferenceEquals(e.Source, DataTabs) && NextButton != null) UpdatePager();
        }
        private async void OnPrevious(object sender, RoutedEventArgs e)
        {
            if(busy) return;
            if(DataTabs.SelectedIndex == 0 && samplePage > 0) samplePage--;
            else if(DataTabs.SelectedIndex == 1 && eventPage > 0) eventPage--;
            await QueryAsync(false);
        }
        private async void OnNext(object sender, RoutedEventArgs e)
        {
            if(busy) return;
            if(DataTabs.SelectedIndex == 0 && moreSamples) samplePage++;
            else if(DataTabs.SelectedIndex == 1 && moreEvents) eventPage++;
            await QueryAsync(false);
        }

        /// <summary>UI 상태를 값으로 복사하여 작업 스레드에서 조회. 모든 결과 반영은 UI 스레드에서 수행.</summary>
        private async Task QueryAsync(bool resetChannels)
        {
            if(currentPath is null) return;
            SetBusy(true);
            ActivityText.Text = "읽기 전용 조회 중…";
            string path = currentPath;
            int? channel = selectedChannel;
            long start = fromTicks, end = toTicks;
            int samplesOffset = samplePage, eventsOffset = eventPage;
            try
            {
                var result = await Task.Run(() => ReadDatabase(path, channel, start, end, samplesOffset, eventsOffset));
                SessionText.Text = result.Summary;
                SamplesGrid.ItemsSource = result.Samples.DefaultView;
                EventsGrid.ItemsSource = result.Events.DefaultView;
                ChannelsGrid.ItemsSource = result.Channels.DefaultView;
                moreSamples = result.MoreSamples; moreEvents = result.MoreEvents;
                if(resetChannels)
                {
                    ChannelBox.ItemsSource = result.ChannelOptions;
                    ChannelBox.SelectedIndex = 0;
                }
                ActivityText.Text = "조회 완료 · " + DateTime.Now.ToString("HH:mm:ss") + " · 시각은 로컬 시간";
            }
            catch(Exception error)
            {
                SamplesGrid.ItemsSource = EventsGrid.ItemsSource = ChannelsGrid.ItemsSource = null;
                moreSamples = moreEvents = false;
                SessionText.Text = "DB를 읽지 못했습니다. Telemetry SchemaVersion 1 파일인지 확인하세요.";
                ActivityText.Text = "조회 실패: " + error.Message;
            }
            finally { SetBusy(false); }
        }

        /// <summary>
        /// Mode=ReadOnly: 파일 생성/수정 불가. 한 읽기 트랜잭션으로 표와 건수의 조회 시점을 맞춤.
        /// DB 잠금 대기는 3초로 제한. 전체 데이터를 UI 메모리로 가져오지 않고 LIMIT/OFFSET으로 제한.
        /// </summary>
        private static QueryResult ReadDatabase(string path, int? channel, long start, long end, int samplePage, int eventPage)
        {
            var builder = new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly, Pooling = false, DefaultTimeout = 3 };
            using var connection = new SqliteConnection(builder.ToString());
            connection.Open();
            using var transaction = connection.BeginTransaction(deferred: true);
            var result = new QueryResult();
            using(var command = Command(connection, transaction, @"SELECT SessionId, BuildFileName, StartUtcTicks, EndUtcTicks, EndElapsedTicks, EndReason, SchemaVersion,
                (SELECT COUNT(*) FROM Frames), (SELECT COUNT(*) FROM Samples), (SELECT COUNT(*) FROM Events) FROM Session WHERE Id=1;"))
            using(var reader = command.ExecuteReader())
            {
                if(!reader.Read() || reader.GetInt32(6) != 1) throw new InvalidDataException("지원하는 Session/SchemaVersion 정보가 없습니다.");
                string reason = reader.IsDBNull(5) ? "종료 기록 없음 (수집 중 또는 비정상 종료)" : reader.GetString(5);
                result.Summary = $"Job: {reader.GetString(1)}\nSession: {reader.GetString(0)}\n시작: {LocalTime(reader.GetInt64(2))}\n종료: {(reader.IsDBNull(3) ? "—" : LocalTime(reader.GetInt64(3)))} · {reason}\n경과: {(reader.IsDBNull(4) ? "—" : Seconds(reader.GetInt64(4)))}초 · Frames {reader.GetInt64(7):N0} / Samples {reader.GetInt64(8):N0} / Events {reader.GetInt64(9):N0}";
            }
            result.Channels = Table("ID", "채널", "단위", "자료형", "소수 자릿수");
            result.ChannelOptions.Add(new ChannelOption(null, "모든 채널"));
            using(var command = Command(connection, transaction, "SELECT ChannelId, Name, Unit, ValueType, DecimalPlaces FROM Channels ORDER BY ChannelId LIMIT 10001;"))
            using(var reader = command.ExecuteReader())
            {
                while(reader.Read())
                {
                    if(result.Channels.Rows.Count >= 10000) throw new InvalidDataException("테스트 뷰어는 최대 10,000채널을 지원합니다.");
                    int id = reader.GetInt32(0);
                    result.Channels.Rows.Add(id, reader.GetString(1), reader.GetString(2), ((TelemetryValueType)reader.GetInt32(3)).ToString(), reader.GetInt32(4));
                    result.ChannelOptions.Add(new ChannelOption(id, id + " · " + reader.GetString(1)));
                }
            }

            result.Samples = Table("Frame", "경과 초", "수집 시각", "채널 ID", "채널", "값", "단위", "상태", "DB 원시값", "장치 시각");
            using(var command = Command(connection, transaction, @"SELECT s.FrameNumber, f.ElapsedTicks, f.UtcTicks, s.ChannelId, c.Name, s.Value, c.Unit, s.Status, c.ValueType, c.DecimalPlaces, s.SourceUtcTicks
                FROM Samples s JOIN Frames f ON f.FrameNumber=s.FrameNumber JOIN Channels c ON c.ChannelId=s.ChannelId
                WHERE f.ElapsedTicks >= $start AND f.ElapsedTicks <= $end AND ($channel IS NULL OR s.ChannelId=$channel)
                ORDER BY f.ElapsedTicks, s.FrameNumber, s.ChannelId LIMIT $limit OFFSET $offset;"))
            {
                Bounds(command, start, end, samplePage);
                command.Parameters.AddWithValue("$channel", (object?)channel ?? DBNull.Value);
                using var reader = command.ExecuteReader();
                while(reader.Read())
                {
                    if(result.Samples.Rows.Count == PageSize) { result.MoreSamples = true; break; }
                    string raw = reader.IsDBNull(5) ? "NULL" : reader.GetInt64(5).ToString(CultureInfo.InvariantCulture);
                    string value = reader.IsDBNull(5) ? "값 없음" : RestoreValue(reader.GetInt64(5), reader.GetInt32(8), reader.GetInt32(9));
                    result.Samples.Rows.Add(reader.GetInt64(0), Seconds(reader.GetInt64(1)), LocalTime(reader.GetInt64(2)), reader.GetInt32(3),
                        reader.GetString(4), value, reader.GetString(6), ((TelemetryDataStatus)reader.GetInt32(7)).ToString(), raw,
                        reader.IsDBNull(10) ? "—" : LocalTime(reader.GetInt64(10)));
                }
            }
            result.Events = Table("ID", "경과 초", "발생 시각", "코드", "발생 대상", "작업 ID", "메시지");
            using(var command = Command(connection, transaction, @"SELECT EventId, ElapsedTicks, UtcTicks, EventCode, Source, OperationId, Message
                FROM Events WHERE ElapsedTicks >= $start AND ElapsedTicks <= $end ORDER BY ElapsedTicks, EventId LIMIT $limit OFFSET $offset;"))
            {
                Bounds(command, start, end, eventPage);
                using var reader = command.ExecuteReader();
                while(reader.Read())
                {
                    if(result.Events.Rows.Count == PageSize) { result.MoreEvents = true; break; }
                    result.Events.Rows.Add(reader.GetInt64(0), Seconds(reader.GetInt64(1)), LocalTime(reader.GetInt64(2)), reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6));
                }
            }
            transaction.Commit();
            return result;
        }

        private static SqliteCommand Command(SqliteConnection connection, SqliteTransaction transaction, string sql)
        {
            var command = connection.CreateCommand(); command.Transaction = transaction; command.CommandText = sql; return command;
        }
        private static void Bounds(SqliteCommand command, long start, long end, int page)
        {
            command.Parameters.AddWithValue("$start", start); command.Parameters.AddWithValue("$end", end);
            command.Parameters.AddWithValue("$limit", PageSize + 1); command.Parameters.AddWithValue("$offset", checked((long)page * PageSize));
        }
        private static DataTable Table(params string[] columns)
        {
            var table = new DataTable(); foreach(string name in columns) table.Columns.Add(name, typeof(string)); return table;
        }
        private static string LocalTime(long ticks) => new DateTimeOffset(ticks, TimeSpan.Zero).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
        private static string Seconds(long ticks) => (ticks / (decimal)TimeSpan.TicksPerSecond).ToString("0.0000000", CultureInfo.InvariantCulture);

        /// <summary>저장된 정수값을 채널 정의에 맞춰 복원. decimal로 계산하여 표시 과정의 부동소수 오차 방지.</summary>
        private static string RestoreValue(long value, int type, int places)
        {
            if(type == (int)TelemetryValueType.Int) return value.ToString(CultureInfo.InvariantCulture);
            if(type == (int)TelemetryValueType.Boolean) return value == 0 ? "False" : "True";
            if(type != (int)TelemetryValueType.Real || places < 0 || places > 15) throw new InvalidDataException("잘못된 채널 자료형/정밀도입니다.");
            decimal scale = 1; for(int i=0; i<places; i++) scale *= 10;
            return (value / scale).ToString("F" + places, CultureInfo.InvariantCulture);
        }

        private void SetBusy(bool value)
        {
            busy = value;
            OpenButton.IsEnabled = RefreshButton.IsEnabled = FilesList.IsEnabled = !value;
            ReloadButton.IsEnabled = FilterPanel.IsEnabled = !value && currentPath != null;
            UpdatePager();
        }
        private void UpdatePager()
        {
            int tab = DataTabs.SelectedIndex;
            int page = tab == 0 ? samplePage : eventPage;
            PreviousButton.IsEnabled = !busy && currentPath != null && tab != 2 && page > 0;
            NextButton.IsEnabled = !busy && currentPath != null && (tab == 0 ? moreSamples : tab == 1 && moreEvents);
            PageText.Text = tab == 2 ? "채널 정의 전체 (ID 순)" : $"{page + 1}페이지 · 페이지당 최대 {PageSize}행";
        }
        private sealed record FileEntry(string Path, string Name, string Size);
        private sealed record ChannelOption(int? Id, string Label);
        private sealed class QueryResult
        {
            public string Summary = string.Empty;
            public DataTable Samples = new(), Events = new(), Channels = new();
            public List<ChannelOption> ChannelOptions = new();
            public bool MoreSamples, MoreEvents;
        }
    }
}
