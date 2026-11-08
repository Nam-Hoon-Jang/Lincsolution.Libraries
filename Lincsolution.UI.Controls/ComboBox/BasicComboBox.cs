using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 드롭다운 아이콘과 모서리 반경을 조절할 수 있는 콤보박스입니다.
    /// EN: A combo box with an adjustable drop-down icon and corner radius.
    /// </summary>
    [LocalizedDescription("드롭다운 아이콘과 모서리 반경을 조절할 수 있는 콤보박스입니다.", "A combo box with an adjustable drop-down icon and corner radius.")]
    public class BasicComboBox : ComboBox
    {
        /// <summary>
        /// KO: BasicComboBox 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicComboBox control.
        /// </summary>
        static BasicComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(typeof(BasicComboBox)));
        }

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// KO: DropDownButtonWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the DropDownButtonWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty DropDownButtonWidthProperty =
            DependencyProperty.Register(
                nameof(DropDownButtonWidth),
                typeof(double),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(40d));

        /// <summary>
        /// KO: IconSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(
                nameof(IconSize),
                typeof(double),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(24d));

        /// <summary>
        /// KO: IconStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconStrokeProperty =
            DependencyProperty.Register(
                nameof(IconStroke),
                typeof(Brush),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: IconStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty IconStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(IconStrokeThickness),
                typeof(double),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(1.5d));

        /// <summary>
        /// KO: IconMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register(
                nameof(IconMargin),
                typeof(Thickness),
                typeof(BasicComboBox),
                new FrameworkPropertyMetadata(new Thickness(0)));

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

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double DropDownButtonWidth
        {
            get => (double)GetValue(DropDownButtonWidthProperty);
            set => SetValue(DropDownButtonWidthProperty, value);
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
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness IconMargin
        {
            get => (Thickness)GetValue(IconMarginProperty);
            set => SetValue(IconMarginProperty, value);
        }
    }
}
