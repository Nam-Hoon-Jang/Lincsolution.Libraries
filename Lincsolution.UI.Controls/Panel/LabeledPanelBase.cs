using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 제목과 단위 텍스트를 표시하는 패널 컨트롤의 공통 기반입니다.
    /// EN: Provides common subject and unit text properties for panel controls.
    /// </summary>
    public abstract class LabeledPanelBase : Control
    {
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 패널 외곽 모서리 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the panel corner radius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = Register(nameof(CornerRadius), new CornerRadius(0));
        /// <summary>
        /// KO: LeftSpace 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the LeftSpace dependency property. Gets or sets the spacing.
        /// </summary>
        public static readonly DependencyProperty LeftSpaceProperty = Register(nameof(LeftSpace), new GridLength(24));
        /// <summary>
        /// KO: MidSpace 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the MidSpace dependency property. Gets or sets the spacing.
        /// </summary>
        public static readonly DependencyProperty MidSpaceProperty = Register(nameof(MidSpace), new GridLength(24));
        /// <summary>
        /// KO: EndSpace 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the EndSpace dependency property. Gets or sets the spacing.
        /// </summary>
        public static readonly DependencyProperty EndSpaceProperty = Register(nameof(EndSpace), new GridLength(0));
        /// <summary>
        /// KO: SubjectWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty SubjectWidthProperty = Register(nameof(SubjectWidth), new GridLength(120));
        /// <summary>
        /// KO: UnitWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty UnitWidthProperty = Register(nameof(UnitWidth), new GridLength(1, GridUnitType.Star));
        /// <summary>
        /// KO: SubjectText 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectText dependency property. Gets or sets the text.
        /// </summary>
        public static readonly DependencyProperty SubjectTextProperty = Register(nameof(SubjectText), string.Empty);
        /// <summary>
        /// KO: SubjectBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty SubjectBackgroundProperty = Register<Brush?>(nameof(SubjectBackground), Brushes.Transparent);
        /// <summary>
        /// KO: SubjectForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty SubjectForegroundProperty = Register<Brush?>(nameof(SubjectForeground), null);
        /// <summary>
        /// KO: SubjectBorderBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectBorderBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty SubjectBorderBrushProperty = Register<Brush?>(nameof(SubjectBorderBrush), null);
        /// <summary>
        /// KO: SubjectBorderThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectBorderThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty SubjectBorderThicknessProperty = Register(nameof(SubjectBorderThickness), new Thickness(0));
        /// <summary>
        /// KO: SubjectCornerRadius 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectCornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty SubjectCornerRadiusProperty = Register(nameof(SubjectCornerRadius), new CornerRadius(0));
        /// <summary>
        /// KO: SubjectFontFamily 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectFontFamily dependency property. Gets or sets the font family.
        /// </summary>
        public static readonly DependencyProperty SubjectFontFamilyProperty = Register(nameof(SubjectFontFamily), SystemFonts.MessageFontFamily);
        /// <summary>
        /// KO: SubjectFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty SubjectFontSizeProperty = Register(nameof(SubjectFontSize), 14d);
        /// <summary>
        /// KO: SubjectFontWeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectFontWeight dependency property. Gets or sets the font weight.
        /// </summary>
        public static readonly DependencyProperty SubjectFontWeightProperty = Register(nameof(SubjectFontWeight), FontWeights.Normal);
        /// <summary>
        /// KO: SubjectFontStyle 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectFontStyle dependency property. Gets or sets the font style.
        /// </summary>
        public static readonly DependencyProperty SubjectFontStyleProperty = Register(nameof(SubjectFontStyle), FontStyles.Normal);
        /// <summary>
        /// KO: SubjectPadding 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectPadding dependency property. Gets or sets the padding.
        /// </summary>
        public static readonly DependencyProperty SubjectPaddingProperty = Register(nameof(SubjectPadding), new Thickness(0));
        /// <summary>
        /// KO: SubjectTextWrapping 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectTextWrapping dependency property. Gets or sets the text wrapping.
        /// </summary>
        public static readonly DependencyProperty SubjectTextWrappingProperty = Register(nameof(SubjectTextWrapping), TextWrapping.NoWrap);
        /// <summary>
        /// KO: SubjectTextTrimming 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the SubjectTextTrimming dependency property. Gets or sets the text trimming.
        /// </summary>
        public static readonly DependencyProperty SubjectTextTrimmingProperty = Register(nameof(SubjectTextTrimming), TextTrimming.None);
        /// <summary>
        /// KO: UnitText 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitText dependency property. Gets or sets the text.
        /// </summary>
        public static readonly DependencyProperty UnitTextProperty = Register(nameof(UnitText), string.Empty);
        /// <summary>
        /// KO: UnitBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty UnitBackgroundProperty = Register<Brush?>(nameof(UnitBackground), Brushes.Transparent);
        /// <summary>
        /// KO: UnitForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty UnitForegroundProperty = Register<Brush?>(nameof(UnitForeground), null);
        /// <summary>
        /// KO: UnitBorderBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitBorderBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty UnitBorderBrushProperty = Register<Brush?>(nameof(UnitBorderBrush), null);
        /// <summary>
        /// KO: UnitBorderThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitBorderThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty UnitBorderThicknessProperty = Register(nameof(UnitBorderThickness), new Thickness(0));
        /// <summary>
        /// KO: UnitCornerRadius 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitCornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty UnitCornerRadiusProperty = Register(nameof(UnitCornerRadius), new CornerRadius(0));
        /// <summary>
        /// KO: UnitFontFamily 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitFontFamily dependency property. Gets or sets the font family.
        /// </summary>
        public static readonly DependencyProperty UnitFontFamilyProperty = Register(nameof(UnitFontFamily), SystemFonts.MessageFontFamily);
        /// <summary>
        /// KO: UnitFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty UnitFontSizeProperty = Register(nameof(UnitFontSize), 14d);
        /// <summary>
        /// KO: UnitFontWeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitFontWeight dependency property. Gets or sets the font weight.
        /// </summary>
        public static readonly DependencyProperty UnitFontWeightProperty = Register(nameof(UnitFontWeight), FontWeights.Normal);
        /// <summary>
        /// KO: UnitFontStyle 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitFontStyle dependency property. Gets or sets the font style.
        /// </summary>
        public static readonly DependencyProperty UnitFontStyleProperty = Register(nameof(UnitFontStyle), FontStyles.Normal);
        /// <summary>
        /// KO: UnitPadding 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitPadding dependency property. Gets or sets the padding.
        /// </summary>
        public static readonly DependencyProperty UnitPaddingProperty = Register(nameof(UnitPadding), new Thickness(0));
        /// <summary>
        /// KO: UnitTextWrapping 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitTextWrapping dependency property. Gets or sets the text wrapping.
        /// </summary>
        public static readonly DependencyProperty UnitTextWrappingProperty = Register(nameof(UnitTextWrapping), TextWrapping.NoWrap);
        /// <summary>
        /// KO: UnitTextTrimming 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UnitTextTrimming dependency property. Gets or sets the text trimming.
        /// </summary>
        public static readonly DependencyProperty UnitTextTrimmingProperty = Register(nameof(UnitTextTrimming), TextTrimming.None);

        /// <summary>
        /// KO: 패널 외곽 모서리 반경을 가져오거나 설정합니다.
        /// EN: Gets or sets the panel corner radius.
        /// </summary>
        [LocalizedDescription("패널 외곽 모서리 반경을 가져오거나 설정합니다.", "Gets or sets the panel corner radius.")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        [LocalizedDescription("패널 왼쪽의 빈 공간을 가져오거나 설정합니다.", "Gets or sets the empty space at the left of the panel.")]
        public GridLength LeftSpace { get => (GridLength)GetValue(LeftSpaceProperty); set => SetValue(LeftSpaceProperty, value); }
        [LocalizedDescription("제목과 입력 컨트롤 및 입력 컨트롤과 단위 사이의 간격을 가져오거나 설정합니다.", "Gets or sets the shared spacing around the input control.")]
        public GridLength MidSpace { get => (GridLength)GetValue(MidSpaceProperty); set => SetValue(MidSpaceProperty, value); }
        [LocalizedDescription("단위 텍스트 뒤의 빈 공간을 가져오거나 설정합니다.", "Gets or sets the empty space after the unit text.")]
        public GridLength EndSpace { get => (GridLength)GetValue(EndSpaceProperty); set => SetValue(EndSpaceProperty, value); }
        [LocalizedDescription("제목 영역 너비를 가져오거나 설정합니다.", "Gets or sets the subject area width.")]
        public GridLength SubjectWidth { get => (GridLength)GetValue(SubjectWidthProperty); set => SetValue(SubjectWidthProperty, value); }
        [LocalizedDescription("단위 영역 너비를 가져오거나 설정합니다.", "Gets or sets the unit area width.")]
        public GridLength UnitWidth { get => (GridLength)GetValue(UnitWidthProperty); set => SetValue(UnitWidthProperty, value); }

        [LocalizedDescription("제목 텍스트를 가져오거나 설정합니다.", "Gets or sets the subject text.")]
        public string SubjectText { get => (string)GetValue(SubjectTextProperty); set => SetValue(SubjectTextProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? SubjectBackground { get => (Brush?)GetValue(SubjectBackgroundProperty); set => SetValue(SubjectBackgroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? SubjectForeground { get => (Brush?)GetValue(SubjectForegroundProperty); set => SetValue(SubjectForegroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? SubjectBorderBrush { get => (Brush?)GetValue(SubjectBorderBrushProperty); set => SetValue(SubjectBorderBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public Thickness SubjectBorderThickness { get => (Thickness)GetValue(SubjectBorderThicknessProperty); set => SetValue(SubjectBorderThicknessProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius SubjectCornerRadius { get => (CornerRadius)GetValue(SubjectCornerRadiusProperty); set => SetValue(SubjectCornerRadiusProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font family.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font family.")]
        public FontFamily SubjectFontFamily { get => (FontFamily)GetValue(SubjectFontFamilyProperty); set => SetValue(SubjectFontFamilyProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double SubjectFontSize { get => (double)GetValue(SubjectFontSizeProperty); set => SetValue(SubjectFontSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font weight.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font weight.")]
        public FontWeight SubjectFontWeight { get => (FontWeight)GetValue(SubjectFontWeightProperty); set => SetValue(SubjectFontWeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font style.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font style.")]
        public FontStyle SubjectFontStyle { get => (FontStyle)GetValue(SubjectFontStyleProperty); set => SetValue(SubjectFontStyleProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the padding.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the padding.")]
        public Thickness SubjectPadding { get => (Thickness)GetValue(SubjectPaddingProperty); set => SetValue(SubjectPaddingProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the text wrapping.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the text wrapping.")]
        public TextWrapping SubjectTextWrapping { get => (TextWrapping)GetValue(SubjectTextWrappingProperty); set => SetValue(SubjectTextWrappingProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the text trimming.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the text trimming.")]
        public TextTrimming SubjectTextTrimming { get => (TextTrimming)GetValue(SubjectTextTrimmingProperty); set => SetValue(SubjectTextTrimmingProperty, value); }

        [LocalizedDescription("단위 텍스트를 가져오거나 설정합니다.", "Gets or sets the unit text.")]
        public string UnitText { get => (string)GetValue(UnitTextProperty); set => SetValue(UnitTextProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? UnitBackground { get => (Brush?)GetValue(UnitBackgroundProperty); set => SetValue(UnitBackgroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? UnitForeground { get => (Brush?)GetValue(UnitForegroundProperty); set => SetValue(UnitForegroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? UnitBorderBrush { get => (Brush?)GetValue(UnitBorderBrushProperty); set => SetValue(UnitBorderBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public Thickness UnitBorderThickness { get => (Thickness)GetValue(UnitBorderThicknessProperty); set => SetValue(UnitBorderThicknessProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius UnitCornerRadius { get => (CornerRadius)GetValue(UnitCornerRadiusProperty); set => SetValue(UnitCornerRadiusProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font family.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font family.")]
        public FontFamily UnitFontFamily { get => (FontFamily)GetValue(UnitFontFamilyProperty); set => SetValue(UnitFontFamilyProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double UnitFontSize { get => (double)GetValue(UnitFontSizeProperty); set => SetValue(UnitFontSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font weight.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font weight.")]
        public FontWeight UnitFontWeight { get => (FontWeight)GetValue(UnitFontWeightProperty); set => SetValue(UnitFontWeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the font style.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the font style.")]
        public FontStyle UnitFontStyle { get => (FontStyle)GetValue(UnitFontStyleProperty); set => SetValue(UnitFontStyleProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the padding.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the padding.")]
        public Thickness UnitPadding { get => (Thickness)GetValue(UnitPaddingProperty); set => SetValue(UnitPaddingProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the text wrapping.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the text wrapping.")]
        public TextWrapping UnitTextWrapping { get => (TextWrapping)GetValue(UnitTextWrappingProperty); set => SetValue(UnitTextWrappingProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the text trimming.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the text trimming.")]
        public TextTrimming UnitTextTrimming { get => (TextTrimming)GetValue(UnitTextTrimmingProperty); set => SetValue(UnitTextTrimmingProperty, value); }

        /// <summary>
        /// KO: Register 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by Register.
        /// </summary>
        private static DependencyProperty Register<T>(string name, T defaultValue) =>
            DependencyProperty.Register(name, typeof(T), typeof(LabeledPanelBase), new FrameworkPropertyMetadata(defaultValue));
    }
}
