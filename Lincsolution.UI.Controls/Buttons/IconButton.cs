using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: Geometry 아이콘을 표시하는 버튼입니다.
    /// EN: A button that displays a Geometry icon.
    /// </summary>
    [LocalizedDescription("Geometry 아이콘을 표시하는 버튼입니다.", "A button that displays a Geometry icon.")]
    public class IconButton : Button
    {
        /// <summary>
        /// KO: IconButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the IconButton control.
        /// </summary>
        static IconButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IconButton), new FrameworkPropertyMetadata(typeof(IconButton)));
        }

        /// <summary>
        /// KO: Icon 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the Icon dependency property. Gets or sets the icon.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(Geometry), typeof(IconButton));
        /// <summary>
        /// KO: IconWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(IconButton));
        /// <summary>
        /// KO: IconHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(IconButton));
        /// <summary>
        /// KO: IconFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(nameof(IconFill), typeof(Brush), typeof(IconButton));
        /// <summary>
        /// KO: IconStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconStrokeProperty = DependencyProperty.Register(nameof(IconStroke), typeof(Brush), typeof(IconButton));
        /// <summary>
        /// KO: IconStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty IconStrokeThicknessProperty = DependencyProperty.Register(nameof(IconStrokeThickness), typeof(double), typeof(IconButton));
        /// <summary>
        /// KO: IconHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconHorizontalAlignmentProperty = DependencyProperty.Register(nameof(IconHorizontalAlignment), typeof(HorizontalAlignment), typeof(IconButton));
        /// <summary>
        /// KO: IconVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty IconVerticalAlignmentProperty = DependencyProperty.Register(nameof(IconVerticalAlignment), typeof(VerticalAlignment), typeof(IconButton));
        /// <summary>
        /// KO: IconMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(IconButton));
        /// <summary>
        /// KO: ContentMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(IconButton));
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(IconButton));
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
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double IconWidth
        {
            get => (double)GetValue(IconWidthProperty);
            set => SetValue(IconWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double IconHeight
        {
            get => (double)GetValue(IconHeightProperty);
            set => SetValue(IconHeightProperty, value);
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
        public HorizontalAlignment IconHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(IconHorizontalAlignmentProperty);
            set => SetValue(IconHorizontalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment IconVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(IconVerticalAlignmentProperty);
            set => SetValue(IconVerticalAlignmentProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ContentMargin
        {
            get => (Thickness)(GetValue(ContentMarginProperty));
            set => SetValue(ContentMarginProperty, value);
        }
        /// <summary>
        /// KO: 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius of the control.
        /// </summary>
        [LocalizedDescription("컨트롤 모서리의 반경을 가져오거나 설정합니다.", "Gets or sets the corner radius of the control.")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
