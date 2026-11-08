using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Lincsolution.UI.Resources;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 테마 선택 상태를 아이콘과 함께 표시하는 토글 버튼입니다.
    /// EN: A toggle button that displays theme selection with an icon.
    /// </summary>
    [LocalizedDescription("테마 선택 상태를 아이콘과 함께 표시하는 토글 버튼입니다.", "A toggle button that displays theme selection with an icon.")]
    public class ThemeToggleButton : ToggleButton
    {
        /// <summary>
        /// KO: ThemeToggleButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the ThemeToggleButton control.
        /// </summary>
        static ThemeToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThemeToggleButton), new FrameworkPropertyMetadata(typeof(ThemeToggleButton)));
        }

        /// <summary>
        /// KO: ToggleBoxWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBoxWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ToggleBoxWidthProperty =
            DependencyProperty.Register(nameof(ToggleBoxWidth),
            typeof(double),
            typeof(ThemeToggleButton));
        
        /// <summary>
        /// KO: ToggleBoxHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBoxHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ToggleBoxHeightProperty =
            DependencyProperty.Register(nameof(ToggleBoxHeight),
            typeof(double),
            typeof(ThemeToggleButton));
        
        /// <summary>
        /// KO: ToggleBoxFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBoxFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty ToggleBoxFillProperty =
            DependencyProperty.Register(nameof(ToggleBoxFill),
            typeof(Brush),
            typeof(ThemeToggleButton));
        
        /// <summary>
        /// KO: ToggleBoxStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBoxStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty ToggleBoxStrokeProperty =
            DependencyProperty.Register(nameof(ToggleBoxStroke),
            typeof(Brush),
            typeof(ThemeToggleButton));
        
        /// <summary>
        /// KO: ToggleBoxStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ToggleBoxStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty ToggleBoxStrokeThicknessProperty =
            DependencyProperty.Register(nameof(ToggleBoxStrokeThickness),
            typeof(double),
            typeof(ThemeToggleButton));
        
        /// <summary>
        /// KO: IconSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register(nameof(IconSize),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: IconLeftPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconLeftPos dependency property. Gets or sets the IconLeftPos value.
        /// </summary>
        public static readonly DependencyProperty IconLeftPosProperty =
            DependencyProperty.Register(nameof(IconLeftPos),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: IconRightPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconRightPos dependency property. Gets or sets the IconRightPos value.
        /// </summary>
        public static readonly DependencyProperty IconRightPosProperty =
            DependencyProperty.Register(nameof(IconRightPos),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: IconTextLeftPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconTextLeftPos dependency property. Gets or sets the IconTextLeftPos value.
        /// </summary>
        public static readonly DependencyProperty IconTextLeftPosProperty =
            DependencyProperty.Register(nameof(IconTextLeftPos),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: IconTextRightPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconTextRightPos dependency property. Gets or sets the IconTextRightPos value.
        /// </summary>
        public static readonly DependencyProperty IconTextRightPosProperty =
            DependencyProperty.Register(nameof(IconTextRightPos),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: TextBlockWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty TextBlockWidthProperty =
            DependencyProperty.Register(nameof(TextBlockWidth),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <summary>
        /// KO: TextBlockHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty TextBlockHeightProperty =
            DependencyProperty.Register(nameof(TextBlockHeight),
            typeof(double),
            typeof(ThemeToggleButton));

        /// <inheritdoc/>
        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            ThemeManager.ApplyTheme(UiTheme.Dark);
        }

        /// <inheritdoc/>
        protected override void OnUnchecked(RoutedEventArgs e)
        {
            base.OnUnchecked(e);
            ThemeManager.ApplyTheme(UiTheme.Light);
        }


        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ToggleBoxWidth
        {
            get => (double)GetValue(ToggleBoxWidthProperty);
            set => SetValue(ToggleBoxWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ToggleBoxHeight
        {
            get => (double)GetValue(ToggleBoxHeightProperty);
            set => SetValue(ToggleBoxHeightProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? ToggleBoxFill
        {
            get => (Brush?)GetValue(ToggleBoxFillProperty);
            set => SetValue(ToggleBoxFillProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? ToggleBoxStroke
        {
            get => (Brush?)GetValue(ToggleBoxStrokeProperty);
            set => SetValue(ToggleBoxStrokeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double ToggleBoxStrokeThickness
        {
            get => (double)GetValue(ToggleBoxStrokeThicknessProperty);
            set => SetValue(ToggleBoxStrokeThicknessProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double? IconSize
        {
            get => (double?)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the IconLeftPos value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IconLeftPos value.")]
        public double IconLeftPos
        {
            get => (double)GetValue(IconLeftPosProperty);
            set => SetValue(IconLeftPosProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the IconRightPos value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IconRightPos value.")]
        public double IconRightPos
        {
            get => (double)GetValue(IconRightPosProperty);
            set => SetValue(IconRightPosProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the IconTextLeftPos value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IconTextLeftPos value.")]
        public double IconTextLeftPos
        {
            get => (double)GetValue(IconTextLeftPosProperty);
            set => SetValue(IconTextLeftPosProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the IconTextRightPos value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IconTextRightPos value.")]
        public double IconTextRightPos
        {
            get => (double)GetValue(IconTextRightPosProperty);
            set => SetValue(IconTextRightPosProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double TextBlockWidth
        {
            get => (double)GetValue(TextBlockWidthProperty);
            set => SetValue(TextBlockWidthProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double TextBlockHeight
        {
            get => (double)GetValue(TextBlockHeightProperty);
            set => SetValue(TextBlockHeightProperty, value);
        }
    }
}
