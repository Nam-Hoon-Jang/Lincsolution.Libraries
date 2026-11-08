using System.Windows;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 제목, 토글 버튼 및 단위를 한 행에 표시합니다.
    /// EN: Displays a subject, toggle button, and unit in one row.
    /// </summary>
    public class ToggleButtonPanel : LabeledPanelBase
    {
        static ToggleButtonPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButtonPanel), new FrameworkPropertyMetadata(typeof(ToggleButtonPanel)));
        }

        /// <summary>
        /// KO: IsChecked 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IsChecked dependency property. Gets or sets the IsChecked value.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = Register<bool?>(nameof(IsChecked), false);
        /// <summary>
        /// KO: ToggleWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ToggleWidthProperty = Register(nameof(ToggleWidth), 64d);
        /// <summary>
        /// KO: ToggleHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ToggleHeightProperty = Register(nameof(ToggleHeight), 32d);
        /// <summary>
        /// KO: ToggleCornerRadius 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleCornerRadius dependency property. Gets or sets the corner radius.
        /// </summary>
        public static readonly DependencyProperty ToggleCornerRadiusProperty = Register(nameof(ToggleCornerRadius), new CornerRadius(16));
        /// <summary>
        /// KO: ToggleBorderBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBorderBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty ToggleBorderBrushProperty = Register<Brush?>(nameof(ToggleBorderBrush), null);
        /// <summary>
        /// KO: ToggleBorderThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBorderThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty ToggleBorderThicknessProperty = Register(nameof(ToggleBorderThickness), new Thickness(1));
        /// <summary>
        /// KO: CheckedBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CheckedBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty CheckedBackgroundProperty = Register<Brush?>(nameof(CheckedBackground), null);
        /// <summary>
        /// KO: UncheckedBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UncheckedBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty UncheckedBackgroundProperty = Register<Brush?>(nameof(UncheckedBackground), null);
        /// <summary>
        /// KO: ThumbBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ThumbBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty ThumbBrushProperty = Register<Brush?>(nameof(ThumbBrush), null);
        public static readonly DependencyProperty CheckedThumbBrushProperty = Register<Brush?>(nameof(CheckedThumbBrush), null);
        public static readonly DependencyProperty UncheckedThumbBrushProperty = Register<Brush?>(nameof(UncheckedThumbBrush), null);
        /// <summary>
        /// KO: ThumbSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ThumbSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty ThumbSizeProperty = Register(nameof(ThumbSize), 24d);
        /// <summary>
        /// KO: ThumbMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ThumbMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ThumbMarginProperty = Register(nameof(ThumbMargin), new Thickness(3));

        [LocalizedDescription("토글 선택 상태를 가져오거나 설정합니다.", "Gets or sets the checked state of the toggle.")]
        public bool? IsChecked { get => (bool?)GetValue(IsCheckedProperty); set => SetValue(IsCheckedProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ToggleWidth { get => (double)GetValue(ToggleWidthProperty); set => SetValue(ToggleWidthProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ToggleHeight { get => (double)GetValue(ToggleHeightProperty); set => SetValue(ToggleHeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the corner radius.")]
        public CornerRadius ToggleCornerRadius { get => (CornerRadius)GetValue(ToggleCornerRadiusProperty); set => SetValue(ToggleCornerRadiusProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? ToggleBorderBrush { get => (Brush?)GetValue(ToggleBorderBrushProperty); set => SetValue(ToggleBorderBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public Thickness ToggleBorderThickness { get => (Thickness)GetValue(ToggleBorderThicknessProperty); set => SetValue(ToggleBorderThicknessProperty, value); }
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
        public Brush? UncheckedBackground { get => (Brush?)GetValue(UncheckedBackgroundProperty); set => SetValue(UncheckedBackgroundProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? ThumbBrush { get => (Brush?)GetValue(ThumbBrushProperty); set => SetValue(ThumbBrushProperty, value); }
        [LocalizedDescription("선택 상태의 Thumb 브러시를 가져오거나 설정합니다.", "Gets or sets the thumb brush used when checked.")]
        public Brush? CheckedThumbBrush { get => (Brush?)GetValue(CheckedThumbBrushProperty); set => SetValue(CheckedThumbBrushProperty, value); }
        [LocalizedDescription("선택되지 않은 상태의 Thumb 브러시를 가져오거나 설정합니다.", "Gets or sets the thumb brush used when unchecked.")]
        public Brush? UncheckedThumbBrush { get => (Brush?)GetValue(UncheckedThumbBrushProperty); set => SetValue(UncheckedThumbBrushProperty, value); }
        [LocalizedDescription("Thumb 크기를 가져오거나 설정합니다.", "Gets or sets the thumb size.")]
        public double ThumbSize { get => (double)GetValue(ThumbSizeProperty); set => SetValue(ThumbSizeProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ThumbMargin { get => (Thickness)GetValue(ThumbMarginProperty); set => SetValue(ThumbMarginProperty, value); }

        /// <summary>
        /// KO: Register 관련 처리를 수행합니다.
        /// EN: Performs the operation implemented by Register.
        /// </summary>
        private static DependencyProperty Register<T>(string name, T defaultValue) =>
            DependencyProperty.Register(name, typeof(T), typeof(ToggleButtonPanel), new FrameworkPropertyMetadata(defaultValue));
    }
}
