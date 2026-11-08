using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 체크 표시와 모서리 및 색상을 조절할 수 있는 체크박스입니다.
    /// EN: A check box with customizable check mark, corners, and colors.
    /// </summary>
    public class BasicCheckBox : CheckBox
    {
        static BasicCheckBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicCheckBox), new FrameworkPropertyMetadata(typeof(BasicCheckBox)));
        }

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BasicCheckBox), new FrameworkPropertyMetadata(new CornerRadius(3)));
        /// <summary>
        /// KO: BoxSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the BoxSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty BoxSizeProperty =
            DependencyProperty.Register(nameof(BoxSize), typeof(double), typeof(BasicCheckBox), new FrameworkPropertyMetadata(20d));
        /// <summary>
        /// KO: CheckedBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CheckedBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register(nameof(CheckedBackground), typeof(Brush), typeof(BasicCheckBox));
        /// <summary>
        /// KO: CheckBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CheckBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty CheckBrushProperty =
            DependencyProperty.Register(nameof(CheckBrush), typeof(Brush), typeof(BasicCheckBox));
        /// <summary>
        /// KO: StrokeThickness 종속성 속성 식별자입니다. 체크 표시 선 두께를 가져오거나 설정합니다.
        /// EN: Identifies the StrokeThickness dependency property. Gets or sets the check mark stroke thickness.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(StrokeThickness),
                typeof(double),
                typeof(BasicCheckBox),
                new FrameworkPropertyMetadata(2.5d));

        /// <summary>
        /// KO: CheckArrowSize 종속성 속성 식별자입니다. 체크 화살표의 가로 및 세로 크기를 가져오거나 설정합니다.
        /// EN: Identifies the CheckArrowSize dependency property. Gets or sets both the width and height of the check arrow.
        /// </summary>
        public static readonly DependencyProperty CheckArrowSizeProperty =
            DependencyProperty.Register(
                nameof(CheckArrowSize),
                typeof(double),
                typeof(BasicCheckBox),
                new FrameworkPropertyMetadata(15d));
        /// <summary>
        /// KO: ContentSpacing 종속성 속성 식별자입니다. 콘텐츠와 체크박스 사이 간격을 가져오거나 설정합니다.
        /// EN: Identifies the ContentSpacing dependency property. Gets or sets spacing between content and the check box.
        /// </summary>
        public static readonly DependencyProperty ContentSpacingProperty =
            DependencyProperty.Register(nameof(ContentSpacing), typeof(double), typeof(BasicCheckBox), new FrameworkPropertyMetadata(8d));

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius CornerRadius { get => (CornerRadius)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double BoxSize { get => (double)GetValue(BoxSizeProperty); set => SetValue(BoxSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? CheckedBackground { get => (Brush?)GetValue(CheckedBackgroundProperty); set => SetValue(CheckedBackgroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? CheckBrush { get => (Brush?)GetValue(CheckBrushProperty); set => SetValue(CheckBrushProperty, value); }
        /// <summary>
        /// KO: 체크 표시 선 두께를 가져오거나 설정합니다.
        /// EN: Gets or sets the check mark stroke thickness.
        /// </summary>
        [LocalizedDescription("체크 표시 선 두께를 가져오거나 설정합니다.", "Gets or sets the check mark stroke thickness.")]
        public double StrokeThickness
        {
            get => (double)GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        /// <summary>
        /// KO: 체크 화살표의 가로 및 세로 크기를 가져오거나 설정합니다.
        /// EN: Gets or sets both the width and height of the check arrow.
        /// </summary>
        [LocalizedDescription("체크 화살표의 가로 및 세로 크기를 가져오거나 설정합니다.", "Gets or sets both the width and height of the check arrow.")]
        public double CheckArrowSize
        {
            get => (double)GetValue(CheckArrowSizeProperty);
            set => SetValue(CheckArrowSizeProperty, value);
        }
        /// <summary>
        /// KO: 콘텐츠와 체크박스 사이 간격을 가져오거나 설정합니다.
        /// EN: Gets or sets spacing between content and the check box.
        /// </summary>
        [LocalizedDescription("콘텐츠와 체크박스 사이 간격을 가져오거나 설정합니다.", "Gets or sets spacing between content and the check box.")]
        public double ContentSpacing { get => (double)GetValue(ContentSpacingProperty); set => SetValue(ContentSpacingProperty, value); }
    }
}
