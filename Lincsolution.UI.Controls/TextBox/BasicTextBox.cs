using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 워터마크와 모서리 반경을 지원하는 텍스트 입력 컨트롤입니다.
    /// EN: A text input control supporting a watermark and corner radius.
    /// </summary>
    [LocalizedDescription("워터마크와 모서리 반경을 지원하는 텍스트 입력 컨트롤입니다.", "A text input control supporting a watermark and corner radius.")]
    public class BasicTextBox : TextBox
    {
        /// <summary>
        /// KO: BasicTextBox 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicTextBox control.
        /// </summary>
        static BasicTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(typeof(BasicTextBox)));
        }

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// KO: Watermark 종속성 속성 식별자입니다. 입력값이 없을 때 표시할 워터마크를 가져오거나 설정합니다.
        /// EN: Identifies the Watermark dependency property. Gets or sets the watermark displayed when the input is empty.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                nameof(Watermark),
                typeof(string),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// KO: WatermarkFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontSizeProperty =
            DependencyProperty.Register(
                nameof(WatermarkFontSize),
                typeof(double),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(SystemFonts.MessageFontSize));

        /// <summary>
        /// KO: WatermarkFontWeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontWeight dependency property. Gets or sets the font weight.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontWeightProperty =
            DependencyProperty.Register(
                nameof(WatermarkFontWeight),
                typeof(FontWeight),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(FontWeights.Normal));

        /// <summary>
        /// KO: WatermarkFontStyle 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontStyle dependency property. Gets or sets the font style.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontStyleProperty =
            DependencyProperty.Register(
                nameof(WatermarkFontStyle),
                typeof(FontStyle),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(FontStyles.Normal));

        /// <summary>
        /// KO: WatermarkForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty WatermarkForegroundProperty =
            DependencyProperty.Register(
                nameof(WatermarkForeground),
                typeof(Brush),
                typeof(BasicTextBox),
                new FrameworkPropertyMetadata(Brushes.Gray));

        /// <summary>
        /// KO: WatermarkMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty WatermarkMarginProperty =
            DependencyProperty.Register(
                nameof(WatermarkMargin),
                typeof(Thickness),
                typeof(BasicTextBox),
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
        /// KO: 입력값이 없을 때 표시할 워터마크를 가져오거나 설정합니다.
        /// EN: Gets or sets the watermark displayed when the input is empty.
        /// </summary>
        [LocalizedDescription("입력값이 없을 때 표시할 워터마크를 가져오거나 설정합니다.", "Gets or sets the watermark displayed when the input is empty.")]
        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double WatermarkFontSize
        {
            get => (double)GetValue(WatermarkFontSizeProperty);
            set => SetValue(WatermarkFontSizeProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font weight.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font weight.")]
        public FontWeight WatermarkFontWeight
        {
            get => (FontWeight)GetValue(WatermarkFontWeightProperty);
            set => SetValue(WatermarkFontWeightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font style.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font style.")]
        public FontStyle WatermarkFontStyle
        {
            get => (FontStyle)GetValue(WatermarkFontStyleProperty);
            set => SetValue(WatermarkFontStyleProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush WatermarkForeground
        {
            get => (Brush)GetValue(WatermarkForegroundProperty);
            set => SetValue(WatermarkForegroundProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness WatermarkMargin
        {
            get => (Thickness)GetValue(WatermarkMarginProperty);
            set => SetValue(WatermarkMarginProperty, value);
        }
    }
}
