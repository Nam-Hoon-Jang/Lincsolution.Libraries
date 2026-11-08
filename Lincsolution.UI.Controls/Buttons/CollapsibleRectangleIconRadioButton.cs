using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 확장 및 축소 상태를 지원하는 아이콘 라디오 버튼입니다.
    /// EN: An icon radio button that supports expanded and collapsed states.
    /// </summary>
    [LocalizedDescription("확장 및 축소 상태를 지원하는 아이콘 라디오 버튼입니다.", "An icon radio button that supports expanded and collapsed states.")]
    public class CollapsibleRectangleIconRadioButton : RadioButton
    {
        /// <summary>
        /// KO: CollapsibleRectangleIconRadioButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the CollapsibleRectangleIconRadioButton control.
        /// </summary>
        static CollapsibleRectangleIconRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CollapsibleRectangleIconRadioButton), new FrameworkPropertyMetadata(typeof(CollapsibleRectangleIconRadioButton)));
        }
        /// <summary>
        /// KO: IsCollapsed 종속성 속성 식별자입니다. 컨트롤의 축소 상태를 가져오거나 설정합니다.
        /// EN: Identifies the IsCollapsed dependency property. Gets or sets whether the control is collapsed.
        /// </summary>
        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register(nameof(IsCollapsed),typeof(bool), typeof(CollapsibleRectangleIconRadioButton), new FrameworkPropertyMetadata(false, OnIsCollapsedChanged));
        /// <summary>
        /// KO: ExpandedWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ExpandedWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ExpandedWidthProperty = DependencyProperty.Register(nameof(ExpandedWidth), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: CollapsedWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CollapsedWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty CollapsedWidthProperty = DependencyProperty.Register(nameof(CollapsedWidth), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: Icon 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the Icon dependency property. Gets or sets the icon.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Geometry), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(nameof(IconFill), typeof(Brush), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconStrokeProperty = DependencyProperty.Register(nameof(IconStroke), typeof(Brush), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty IconStrokeThicknessProperty = DependencyProperty.Register(nameof(IconStrokeThickness), typeof(double), typeof(CollapsibleRectangleIconRadioButton));

        /// <summary>
        /// KO: IconExpandedHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconExpandedHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconExpandedHorizontalAlignmentProperty = DependencyProperty.Register(nameof(IconExpandedHorizontalAlignment), typeof(HorizontalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconExpandedVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconExpandedVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconExpandedVerticalAlignmentProperty = DependencyProperty.Register(nameof(IconExpandedVerticalAlignment), typeof(VerticalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconCollapsedHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconCollapsedHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconCollapsedHorizontalAlignmentProperty = DependencyProperty.Register(nameof(IconCollapsedHorizontalAlignment), typeof(HorizontalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconCollapsedVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconCollapsedVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconCollapsedVerticalAlignmentProperty = DependencyProperty.Register(nameof(IconCollapsedVerticalAlignment), typeof(VerticalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconExpandedMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconExpandedMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty IconExpandedMarginProperty = DependencyProperty.Register(nameof(IconExpandedMargin), typeof(Thickness), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: IconCollapsedMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconCollapsedMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty IconCollapsedMarginProperty = DependencyProperty.Register(nameof(IconCollapsedMargin), typeof(Thickness), typeof(CollapsibleRectangleIconRadioButton));

        /// <summary>
        /// KO: ContentWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(nameof(ContentWidth), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentExpandedMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentExpandedMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ContentExpandedMarginProperty = DependencyProperty.Register(nameof(ContentExpandedMargin), typeof(Thickness), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentCollapsedMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentCollapsedMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ContentCollapsedMarginProperty = DependencyProperty.Register(nameof(ContentCollapsedMargin), typeof(Thickness), typeof(CollapsibleRectangleIconRadioButton));

        /// <summary>
        /// KO: ContentExpandedHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentExpandedHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty ContentExpandedHorizontalAlignmentProperty = DependencyProperty.Register(nameof(ContentExpandedHorizontalAlignment), typeof(HorizontalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentExpandedVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentExpandedVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty ContentExpandedVerticalAlignmentProperty = DependencyProperty.Register(nameof(ContentExpandedVerticalAlignment), typeof(VerticalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentCollapsedHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentCollapsedHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty ContentCollapsedHorizontalAlignmentProperty = DependencyProperty.Register(nameof(ContentCollapsedHorizontalAlignment), typeof(HorizontalAlignment), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentCollapsedVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentCollapsedVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty ContentCollapsedVerticalAlignmentProperty = DependencyProperty.Register(nameof(ContentCollapsedVerticalAlignment), typeof(VerticalAlignment), typeof(CollapsibleRectangleIconRadioButton));

        /// <summary>
        /// KO: ContentExpandedFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentExpandedFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty ContentExpandedFontSizeProperty = DependencyProperty.Register(nameof(ContentExpandedFontSize), typeof(double), typeof(CollapsibleRectangleIconRadioButton));
        /// <summary>
        /// KO: ContentCollapsedFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentCollapsedFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty ContentCollapsedFontSizeProperty = DependencyProperty.Register(nameof(ContentCollapsedFontSize), typeof(double), typeof(CollapsibleRectangleIconRadioButton));

        /// <summary>
        /// KO: 축소 상태 변경을 컨트롤 너비에 반영합니다.
        /// EN: Applies a collapsed-state change to the control width.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CollapsibleRectangleIconRadioButton)d;
            var target = (bool)e.NewValue ? control.CollapsedWidth : control.ExpandedWidth;

            control.BeginAnimation(WidthProperty, new DoubleAnimation(target, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                },
                FillBehavior = FillBehavior.Stop
            });
            control.Width = target;
        }
        /// <summary>
        /// KO: 컨트롤의 축소 상태를 가져오거나 설정합니다.
        /// EN: Gets or sets whether the control is collapsed.
        /// </summary>
        [LocalizedDescription("컨트롤의 축소 상태를 가져오거나 설정합니다.", "Gets or sets whether the control is collapsed.")]
        public bool IsCollapsed
        {
            get => (bool)GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ExpandedWidth
        {
            get => (double)GetValue(ExpandedWidthProperty);
            set => SetValue(ExpandedWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double CollapsedWidth
        {
            get => (double)GetValue(CollapsedWidthProperty);
            set => SetValue(CollapsedWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the icon.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the icon.")]
        public Geometry? Icon
        {
            get => (Geometry?)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? IconFill
        {
            get => (Brush?)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? IconStroke
        {
            get => (Brush?)GetValue(IconStrokeProperty);
            set => SetValue(IconStrokeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double IconStrokeThickness
        {
            get => (double)GetValue(IconStrokeThicknessProperty);
            set => SetValue(IconStrokeThicknessProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public HorizontalAlignment IconExpandedHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(IconExpandedHorizontalAlignmentProperty);
            set => SetValue(IconExpandedHorizontalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment IconExpandedVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(IconExpandedVerticalAlignmentProperty);
            set => SetValue(IconExpandedVerticalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public HorizontalAlignment IconCollapsedHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(IconCollapsedHorizontalAlignmentProperty);
            set => SetValue(IconCollapsedHorizontalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment IconCollapsedVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(IconCollapsedVerticalAlignmentProperty);
            set => SetValue(IconCollapsedVerticalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness IconCollapsedMargin
        {
            get => (Thickness)GetValue(IconCollapsedMarginProperty);
            set => SetValue(IconCollapsedMarginProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness IconExpandedMargin
        {
            get => (Thickness)GetValue(IconExpandedMarginProperty);
            set => SetValue(IconExpandedMarginProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ContentWidth
        {
            get => (double)GetValue(ContentWidthProperty);
            set => SetValue(ContentWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ContentHeight
        {
            get => (double)GetValue(ContentHeightProperty);
            set => SetValue(ContentHeightProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ContentExpandedMargin
        {
            get => (Thickness)GetValue(ContentExpandedMarginProperty);
            set => SetValue(ContentExpandedMarginProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ContentCollapsedMargin
        {
            get => (Thickness)GetValue(ContentCollapsedMarginProperty);
            set => SetValue(ContentCollapsedMarginProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public HorizontalAlignment ContentExpandedHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(ContentExpandedHorizontalAlignmentProperty);
            set => SetValue(ContentExpandedHorizontalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment ContentExpandedVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(ContentExpandedVerticalAlignmentProperty);
            set => SetValue(ContentExpandedVerticalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public HorizontalAlignment ContentCollapsedHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(ContentCollapsedHorizontalAlignmentProperty);
            set => SetValue(ContentCollapsedHorizontalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment ContentCollapsedVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(ContentCollapsedVerticalAlignmentProperty);
            set => SetValue(ContentCollapsedVerticalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double ContentExpandedFontSize
        {
            get => (double)GetValue(ContentExpandedFontSizeProperty);
            set => SetValue(ContentExpandedFontSizeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double ContentCollapsedFontSize
        {
            get => (double)GetValue(ContentCollapsedFontSizeProperty);
            set => SetValue(ContentCollapsedFontSizeProperty, value);
        }
    }
}
