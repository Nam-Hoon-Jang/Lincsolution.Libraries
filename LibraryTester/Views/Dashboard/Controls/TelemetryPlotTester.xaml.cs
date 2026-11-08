using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Lincsolution.Core.Telemetry;
using Lincsolution.Core.Telemetry.Models;
using Lincsolution.UI.Controls;
using Microsoft.Win32;

namespace LibraryTester.Views.Dashboard.Controls
{
    /// <summary>
    /// Core 조회 API를 재사용 Plot 컨트롤에 연결하는 테스트 화면.
    /// 체크박스 변경은 짧게 모아 조회하고, 새 요청이 들어오면 이전 조회를 취소.
    /// 완료 순서가 뒤바뀌어도 요청 번호가 다른 결과는 UI에 반영하지 않음.
    /// </summary>
    public partial class TelemetryPlotTester : UserControl
    {
        private readonly ObservableCollection<PlotSeries> series=new();
        private readonly DispatcherTimer debounce=new() { Interval=TimeSpan.FromMilliseconds(180) };
        // 취소 직후에도 이전 SQLite 쿼리가 잠시 실행될 수 있으므로 동시에 한 쿼리만 실행.
        private readonly SemaphoreSlim queryGate=new(1,1);
        private CancellationTokenSource? cancellation;
        private int request;
        private bool updating;
        private bool loaded;
        private TelemetryQueryReader? reader;
        private double lastSeconds;
        private static readonly Color[] Palette={Colors.SteelBlue,Colors.OrangeRed,Colors.SeaGreen,Colors.DarkViolet,Colors.DarkGoldenrod,Colors.Teal,Colors.DeepPink,Colors.SlateBlue,Colors.SaddleBrown,Colors.OliveDrab};

        public TelemetryPlotTester()
        {
            InitializeComponent(); Plot.Series=series;
            debounce.Tick+=async(_,_)=>{debounce.Stop();await QueryAsync();};
        }
        private async void OnLoaded(object sender,RoutedEventArgs e)
        {
            if(DesignerProperties.GetIsInDesignMode(this))return;
            loaded=true;
            if(reader is null)await RefreshRecentAsync();else await QueryAsync();
        }
        private void OnUnloaded(object sender,RoutedEventArgs e)
        {
            loaded=false;debounce.Stop();cancellation?.Cancel();request++;
        }
        private async void OnRecent(object sender,RoutedEventArgs e)=>await RefreshRecentAsync();
        private async Task RefreshRecentAsync()
        {
            int listRequest = request;
            try
            {
                string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Lincsolution","LibraryTester","Telemetry");
                var files=await Task.Run(()=>Directory.Exists(root)?Directory.EnumerateFiles(root,"telemetry.db",SearchOption.AllDirectories)
                    .Select(p=>new FileInfo(p)).OrderByDescending(f=>f.LastWriteTimeUtc).Take(100).Select(f=>new FileEntry(f.FullName,f.Directory!.Name)).ToArray():Array.Empty<FileEntry>());
                if(!loaded || listRequest!=request)return;
                updating=true; RecentFiles.ItemsSource=files;RecentFiles.SelectedIndex=files.Length>0?0:-1;updating=false;
                if(files.Length>0)await OpenAsync(files[0].Path);else StatusText.Text="테스트 DB가 없습니다. DB 파일 열기를 사용하세요.";
            }
            catch(Exception error){updating=false;StatusText.Text="파일 목록 오류: "+error.Message;}
        }
        private async void OnRecentSelected(object sender,SelectionChangedEventArgs e)
        {if(!updating&&RecentFiles.SelectedItem is FileEntry file)await OpenAsync(file.Path);}
        private async void OnOpen(object sender,RoutedEventArgs e)
        {
            var dialog=new OpenFileDialog{Filter="Telemetry DB|*.db;*.sqlite;*.sqlite3|모든 파일|*.*",CheckFileExists=true};
            if(dialog.ShowDialog()==true)await OpenAsync(dialog.FileName);
        }

        /// <summary>파일 메타데이터만 먼저 읽고 최초 3개 센서 선택. 전체 원본은 가져오지 않음.</summary>
        private async Task OpenAsync(string path)
        {
            debounce.Stop();cancellation?.Cancel();int version=++request;
            var source=new CancellationTokenSource();cancellation=source;
            reader=null;QueryPanel.IsEnabled=false;PathBox.Text=path;StatusText.Text="세션/채널 조회 중…";
            foreach(var item in series)item.PropertyChanged-=OnSeriesChanged;
            series.Clear();
            try
            {
                var newReader=new TelemetryQueryReader(path);
                await queryGate.WaitAsync(source.Token);
                TelemetrySessionInfo info;IReadOnlyList<TelemetryChannel> channels;
                try
                {
                    var result=await Task.Run(()=>(newReader.ReadSession(source.Token),newReader.ReadChannels(source.Token)),source.Token);
                    info=result.Item1;channels=result.Item2;
                }
                finally{queryGate.Release();}
                if(version!=request||!loaded)return;
                reader=newReader;lastSeconds=Math.Max(0.001,info.LastElapsed.TotalSeconds);
                FromBox.Text="0";ToBox.Text=lastSeconds.ToString("0.#######",CultureInfo.InvariantCulture);
                Plot.RangeStart=0;Plot.RangeEnd=lastSeconds;
                updating=true;
                foreach(var channel in channels)
                {
                    var brush=new SolidColorBrush(Palette[series.Count%Palette.Length]);brush.Freeze();
                    var item=new PlotSeries(channel.ChannelId,channel.Name,channel.Unit,brush,series.Count<3,channel.ValueType==TelemetryValueType.Boolean);
                    item.PropertyChanged+=OnSeriesChanged;series.Add(item);
                }
                updating=false;QueryPanel.IsEnabled=true;
                SessionText.Text=$"{info.FrameCount:N0} Frames · {info.LastElapsed.TotalMinutes:N1}분 · {info.EndReason??"종료 기록 없음"}";
            }
            catch(OperationCanceledException){if(version==request)StatusText.Text="조회 취소";}
            catch(Exception error){if(version==request){StatusText.Text="파일 조회 실패: "+error.Message;SessionText.Text="파일을 확인하세요.";}}
            finally{if(ReferenceEquals(cancellation,source))cancellation=null;source.Dispose();}
            if(version==request && reader!=null)await QueryAsync();
        }

