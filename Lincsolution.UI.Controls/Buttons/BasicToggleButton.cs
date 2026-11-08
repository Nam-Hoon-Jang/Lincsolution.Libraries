using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 선택 상태에 따라 썸 위치와 색상이 바뀌는 기본 토글 버튼입니다.
    /// EN: A basic toggle button whose thumb position and colors reflect its checked state.
    /// </summary>
    [LocalizedDescription("선택 상태에 따라 썸 위치와 색상이 바뀌는 기본 토글 버튼입니다.", "A basic toggle button whose thumb position and colors reflect its checked state.")]
    public class BasicToggleButton : ToggleButton
    {
        /// <summary>
        /// KO: BasicToggleButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicToggleButton control.
        /// </summary>
        static BasicToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BasicToggleButton),
                new FrameworkPropertyMetadata(typeof(BasicToggleButton)));
        }

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(BasicToggleButton),
                new FrameworkPropertyMetadata(new CornerRadius(16)));

        /// <summary>
        /// KO: CheckedBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the CheckedBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty CheckedBackgroundProperty =
            DependencyProperty.Register(
                nameof(CheckedBackground),
                typeof(Brush),
                typeof(BasicToggleButton));

        /// <summary>
        /// KO: UncheckedBackground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the UncheckedBackground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty UncheckedBackgroundProperty =
            DependencyProperty.Register(
                nameof(UncheckedBackground),
                typeof(Brush),
                typeof(BasicToggleButton));

        /// <summary>
        /// KO: ThumbBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ThumbBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty ThumbBrushProperty =
            DependencyProperty.Register(
                nameof(ThumbBrush),
                typeof(Brush),
                typeof(BasicToggleButton));

        /// <summary>
        /// KO: CheckedThumbBrush 종속성 속성 식별자입니다. 선택 상태의 Thumb 브러시를 가져오거나 설정합니다.
        /// EN: Identifies the CheckedThumbBrush dependency property. Gets or sets the thumb brush used when checked.
        /// </summary>
        public static readonly DependencyProperty CheckedThumbBrushProperty =
            DependencyProperty.Register(
                nameof(CheckedThumbBrush),
                typeof(Brush),
                typeof(BasicToggleButton));

        /// <summary>
        /// KO: UncheckedThumbBrush 종속성 속성 식별자입니다. 선택되지 않은 상태의 Thumb 브러시를 가져오거나 설정합니다.
        /// EN: Identifies the UncheckedThumbBrush dependency property. Gets or sets the thumb brush used when unchecked.
        /// </summary>
        public static readonly DependencyProperty UncheckedThumbBrushProperty =
            DependencyProperty.Register(
                nameof(UncheckedThumbBrush),
                typeof(Brush),
                typeof(BasicToggleButton));

        /// <summary>
        /// KO: ThumbSize 종속성 속성 식별자입니다. Thumb 크기를 가져오거나 설정합니다.
        /// EN: Identifies the ThumbSize dependency property. Gets or sets the thumb size.
        /// </summary>
        public static readonly DependencyProperty ThumbSizeProperty =
            DependencyProperty.Register(
                nameof(ThumbSize),
                typeof(double),
                typeof(BasicToggleButton),
                new FrameworkPropertyMetadata(24d));

        /// <summary>
        /// KO: ThumbMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ThumbMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ThumbMarginProperty =
            DependencyProperty.Register(
                nameof(ThumbMargin),
                typeof(Thickness),
                typeof(BasicToggleButton),
                new FrameworkPropertyMetadata(new Thickness(3)));

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
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? CheckedBackground
        {
            get => (Brush?)GetValue(CheckedBackgroundProperty);
            set => SetValue(CheckedBackgroundProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? UncheckedBackground
        {
            get => (Brush?)GetValue(UncheckedBackgroundProperty);
            set => SetValue(UncheckedBackgroundProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? ThumbBrush
        {
            get => (Brush?)GetValue(ThumbBrushProperty);
            set => SetValue(ThumbBrushProperty, value);
        }

        /// <summary>
        /// KO: 선택 상태의 Thumb 브러시를 가져오거나 설정합니다.
        /// EN: Gets or sets the thumb brush used when checked.
        /// </summary>
        [LocalizedDescription("선택 상태의 Thumb 브러시를 가져오거나 설정합니다.", "Gets or sets the thumb brush used when checked.")]
        public Brush? CheckedThumbBrush
        {
            get => (Brush?)GetValue(CheckedThumbBrushProperty);
            set => SetValue(CheckedThumbBrushProperty, value);
        }

        /// <summary>
        /// KO: 선택되지 않은 상태의 Thumb 브러시를 가져오거나 설정합니다.
        /// EN: Gets or sets the thumb brush used when unchecked.
        /// </summary>
        [LocalizedDescription("선택되지 않은 상태의 Thumb 브러시를 가져오거나 설정합니다.", "Gets or sets the thumb brush used when unchecked.")]
        public Brush? UncheckedThumbBrush
        {
            get => (Brush?)GetValue(UncheckedThumbBrushProperty);
            set => SetValue(UncheckedThumbBrushProperty, value);
        }

        /// <summary>
        /// KO: Thumb 크기를 가져오거나 설정합니다.
        /// EN: Gets or sets the thumb size.
        /// </summary>
        [LocalizedDescription("Thumb 크기를 가져오거나 설정합니다.", "Gets or sets the thumb size.")]
        public double ThumbSize
        {
            get => (double)GetValue(ThumbSizeProperty);
            set => SetValue(ThumbSizeProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ThumbMargin
        {
            get => (Thickness)GetValue(ThumbMarginProperty);
            set => SetValue(ThumbMarginProperty, value);
        }
    }
}
