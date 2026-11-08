using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 실제 좌표와 목표 좌표를 좌표평면에 표시하는 컨트롤입니다.
    /// EN: A control that plots actual and target positions on a coordinate plane.
    /// </summary>
    [LocalizedDescription("실제 좌표와 목표 좌표를 좌표평면에 표시하는 컨트롤입니다.", "A control that plots actual and target positions on a coordinate plane.")]
    [TemplatePart(Name = GridCanvasPartName, Type = typeof(Canvas))]
    [TemplatePart(Name = OverlayCanvasPartName, Type = typeof(Canvas))]
    public class CoordinateMap : Control
    {
        private const string GridCanvasPartName = "PART_GridCanvas";
        private const string OverlayCanvasPartName = "PART_OverlayCanvas";

        private Canvas? _gridCanvas;
        private Canvas? _overlayCanvas;
        private Ellipse? _realDot;
        private Ellipse? _targetDot;

        /// <summary>
        /// KO: CoordinateMap 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the CoordinateMap control.
        /// </summary>
        static CoordinateMap()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CoordinateMap),
                new FrameworkPropertyMetadata(typeof(CoordinateMap)));
        }

        /// <summary>
        /// KO: CoordinateMap 인스턴스를 초기화하고 크기 변경 시 다시 그리도록 등록합니다.
        /// EN: Initializes a CoordinateMap instance and registers redraw behavior for size changes.
        /// </summary>
        public CoordinateMap()
        {
            SizeChanged += (_, _) => Redraw();
        }

        /// <summary>
        /// KO: RealPosition 종속성 속성 식별자입니다. 표시할 실제 좌표를 가져오거나 설정합니다.
        /// EN: Identifies the RealPosition dependency property. Gets or sets the actual position to display.
        /// </summary>
        public static readonly DependencyProperty RealPositionProperty = Register(nameof(RealPosition), new Point(), OnPositionChanged);
        /// <summary>
        /// KO: TargetPosition 종속성 속성 식별자입니다. 표시할 목표 좌표를 가져오거나 설정합니다.
        /// EN: Identifies the TargetPosition dependency property. Gets or sets the target position to display.
        /// </summary>
        public static readonly DependencyProperty TargetPositionProperty = Register(nameof(TargetPosition), new Point(), OnPositionChanged);
        /// <summary>
        /// KO: GridSpacing 종속성 속성 식별자입니다. 좌표 격자선 사이의 논리 간격을 가져오거나 설정합니다.
        /// EN: Identifies the GridSpacing dependency property. Gets or sets the logical spacing between grid lines.
        /// </summary>
        public static readonly DependencyProperty GridSpacingProperty = Register(nameof(GridSpacing), 50d, OnGridChanged);
        /// <summary>
        /// KO: LogicalMinimum 종속성 속성 식별자입니다. 좌표축의 최소 논리값을 가져오거나 설정합니다.
        /// EN: Identifies the LogicalMinimum dependency property. Gets or sets the minimum logical coordinate value.
        /// </summary>
        public static readonly DependencyProperty LogicalMinimumProperty = Register(nameof(LogicalMinimum), -425d, OnGridChanged);
        /// <summary>
        /// KO: LogicalMaximum 종속성 속성 식별자입니다. 좌표축의 최대 논리값을 가져오거나 설정합니다.
        /// EN: Identifies the LogicalMaximum dependency property. Gets or sets the maximum logical coordinate value.
        /// </summary>
        public static readonly DependencyProperty LogicalMaximumProperty = Register(nameof(LogicalMaximum), 425d, OnGridChanged);
        /// <summary>
        /// KO: GridLineBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the GridLineBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty GridLineBrushProperty = Register<Brush>(nameof(GridLineBrush), Brushes.LightGray, OnGridChanged);
        /// <summary>
        /// KO: GridLineThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the GridLineThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty GridLineThicknessProperty = Register(nameof(GridLineThickness), 0.5d, OnGridChanged);
        /// <summary>
        /// KO: OriginBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the OriginBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty OriginBrushProperty = Register<Brush>(nameof(OriginBrush), Brushes.Black, OnGridChanged);
        /// <summary>
        /// KO: OriginDotSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the OriginDotSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty OriginDotSizeProperty = Register(nameof(OriginDotSize), 4d, OnGridChanged);

        /// <summary>
        /// KO: RealDotSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the RealDotSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty RealDotSizeProperty = Register(nameof(RealDotSize), 8d, OnDotChanged);
        /// <summary>
        /// KO: RealDotFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the RealDotFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty RealDotFillProperty = Register<Brush>(nameof(RealDotFill), Brushes.DodgerBlue, OnDotChanged);
        /// <summary>
        /// KO: RealDotStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the RealDotStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty RealDotStrokeProperty = Register<Brush>(nameof(RealDotStroke), Brushes.Transparent, OnDotChanged);
        /// <summary>
        /// KO: RealDotStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the RealDotStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty RealDotStrokeThicknessProperty = Register(nameof(RealDotStrokeThickness), 0d, OnDotChanged);
        /// <summary>
        /// KO: TargetDotSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TargetDotSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty TargetDotSizeProperty = Register(nameof(TargetDotSize), 10d, OnDotChanged);
        /// <summary>
        /// KO: TargetDotFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TargetDotFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TargetDotFillProperty = Register<Brush>(nameof(TargetDotFill), Brushes.Transparent, OnDotChanged);
        /// <summary>
        /// KO: TargetDotStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TargetDotStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TargetDotStrokeProperty = Register<Brush>(nameof(TargetDotStroke), Brushes.Black, OnDotChanged);
        /// <summary>
        /// KO: TargetDotStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TargetDotStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty TargetDotStrokeThicknessProperty = Register(nameof(TargetDotStrokeThickness), 1d, OnDotChanged);
        /// <summary>
        /// KO: TrailDuration 종속성 속성 식별자입니다. 실제 좌표 이동 흔적이 사라지는 시간을 가져오거나 설정합니다.
        /// EN: Identifies the TrailDuration dependency property. Gets or sets how long the actual-position trail remains visible.
        /// </summary>
        public static readonly DependencyProperty TrailDurationProperty = Register(nameof(TrailDuration), TimeSpan.FromSeconds(3));
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = Register(nameof(CornerRadius), new CornerRadius(0));

        /// <summary>
        /// KO: 표시할 실제 좌표를 가져오거나 설정합니다.
        /// EN: Gets or sets the actual position to display.
        /// </summary>
        [LocalizedDescription("표시할 실제 좌표를 가져오거나 설정합니다.", "Gets or sets the actual position to display.")]
        public Point RealPosition { get => (Point)GetValue(RealPositionProperty); set => SetValue(RealPositionProperty, value); }
        /// <summary>
        /// KO: 표시할 목표 좌표를 가져오거나 설정합니다.
        /// EN: Gets or sets the target position to display.
        /// </summary>
        [LocalizedDescription("표시할 목표 좌표를 가져오거나 설정합니다.", "Gets or sets the target position to display.")]
        public Point TargetPosition { get => (Point)GetValue(TargetPositionProperty); set => SetValue(TargetPositionProperty, value); }
        /// <summary>
        /// KO: 좌표 격자선 사이의 논리 간격을 가져오거나 설정합니다.
        /// EN: Gets or sets the logical spacing between grid lines.
        /// </summary>
        [LocalizedDescription("좌표 격자선 사이의 논리 간격을 가져오거나 설정합니다.", "Gets or sets the logical spacing between grid lines.")]
        public double GridSpacing { get => (double)GetValue(GridSpacingProperty); set => SetValue(GridSpacingProperty, value); }
        /// <summary>
        /// KO: 좌표축의 최소 논리값을 가져오거나 설정합니다.
        /// EN: Gets or sets the minimum logical coordinate value.
        /// </summary>
        [LocalizedDescription("좌표축의 최소 논리값을 가져오거나 설정합니다.", "Gets or sets the minimum logical coordinate value.")]
        public double LogicalMinimum { get => (double)GetValue(LogicalMinimumProperty); set => SetValue(LogicalMinimumProperty, value); }
        /// <summary>
        /// KO: 좌표축의 최대 논리값을 가져오거나 설정합니다.
        /// EN: Gets or sets the maximum logical coordinate value.
        /// </summary>
        [LocalizedDescription("좌표축의 최대 논리값을 가져오거나 설정합니다.", "Gets or sets the maximum logical coordinate value.")]
        public double LogicalMaximum { get => (double)GetValue(LogicalMaximumProperty); set => SetValue(LogicalMaximumProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush GridLineBrush { get => (Brush)GetValue(GridLineBrushProperty); set => SetValue(GridLineBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double GridLineThickness { get => (double)GetValue(GridLineThicknessProperty); set => SetValue(GridLineThicknessProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush OriginBrush { get => (Brush)GetValue(OriginBrushProperty); set => SetValue(OriginBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double OriginDotSize { get => (double)GetValue(OriginDotSizeProperty); set => SetValue(OriginDotSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double RealDotSize { get => (double)GetValue(RealDotSizeProperty); set => SetValue(RealDotSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush RealDotFill { get => (Brush)GetValue(RealDotFillProperty); set => SetValue(RealDotFillProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush RealDotStroke { get => (Brush)GetValue(RealDotStrokeProperty); set => SetValue(RealDotStrokeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double RealDotStrokeThickness { get => (double)GetValue(RealDotStrokeThicknessProperty); set => SetValue(RealDotStrokeThicknessProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double TargetDotSize { get => (double)GetValue(TargetDotSizeProperty); set => SetValue(TargetDotSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush TargetDotFill { get => (Brush)GetValue(TargetDotFillProperty); set => SetValue(TargetDotFillProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush TargetDotStroke { get => (Brush)GetValue(TargetDotStrokeProperty); set => SetValue(TargetDotStrokeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double TargetDotStrokeThickness { get => (double)GetValue(TargetDotStrokeThicknessProperty); set => SetValue(TargetDotStrokeThicknessProperty, value); }
        /// <summary>
        /// KO: 실제 좌표 이동 흔적이 사라지는 시간을 가져오거나 설정합니다.
        /// EN: Gets or sets how long the actual-position trail remains visible.
        /// </summary>
        [LocalizedDescription("실제 좌표 이동 흔적이 사라지는 시간을 가져오거나 설정합니다.", "Gets or sets how long the actual-position trail remains visible.")]
        public TimeSpan TrailDuration { get => (TimeSpan)GetValue(TrailDurationProperty); set => SetValue(TrailDurationProperty, value); }
        /// <summary>
        /// KO: 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius of the control.
        /// </summary>
        [LocalizedDescription("컨트롤 모서리의 반경을 가져오거나 설정합니다.", "Gets or sets the corner radius of the control.")]
        public CornerRadius CornerRadius { get => (CornerRadius)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }

        /// <summary>
        /// KO: 컨트롤 템플릿을 적용하고 필요한 템플릿 요소와 표시 상태를 초기화합니다.
        /// EN: Applies the control template and initializes the required template parts and visual state.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _gridCanvas = GetTemplateChild(GridCanvasPartName) as Canvas;
            _overlayCanvas = GetTemplateChild(OverlayCanvasPartName) as Canvas;
            InitializeDots();
            Redraw();
        }

        /// <summary>
        /// KO: 좌표 오버레이의 실제 위치점과 목표 위치점을 초기화합니다.
        /// EN: Initializes the actual and target position dots on the coordinate overlay.
        /// </summary>
        private void InitializeDots()
        {
            if (_overlayCanvas is null)
                return;

            _overlayCanvas.Children.Clear();
            _realDot = new Ellipse();
            _targetDot = new Ellipse();
            _overlayCanvas.Children.Add(_realDot);
            _overlayCanvas.Children.Add(_targetDot);
            UpdateDotStyles();
        }

        /// <summary>
        /// KO: 좌표 격자와 위치점 전체를 다시 그립니다.
        /// EN: Redraws the coordinate grid and all position dots.
        /// </summary>
        private void Redraw()
        {
            DrawGrid();
            UpdateDotPositions(false);
        }

        /// <summary>
        /// KO: 현재 논리 범위와 간격에 맞춰 좌표 격자와 원점을 그립니다.
        /// EN: Draws the coordinate grid and origin for the current logical range and spacing.
        /// </summary>
        private void DrawGrid()
        {
            if (_gridCanvas is null || ActualWidth <= 0 || ActualHeight <= 0)
                return;

            _gridCanvas.Children.Clear();
            if (GridSpacing <= 0 || LogicalMaximum <= LogicalMinimum)
                return;

            for (double value = LogicalMinimum; value <= LogicalMaximum; value += GridSpacing)
            {
                DrawLine(value, LogicalMinimum, value, LogicalMaximum);
                DrawLine(LogicalMinimum, value, LogicalMaximum, value);
            }

            Point origin = MapToScreen(new Point(0, 0));
            double originDotSize = Math.Max(0, OriginDotSize);
            var originDot = new Ellipse
            {
                Width = originDotSize,
                Height = originDotSize,
                Fill = OriginBrush
            };
            Canvas.SetLeft(originDot, origin.X - originDotSize / 2);
            Canvas.SetTop(originDot, origin.Y - originDotSize / 2);
            _gridCanvas.Children.Add(originDot);
        }

        /// <summary>
        /// KO: 논리 좌표 두 점을 화면 좌표로 변환해 격자선을 그립니다.
        /// EN: Converts two logical points to screen coordinates and draws a grid line.
        /// </summary>
        /// <param name="x1">KO: 선 시작점의 논리 X 좌표입니다. EN: The logical X coordinate of the line start.</param>
        /// <param name="y1">KO: 선 시작점의 논리 Y 좌표입니다. EN: The logical Y coordinate of the line start.</param>
        /// <param name="x2">KO: 선 끝점의 논리 X 좌표입니다. EN: The logical X coordinate of the line end.</param>
        /// <param name="y2">KO: 선 끝점의 논리 Y 좌표입니다. EN: The logical Y coordinate of the line end.</param>
        private void DrawLine(double x1, double y1, double x2, double y2)
        {
            if (_gridCanvas is null)
                return;

            Point p1 = MapToScreen(new Point(x1, y1));
            Point p2 = MapToScreen(new Point(x2, y2));
            _gridCanvas.Children.Add(new Line
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y,
                Stroke = GridLineBrush,
                StrokeThickness = GridLineThickness
            });
        }

        /// <summary>
        /// KO: 실제 위치점과 목표 위치점의 크기, 채우기 및 테두리를 갱신합니다.
        /// EN: Updates the size, fill, and stroke of the actual and target position dots.
        /// </summary>
        private void UpdateDotStyles()
        {
            if (_realDot is not null)
            {
                SetDotStyle(_realDot, RealDotSize, RealDotFill, RealDotStroke, RealDotStrokeThickness);
            }

            if (_targetDot is not null)
            {
                SetDotStyle(_targetDot, TargetDotSize, TargetDotFill, TargetDotStroke, TargetDotStrokeThickness);
            }

            UpdateDotPositions(false);
        }

        /// <summary>
        /// KO: 지정한 위치점에 크기, 채우기 및 테두리 스타일을 적용합니다.
        /// EN: Applies size, fill, and stroke styling to the specified position dot.
        /// </summary>
        /// <param name="dot">KO: 변경할 위치점입니다. EN: The position dot to modify.</param>
        /// <param name="size">KO: 위치점 크기입니다. EN: The position dot size.</param>
        /// <param name="fill">KO: 위치점 채우기 브러시입니다. EN: The position dot fill brush.</param>
        /// <param name="stroke">KO: 위치점 테두리 브러시입니다. EN: The position dot stroke brush.</param>
        /// <param name="strokeThickness">KO: 위치점 테두리 두께입니다. EN: The position dot stroke thickness.</param>
        private static void SetDotStyle(Ellipse dot, double size, Brush fill, Brush stroke, double strokeThickness)
        {
            dot.Width = Math.Max(0, size);
            dot.Height = Math.Max(0, size);
            dot.Fill = fill;
            dot.Stroke = stroke;
            dot.StrokeThickness = Math.Max(0, strokeThickness);
        }

        /// <summary>
        /// KO: 실제 위치점과 목표 위치점의 화면 위치를 갱신합니다.
        /// EN: Updates the screen positions of the actual and target position dots.
        /// </summary>
        /// <param name="createTrail">KO: 실제 위치의 이동 흔적 생성 여부입니다. EN: Indicates whether to create a trail for the actual position.</param>
        private void UpdateDotPositions(bool createTrail)
        {
            if (_realDot is not null)
            {
                if (createTrail)
                    CreateTrail(RealPosition);
                SetDotPosition(_realDot, RealPosition);
            }

            if (_targetDot is not null)
                SetDotPosition(_targetDot, TargetPosition);
        }

        /// <summary>
        /// KO: 논리 좌표를 기준으로 위치점의 Canvas 좌표를 설정합니다.
        /// EN: Sets the Canvas coordinates of a position dot from a logical point.
        /// </summary>
        /// <param name="dot">KO: 변경할 위치점입니다. EN: The position dot to modify.</param>
        /// <param name="logical">KO: 변환하거나 표시할 논리 좌표입니다. EN: The logical coordinate to convert or display.</param>
        private void SetDotPosition(Ellipse dot, Point logical)
        {
            Point position = MapToScreen(logical);
            Canvas.SetLeft(dot, position.X - dot.Width / 2);
            Canvas.SetTop(dot, position.Y - dot.Height / 2);
        }

        /// <summary>
        /// KO: 논리 좌표를 컨트롤 내부의 화면 좌표로 변환합니다.
        /// EN: Converts a logical coordinate to a screen coordinate within the control.
        /// </summary>
        /// <param name="logical">KO: 변환하거나 표시할 논리 좌표입니다. EN: The logical coordinate to convert or display.</param>
        /// <returns>KO: 처리 결과를 반환합니다. EN: Returns the operation result.</returns>
        private Point MapToScreen(Point logical)
        {
            double range = LogicalMaximum - LogicalMinimum;
            if (range <= 0)
                return new Point();

            double mapWidth = _gridCanvas?.ActualWidth ?? ActualWidth;
            double mapHeight = _gridCanvas?.ActualHeight ?? ActualHeight;
            double x = (logical.X - LogicalMinimum) * mapWidth / range;
            double y = mapHeight - ((logical.Y - LogicalMinimum) * mapHeight / range);
            return new Point(x, y);
        }

        /// <summary>
        /// KO: 실제 위치에 서서히 사라지는 이동 흔적을 생성합니다.
        /// EN: Creates a fading movement trail at the actual position.
        /// </summary>
        /// <param name="position">KO: 이동 흔적을 생성할 논리 좌표입니다. EN: The logical coordinate where the trail is created.</param>
        private void CreateTrail(Point position)
        {
            if (_overlayCanvas is null || _realDot is null || TrailDuration <= TimeSpan.Zero)
                return;

            var trail = new Ellipse
            {
                Width = _realDot.Width,
                Height = _realDot.Height,
                Fill = RealDotFill,
                Stroke = RealDotStroke,
                StrokeThickness = RealDotStrokeThickness,
                Opacity = 0.5,
                IsHitTestVisible = false
            };

            SetDotPosition(trail, position);
            _overlayCanvas.Children.Insert(0, trail);
            var fade = new DoubleAnimation(0, new Duration(TrailDuration))
            {
                FillBehavior = FillBehavior.Stop
            };
            fade.Completed += (_, _) => _overlayCanvas?.Children.Remove(trail);
            trail.BeginAnimation(OpacityProperty, fade);
        }

        /// <summary>
        /// KO: 실제 또는 목표 좌표 변경을 위치점 표시에 반영합니다.
        /// EN: Applies an actual or target position change to the displayed dots.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var map = (CoordinateMap)d;
            bool realPositionChanged = e.Property == RealPositionProperty;
            map.UpdateDotPositions(realPositionChanged);
        }

        /// <summary>
        /// KO: 격자 관련 속성 변경 시 좌표 지도를 다시 그립니다.
        /// EN: Redraws the coordinate map when a grid-related property changes.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((CoordinateMap)d).Redraw();

        /// <summary>
        /// KO: 위치점 관련 속성 변경 시 점 스타일을 갱신합니다.
        /// EN: Updates dot styling when a dot-related property changes.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnDotChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((CoordinateMap)d).UpdateDotStyles();

        /// <summary>
        /// KO: 현재 컨트롤 형식에 사용할 종속성 속성을 등록합니다.
        /// EN: Registers a dependency property for the current control type.
        /// </summary>
        /// <param name="name">KO: 등록할 종속성 속성 이름입니다. EN: The name of the dependency property to register.</param>
        /// <param name="defaultValue">KO: 종속성 속성의 기본값입니다. EN: The default value of the dependency property.</param>
        /// <param name="callback">KO: 속성값 변경 시 호출할 콜백입니다. EN: The callback invoked when the property value changes.</param>
        /// <returns>KO: 처리 결과를 반환합니다. EN: Returns the operation result.</returns>
        private static DependencyProperty Register<T>(
            string name,
            T defaultValue,
            PropertyChangedCallback? callback = null) =>
            DependencyProperty.Register(
                name,
                typeof(T),
                typeof(CoordinateMap),
                new FrameworkPropertyMetadata(defaultValue, callback));
    }
}