        private void OnSeriesChanged(object? sender,PropertyChangedEventArgs e)
        {
            if(updating||e.PropertyName!=nameof(PlotSeries.IsVisible))return;
            cancellation?.Cancel();request++;debounce.Stop();
            // 이전 선택/범위의 점을 최신 조회 결과로 오해하지 않도록 제거.
            foreach(var item in series)item.Points=Array.Empty<PlotPoint>();
            if(loaded){StatusText.Text="선택 변경 · 조회 대기…";debounce.Start();}
        }
        private async void OnApply(object sender,RoutedEventArgs e)
        {
            if(!TryRange(out double from,out double to))return;
            Plot.RangeStart=from;Plot.RangeEnd=to;await QueryAsync();
        }
        private async void OnFullRange(object sender,RoutedEventArgs e)
        {
            FromBox.Text="0";ToBox.Text=lastSeconds.ToString("0.#######",CultureInfo.InvariantCulture);
            Plot.RangeStart=0;Plot.RangeEnd=lastSeconds;await QueryAsync();
        }
        private void OnSelectAll(object sender,RoutedEventArgs e)=>Select(true);
        private void OnClearSelection(object sender,RoutedEventArgs e)=>Select(false);
        private void Select(bool value)
        {
            updating=true;int index=0;foreach(var item in series)item.IsVisible=value&&index++<100;updating=false;
            OnSeriesChanged(this,new PropertyChangedEventArgs(nameof(PlotSeries.IsVisible)));
        }
        private void OnCancel(object sender,RoutedEventArgs e)
        {
            debounce.Stop();cancellation?.Cancel();request++;StatusText.Text="조회 취소 · 조건을 바꾸거나 범위 적용을 누르세요.";
        }
        private bool TryRange(out double from,out double to)
        {
            bool validFrom=double.TryParse(FromBox.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out from);
            bool validTo=double.TryParse(ToBox.Text,NumberStyles.Float,CultureInfo.InvariantCulture,out to);
            if(!validFrom||!validTo||!double.IsFinite(from)||!double.IsFinite(to)||from<0||to<=from||to>TimeSpan.MaxValue.TotalSeconds-1)
            {StatusText.Text="시간 범위 오류: 0 ≤ 시작 < 종료인 초 값을 입력하세요.";return false;}return true;
        }

        /// <summary>선택 채널만 조회. 폭에 따른 점수와 전체 120,000점 상한을 함께 적용.</summary>
        private async Task QueryAsync()
        {
            if(reader is null||!loaded)return;
            debounce.Stop();cancellation?.Cancel();int version=++request;
            var selected=series.Where(s=>s.IsVisible).ToArray();
            foreach(var item in series)item.Points=Array.Empty<PlotPoint>();
            if(selected.Length==0){StatusText.Text="표시할 센서를 선택하세요.";return;}
            if(selected.Length>100){StatusText.Text="한 번에 최대 100개 센서를 선택하세요.";return;}
            var source=new CancellationTokenSource();cancellation=source;
            var queryReader=reader;double from=Plot.RangeStart,to=Plot.RangeEnd;
            int budget=Math.Min(6000,Math.Max(120,(int)Math.Max(600,Plot.ActualWidth)*2));
            budget=Math.Min(budget,120000/selected.Length);
            StatusText.Text=$"{selected.Length}개 센서 조회 중 · 센서당 최대 {budget:N0}점…";
            try
            {
                await queryGate.WaitAsync(source.Token);
                IReadOnlyList<TelemetryPlotSeries> results;
                try
                {
                    results=await Task.Run(()=>queryReader.ReadPlot(selected.Select(s=>s.Id),TimeSpan.FromSeconds(from),TimeSpan.FromSeconds(to),budget,source.Token),source.Token);
                }
                finally{queryGate.Release();}
                if(version!=request||!loaded)return;
                var map=results.ToDictionary(r=>r.Channel.ChannelId);
                foreach(var item in selected)item.Points=map[item.Id].Points.Select(p=>new PlotPoint(p.Timestamp.Elapsed.TotalSeconds,p.Value)).ToArray();
                long raw=results.Sum(r=>r.SourceCount);int shown=results.Sum(r=>r.Points.Count);
                StatusText.Text=$"조회 완료 · {selected.Length}센서 · 원본 {raw:N0}점 → 표시 {shown:N0}점 · "+
                    (results.Any(r=>r.IsReduced)?"축약 표시: 상세 전환은 범위를 좁혀 확인하세요.":"원본 전체 표시");
            }
            catch(OperationCanceledException){if(version==request)StatusText.Text="조회 취소";}
            catch(Exception error){if(version==request)StatusText.Text="조회 실패: "+error.Message;}
            finally{if(ReferenceEquals(cancellation,source))cancellation=null;source.Dispose();}
        }
        private sealed record FileEntry(string Path,string Name);
    }
}
