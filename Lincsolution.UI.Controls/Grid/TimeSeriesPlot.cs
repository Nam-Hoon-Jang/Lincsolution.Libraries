using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 센서 체크박스와 공통 시간축을 가진 재사용 Plot. DB 또는 Telemetry 라이브러리에 의존하지 않습니다.
    /// EN: Reusable checkbox-selected time-series plot with no database or Telemetry dependency.
    /// </summary>
    [TemplatePart(Name="PART_PlotHost",Type=typeof(System.Windows.Controls.Panel))]
    [LocalizedDescription("센서 선택형 시간축 Plot", "Checkbox-selected time-series plot")]
    public partial class TimeSeriesPlot : Control
    {
        private PlotSurface? surface;
        private System.Windows.Controls.Panel? host;
        private INotifyCollectionChanged? collection;
        private readonly List<PlotSeries> observed=new();
        private static readonly IEnumerable<PlotSeries> SeriesDefault=Array.Empty<PlotSeries>();
        private static readonly Brush GridBrushDefault=Brushes.LightGray;
        private static readonly Brush AxisBrushDefault=Brushes.Gray;
        private static readonly Brush PlotBackgroundDefault=Brushes.Transparent;
        private static readonly Brush HighLightBorderBrushDefault=Brushes.SteelBlue;
        private static readonly Thickness PlotPaddingDefault=new(65,18,18,48);

        static TimeSeriesPlot() { DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSeriesPlot),new FrameworkPropertyMetadata(typeof(TimeSeriesPlot))); }
        /// <summary>KO: Plot 초기화. EN: Initializes the plot.</summary>
        public TimeSeriesPlot()
        {
            Loaded+=(_,_)=>Observe(); Unloaded+=(_,_)=>Detach();
        }
        /// <summary>KO: 템플릿의 그리기 영역 연결. EN: Connects the template drawing area.</summary>
        public override void OnApplyTemplate()
        {
            if(host!=null && surface!=null) host.Children.Remove(surface);
            base.OnApplyTemplate(); host=GetTemplateChild("PART_PlotHost") as System.Windows.Controls.Panel;
            surface=new PlotSurface(this); host?.Children.Add(surface); Observe();
        }
        /// <summary>KO: 상속된 글꼴/전경 속성 변경도 다시 그립니다. EN: Redraws inherited font and foreground changes.</summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            surface?.InvalidatePlot();
        }
        private static void OnVisualChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)=>((TimeSeriesPlot)d).surface?.InvalidatePlot();
        private static void OnSeriesChanged(DependencyObject d,DependencyPropertyChangedEventArgs e)=>((TimeSeriesPlot)d).Observe();
        private void Detach()
        {
            if(collection!=null) collection.CollectionChanged-=CollectionChanged;
            collection=null; foreach(var item in observed)item.PropertyChanged-=SeriesChanged; observed.Clear();
        }
        private void Observe()
        {
            Detach();
            if(Series!=null)
            {
                collection=Series as INotifyCollectionChanged;
                if(collection!=null)collection.CollectionChanged+=CollectionChanged;
                foreach(var item in Series) { observed.Add(item); item.PropertyChanged+=SeriesChanged; }
            }
            surface?.InvalidatePlot();
        }
        private void CollectionChanged(object? sender,NotifyCollectionChangedEventArgs e)=>Observe();
        private void SeriesChanged(object? sender,PropertyChangedEventArgs e)=>surface?.InvalidatePlot();

        private interface IDependencyProperties
        {
            /// <summary>KO: 센서 목록. EN: Sensor series collection.</summary>
            [LocalizedDescription("센서 목록", "Sensor series collection")]
            [DependencyProperty(ChangedCallback=nameof(OnSeriesChanged))] IEnumerable<PlotSeries> Series { get; }
            /// <summary>KO: 제목. EN: Plot title.</summary>
            [LocalizedDescription("Plot 제목", "Plot title")]
            [DependencyProperty(DefaultValue="Time Series")] string Title { get; }
            /// <summary>KO: X축 시작 경과 초. EN: X-axis start seconds.</summary>
            [LocalizedDescription("시작 경과 초", "Start seconds")]
            [DependencyProperty(DefaultValue=0.0,ChangedCallback=nameof(OnVisualChanged))] double RangeStart { get; }
            /// <summary>KO: X축 끝 경과 초. EN: X-axis end seconds.</summary>
            [LocalizedDescription("끝 경과 초", "End seconds")]
            [DependencyProperty(DefaultValue=60.0,ChangedCallback=nameof(OnVisualChanged))] double RangeEnd { get; }
            /// <summary>KO: Y축 최솟값. NaN이면 자동. EN: Minimum Y; NaN selects automatic scaling.</summary>
            [LocalizedDescription("Y축 최소 (NaN=자동)", "Minimum Y (NaN=automatic)")]
            [DependencyProperty(DefaultValue=double.NaN,ChangedCallback=nameof(OnVisualChanged))] double MinimumY { get; }
            /// <summary>KO: Y축 최댓값. NaN이면 자동. EN: Maximum Y; NaN selects automatic scaling.</summary>
            [LocalizedDescription("Y축 최대 (NaN=자동)", "Maximum Y (NaN=automatic)")]
            [DependencyProperty(DefaultValue=double.NaN,ChangedCallback=nameof(OnVisualChanged))] double MaximumY { get; }
            /// <summary>KO: 센서별 값을 0~1로 정규화. EN: Normalizes each series to 0–1.</summary>
            [LocalizedDescription("센서별 0~1 정규화", "Normalize each series to 0–1")]
            [DependencyProperty(DefaultValue=false,ChangedCallback=nameof(OnVisualChanged))] bool NormalizeSeries { get; }
            /// <summary>KO: 선 두께. EN: Series line thickness.</summary>
            [LocalizedDescription("선 두께", "Line thickness")]
            [DependencyProperty(DefaultValue=1.5,ChangedCallback=nameof(OnVisualChanged))] double LineThickness { get; }
            /// <summary>KO: 격자 표시. EN: Shows grid lines.</summary>
            [LocalizedDescription("격자 표시", "Show grid lines")]
            [DependencyProperty(DefaultValue=true,ChangedCallback=nameof(OnVisualChanged))] bool ShowGrid { get; }
            /// <summary>KO: 격자 색. EN: Grid brush.</summary>
            [LocalizedDescription("격자 색", "Grid brush")]
            [DependencyProperty(ChangedCallback=nameof(OnVisualChanged))] Brush GridBrush { get; }
            /// <summary>KO: 축 색. EN: Axis brush.</summary>
            [LocalizedDescription("축 색", "Axis brush")]
            [DependencyProperty(ChangedCallback=nameof(OnVisualChanged))] Brush AxisBrush { get; }
            /// <summary>KO: Plot 내부 배경. EN: Drawing-area background.</summary>
            [LocalizedDescription("Plot 배경", "Plot background")]
            [DependencyProperty(ChangedCallback=nameof(OnVisualChanged))] Brush PlotBackground { get; }
            /// <summary>KO: 축 라벨 여백. EN: Axis label margins.</summary>
            [LocalizedDescription("축 라벨 여백", "Axis label margins")]
            [DependencyProperty(ChangedCallback=nameof(OnVisualChanged))] Thickness PlotPadding { get; }
            /// <summary>KO: 센서 선택 영역 너비. EN: Sensor selector width.</summary>
            [LocalizedDescription("센서 선택 영역 너비", "Sensor selector width")]
            [DependencyProperty(DefaultValue=210.0)] double SelectorWidth { get; }
            /// <summary>KO: 센서 선택 영역 표시. EN: Shows the sensor selector.</summary>
            [LocalizedDescription("센서 선택 영역 표시", "Show sensor selector")]
            [DependencyProperty(DefaultValue=true)] bool ShowSelector { get; }
            /// <summary>KO: 센서 항목 템플릿. EN: Sensor item template.</summary>
            [LocalizedDescription("센서 항목 템플릿", "Sensor item template")]
            [DependencyProperty] DataTemplate SeriesItemTemplate { get; }
            /// <summary>KO: 모서리 반경. EN: Border corner radius.</summary>
            [LocalizedDescription("모서리 반경", "Corner radius")]
            [DependencyProperty] CornerRadius CornerRadius { get; }
            /// <summary>KO: 마우스 진입 테두리 색. EN: Hover border brush.</summary>
            [LocalizedDescription("강조 테두리 색", "Hover border brush")]
            [DependencyProperty] Brush HighLightBorderBrush { get; }
            /// <summary>KO: 비활성 불투명도. EN: Disabled opacity.</summary>
            [LocalizedDescription("비활성 불투명도", "Disabled opacity")]
            [DependencyProperty(DefaultValue=0.5)] double DisabledOpacity { get; }
            /// <summary>KO: 빈 Plot 안내. EN: Empty-plot message.</summary>
            [LocalizedDescription("빈 Plot 안내", "Empty plot message")]
            [DependencyProperty(DefaultValue="표시할 센서를 선택하세요.",ChangedCallback=nameof(OnVisualChanged))] string EmptyText { get; }
        }

        /// <summary>픽셀 그리기 전용 요소. 점마다 UIElement를 생성하지 않고 StreamGeometry로 한 번에 그림.</summary>
        private sealed class PlotSurface : FrameworkElement
        {
            private readonly TimeSeriesPlot owner;
            private double? cursorX;
            private Rect plotRect;
            private DrawingGroup? drawing;
            private bool dirty=true;
            // 곡선은 데이터/스타일/크기가 바뀔 때만 재계산. 커서 이동은 기존 Drawing을 재사용.
            internal void InvalidatePlot() { dirty=true; InvalidateVisual(); }
            internal PlotSurface(TimeSeriesPlot owner)
            {
                this.owner=owner; ClipToBounds=true;
                SizeChanged+=(_,_)=>InvalidatePlot();
                MouseMove+=OnMove; MouseLeave+=(_,_)=>{cursorX=null;ToolTip=null;InvalidateVisual();};
            }
            private void Text(DrawingContext dc,string text,double x,double y,Brush? brush=null)
            {
                var label=new FormattedText(text,CultureInfo.CurrentCulture,FlowDirection.LeftToRight,
                    new Typeface(owner.FontFamily,owner.FontStyle,owner.FontWeight,owner.FontStretch),
                    Math.Clamp(owner.FontSize,9,30),brush??owner.Foreground??Brushes.Black,VisualTreeHelper.GetDpi(this).PixelsPerDip);
                dc.DrawText(label,new Point(x,y));
            }
            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
                if(dirty || drawing is null)
                {
                    drawing=new DrawingGroup();
                    using(var context=drawing.Open()) DrawPlot(context);
                    dirty=false;
                }
                dc.DrawDrawing(drawing);
                if(cursorX.HasValue && plotRect.Width>0)
                {
                    dc.PushClip(new RectangleGeometry(plotRect));
                    dc.DrawLine(new Pen(owner.AxisBrush??Brushes.Gray,1),new Point(cursorX.Value,plotRect.Top),new Point(cursorX.Value,plotRect.Bottom));
                    dc.Pop();
                }
            }
            private void DrawPlot(DrawingContext dc)
            {
                dc.DrawRectangle(owner.PlotBackground??Brushes.Transparent,null,new Rect(RenderSize));
                var pad=owner.PlotPadding;
                plotRect=new Rect(Math.Max(0,pad.Left),Math.Max(0,pad.Top),Math.Max(0,ActualWidth-pad.Left-pad.Right),Math.Max(0,ActualHeight-pad.Top-pad.Bottom));
                if(plotRect.Width<40||plotRect.Height<40)return;
                double from=owner.RangeStart,to=owner.RangeEnd;
                if(!double.IsFinite(from)||!double.IsFinite(to)||to<=from){Text(dc,"유효한 시간 범위를 지정하세요.",plotRect.Left,plotRect.Top);return;}
                var selected=owner.observed.Where(s=>s.IsVisible).ToArray();
                // UI 직접 사용 시에도 상한 초과 데이터를 조용히 잘라 그리지 않음.
                // Core ReadPlot 또는 호출 측의 축약으로 표시 점수를 줄여야 함.
                if(selected.Length>100 || selected.Any(s=>s.Points.Count>6000) || selected.Sum(s=>(long)s.Points.Count)>120000)
                {Text(dc,"표시 한도 초과: 시간 범위 또는 조회 점수를 줄이세요.",plotRect.Left,plotRect.Top);return;}
                var ranges=new Dictionary<PlotSeries,(double Min,double Max)>();
                foreach(var s in selected)
                {
                    double lo=double.PositiveInfinity,hi=double.NegativeInfinity;
                    foreach(var p in s.Points)
                        if(p.Seconds>=from&&p.Seconds<=to&&p.Value.HasValue&&double.IsFinite(p.Value.Value)) {lo=Math.Min(lo,p.Value.Value);hi=Math.Max(hi,p.Value.Value);}
                    if(double.IsFinite(lo)) ranges[s]=(lo,hi);
                }
                double ymin=owner.NormalizeSeries?0:ranges.Count>0?ranges.Values.Min(v=>v.Min):0;
                double ymax=owner.NormalizeSeries?1:ranges.Count>0?ranges.Values.Max(v=>v.Max):1;
                if(!owner.NormalizeSeries)
                {
                    double margin=Math.Max((ymax-ymin)*0.05,Math.Max(Math.Abs(ymax)*0.01,0.01));
                    ymin=double.IsFinite(owner.MinimumY)?owner.MinimumY:ymin-margin;
                    ymax=double.IsFinite(owner.MaximumY)?owner.MaximumY:ymax+margin;
                }
                if(!double.IsFinite(ymax-ymin)||ymax<=ymin){Text(dc,"유효한 Y축 범위를 지정하세요.",plotRect.Left,plotRect.Top);return;}
                var axis=new Pen(owner.AxisBrush??Brushes.Gray,1);var grid=new Pen(owner.GridBrush??Brushes.LightGray,0.5);
                for(int i=0;i<=5;i++)
                {
                    double x=plotRect.Left+plotRect.Width*i/5,y=plotRect.Bottom-plotRect.Height*i/5;
                    if(owner.ShowGrid){dc.DrawLine(grid,new Point(x,plotRect.Top),new Point(x,plotRect.Bottom));dc.DrawLine(grid,new Point(plotRect.Left,y),new Point(plotRect.Right,y));}
                    Text(dc,(from+(to-from)*i/5).ToString("0.##"),x-12,plotRect.Bottom+6);
                    Text(dc,(ymin+(ymax-ymin)*i/5).ToString("G4"),2,y-8);
                }
                dc.DrawRectangle(null,axis,plotRect);Text(dc,"경과 시간 (s)",plotRect.Left+plotRect.Width/2-30,plotRect.Bottom+26);
                dc.PushClip(new RectangleGeometry(plotRect));
                foreach(var series in selected)
                {
                    if(!ranges.TryGetValue(series,out var range))continue;
                    var geometry=new StreamGeometry();
                    using(var context=geometry.Open())
                    {
                        bool begin=true;int segment=0;Point previous=default;
                        foreach(var point in series.Points)
                        {
                            if(!point.Value.HasValue||!double.IsFinite(point.Value.Value)||point.Seconds<from||point.Seconds>to)
                            {if(segment==1)dc.DrawEllipse(series.Stroke,null,previous,2,2);begin=true;segment=0;continue;}
                            double value=point.Value.Value;
                            if(owner.NormalizeSeries)value=range.Max==range.Min?0.5:(value-range.Min)/(range.Max-range.Min);
                            var screen=new Point(plotRect.Left+(point.Seconds-from)/(to-from)*plotRect.Width,plotRect.Bottom-(value-ymin)/(ymax-ymin)*plotRect.Height);
                            if(!double.IsFinite(screen.X)||!double.IsFinite(screen.Y)){begin=true;segment=0;continue;}
                            if(begin){context.BeginFigure(screen,false,false);begin=false;}
                            else
                            {
                                // Boolean/상태 값은 다음 측정 시각까지 이전 값을 유지하는 계단 형태.
                                if(series.IsStep)context.LineTo(new Point(screen.X,previous.Y),true,false);
                                context.LineTo(screen,true,false);
                            }
                            previous=screen;segment++;
                        }
                        if(segment==1)dc.DrawEllipse(series.Stroke,null,previous,2,2);
                    }
                    geometry.Freeze();double width=double.IsFinite(owner.LineThickness)?Math.Clamp(owner.LineThickness,0.25,10):1.5;
                    dc.DrawGeometry(null,new Pen(series.Stroke,width),geometry);
                }
                dc.Pop();
                if(ranges.Count==0)Text(dc,owner.EmptyText??string.Empty,plotRect.Left+12,plotRect.Top+12);
            }
            private void OnMove(object sender,MouseEventArgs e)
            {
                var point=e.GetPosition(this); if(!plotRect.Contains(point)||plotRect.Width<=0)return;
                cursorX=point.X; double time=owner.RangeStart+(point.X-plotRect.Left)/plotRect.Width*(owner.RangeEnd-owner.RangeStart);
                var lines=new List<string>{"표시 점 기준 · "+time.ToString("0.000")+" s"};
                foreach(var s in owner.observed.Where(s=>s.IsVisible&&s.Points.Count>0).Take(8))
                {
                    // 조회 결과는 시간순. 커서마다 전체 점을 순회하지 않고 이진 검색.
                    int lo=0,hi=s.Points.Count-1;
                    while(lo<hi){int mid=(lo+hi)/2;if(s.Points[mid].Seconds<time)lo=mid+1;else hi=mid;}
                    if(lo>0 && Math.Abs(s.Points[lo-1].Seconds-time)<Math.Abs(s.Points[lo].Seconds-time))lo--;
                    var p=s.Points[lo];lines.Add(s.Name+": "+(p.Value?.ToString("G7")??"값 없음")+" "+s.Unit+" @ "+p.Seconds.ToString("0.000")+"s");
                }
                ToolTip=string.Join(Environment.NewLine,lines);InvalidateVisual();
            }
        }
    }
}
