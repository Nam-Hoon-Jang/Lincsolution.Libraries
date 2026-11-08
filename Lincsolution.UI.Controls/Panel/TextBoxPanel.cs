using System.Windows;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 제목, 검증 가능한 텍스트 입력 및 단위를 한 행에 표시합니다.
    /// EN: Displays a subject, validated text input, and unit in one row.
    /// </summary>
    public class TextBoxPanel : LabeledPanelBase
    {
        static TextBoxPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxPanel), new FrameworkPropertyMetadata(typeof(TextBoxPanel)));
        }

        /// <summary>
        /// KO: Text 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the Text dependency property. Gets or sets the text.
        /// </summary>
        public static readonly DependencyProperty TextProperty = Register(nameof(Text), string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault);
        /// <summary>
        /// KO: ValidationMode 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ValidationMode dependency property. Gets or sets the ValidationMode value.
        /// </summary>
        public static readonly DependencyProperty ValidationModeProperty = Register(nameof(ValidationMode), InputValidationMode.NumOnly);
        /// <summary>
        /// KO: TextBoxWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty TextBoxWidthProperty = Register(nameof(TextBoxWidth), 64d);
        /// <summary>
        /// KO: TextBoxHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty TextBoxHeightProperty = Register(nameof(TextBoxHeight), 32d);
        /// <summary>
        /// KO: TextBoxBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TextBoxBackgroundProperty = Register<Brush?>(nameof(TextBoxBackground), null);
        /// <summary>
        /// KO: TextBoxForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TextBoxForegroundProperty = Register<Brush?>(nameof(TextBoxForeground), null);
        /// <summary>
        /// KO: TextBoxBorderBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxBorderBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TextBoxBorderBrushProperty = Register<Brush?>(nameof(TextBoxBorderBrush), null);
        /// <summary>
        /// KO: TextBoxBorderThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxBorderThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty TextBoxBorderThicknessProperty = Register(nameof(TextBoxBorderThickness), new Thickness(1));
        /// <summary>
        /// KO: TextBoxCornerRadius 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxCornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty TextBoxCornerRadiusProperty = Register(nameof(TextBoxCornerRadius), new CornerRadius(5));
        /// <summary>
        /// KO: TextBoxFontFamily 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxFontFamily dependency property. Gets or sets the font family.
        /// </summary>
        public static readonly DependencyProperty TextBoxFontFamilyProperty = Register(nameof(TextBoxFontFamily), SystemFonts.MessageFontFamily);
        /// <summary>
        /// KO: TextBoxFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty TextBoxFontSizeProperty = Register(nameof(TextBoxFontSize), 14d);
        /// <summary>
        /// KO: TextBoxFontWeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxFontWeight dependency property. Gets or sets the font weight.
        /// </summary>
        public static readonly DependencyProperty TextBoxFontWeightProperty = Register(nameof(TextBoxFontWeight), FontWeights.Normal);
        /// <summary>
        /// KO: TextBoxFontStyle 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxFontStyle dependency property. Gets or sets the font style.
        /// </summary>
        public static readonly DependencyProperty TextBoxFontStyleProperty = Register(nameof(TextBoxFontStyle), FontStyles.Normal);
        /// <summary>
        /// KO: TextBoxPadding 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBoxPadding dependency property. Gets or sets the padding.
        /// </summary>
        public static readonly DependencyProperty TextBoxPaddingProperty = Register(nameof(TextBoxPadding), new Thickness(8, 3, 8, 3));
        /// <summary>
        /// KO: Watermark 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the Watermark dependency property. Gets or sets the Watermark value.
        /// </summary>
        public static readonly DependencyProperty WatermarkProperty = Register(nameof(Watermark), string.Empty);
        /// <summary>
        /// KO: WatermarkFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontSizeProperty = Register(nameof(WatermarkFontSize), 14d);
        /// <summary>
        /// KO: WatermarkFontWeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontWeight dependency property. Gets or sets the font weight.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontWeightProperty = Register(nameof(WatermarkFontWeight), FontWeights.Normal);
        /// <summary>
        /// KO: WatermarkFontStyle 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkFontStyle dependency property. Gets or sets the font style.
        /// </summary>
        public static readonly DependencyProperty WatermarkFontStyleProperty = Register(nameof(WatermarkFontStyle), FontStyles.Normal);
        /// <summary>
        /// KO: WatermarkForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty WatermarkForegroundProperty = Register<Brush?>(nameof(WatermarkForeground), null);
        /// <summary>
        /// KO: WatermarkMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the WatermarkMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty WatermarkMarginProperty = Register(nameof(WatermarkMargin), new Thickness(2, 0, 0, 0));

        [LocalizedDescription("입력 텍스트를 가져오거나 설정합니다.", "Gets or sets the input text.")]
        public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
        [LocalizedDescription("텍스트 입력 검증 형식을 가져오거나 설정합니다.", "Gets or sets the text validation mode.")]
        public InputValidationMode ValidationMode { get => (InputValidationMode)GetValue(ValidationModeProperty); set => SetValue(ValidationModeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double TextBoxWidth { get => (double)GetValue(TextBoxWidthProperty); set => SetValue(TextBoxWidthProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double TextBoxHeight { get => (double)GetValue(TextBoxHeightProperty); set => SetValue(TextBoxHeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? TextBoxBackground { get => (Brush?)GetValue(TextBoxBackgroundProperty); set => SetValue(TextBoxBackgroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? TextBoxForeground { get => (Brush?)GetValue(TextBoxForegroundProperty); set => SetValue(TextBoxForegroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? TextBoxBorderBrush { get => (Brush?)GetValue(TextBoxBorderBrushProperty); set => SetValue(TextBoxBorderBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public Thickness TextBoxBorderThickness { get => (Thickness)GetValue(TextBoxBorderThicknessProperty); set => SetValue(TextBoxBorderThicknessProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius TextBoxCornerRadius { get => (CornerRadius)GetValue(TextBoxCornerRadiusProperty); set => SetValue(TextBoxCornerRadiusProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font family.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font family.")]
        public FontFamily TextBoxFontFamily { get => (FontFamily)GetValue(TextBoxFontFamilyProperty); set => SetValue(TextBoxFontFamilyProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double TextBoxFontSize { get => (double)GetValue(TextBoxFontSizeProperty); set => SetValue(TextBoxFontSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font weight.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font weight.")]
        public FontWeight TextBoxFontWeight { get => (FontWeight)GetValue(TextBoxFontWeightProperty); set => SetValue(TextBoxFontWeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font style.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font style.")]
        public FontStyle TextBoxFontStyle { get => (FontStyle)GetValue(TextBoxFontStyleProperty); set => SetValue(TextBoxFontStyleProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the padding.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the padding.")]
        public Thickness TextBoxPadding { get => (Thickness)GetValue(TextBoxPaddingProperty); set => SetValue(TextBoxPaddingProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the Watermark value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the Watermark value.")]
        public string Watermark { get => (string)GetValue(WatermarkProperty); set => SetValue(WatermarkProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double WatermarkFontSize { get => (double)GetValue(WatermarkFontSizeProperty); set => SetValue(WatermarkFontSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font weight.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font weight.")]
        public FontWeight WatermarkFontWeight { get => (FontWeight)GetValue(WatermarkFontWeightProperty); set => SetValue(WatermarkFontWeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font style.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font style.")]
        public FontStyle WatermarkFontStyle { get => (FontStyle)GetValue(WatermarkFontStyleProperty); set => SetValue(WatermarkFontStyleProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? WatermarkForeground { get => (Brush?)GetValue(WatermarkForegroundProperty); set => SetValue(WatermarkForegroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness WatermarkMargin { get => (Thickness)GetValue(WatermarkMarginProperty); set => SetValue(WatermarkMarginProperty, value); }

        /// <summary>
        /// KO: Register 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by Register.
        /// </summary>
        private static DependencyProperty Register<T>(string name, T defaultValue, FrameworkPropertyMetadataOptions options = FrameworkPropertyMetadataOptions.None) =>
            DependencyProperty.Register(name, typeof(T), typeof(TextBoxPanel), new FrameworkPropertyMetadata(defaultValue, options));
    }
}
