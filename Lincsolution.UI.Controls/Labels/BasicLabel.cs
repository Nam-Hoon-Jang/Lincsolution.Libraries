using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 상태 아이콘과 상태별 사용자 지정 텍스트를 함께 표시하는 라벨입니다.
    /// EN: A label that displays a status icon and customizable text for each state.
    /// </summary>
    [LocalizedDescription("상태 아이콘과 상태별 사용자 지정 텍스트를 함께 표시하는 라벨입니다.", "A label that displays a status icon and customizable text for each state.")]
    public class BasicLabel : Label
    {
        /// <summary>
        /// KO: StatusLabel 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the StatusLabel control.
        /// </summary>
        static BasicLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicLabel), new FrameworkPropertyMetadata(typeof(BasicLabel)));
        }
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = 
            DependencyProperty.Register(nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(BasicLabel));

        /// <summary>
        /// KO: IconImage 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconImage dependency property. Gets or sets the icon.
        /// </summary>
        public static readonly DependencyProperty IconImageProperty = 
            DependencyProperty.Register(nameof(IconImage),
            typeof(ImageSource),
            typeof(BasicLabel));

        /// <summary>
        /// KO: IconSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty = 
            DependencyProperty.Register(nameof(IconSize),
            typeof(double),
            typeof(BasicLabel));

        /// <summary>
        /// KO: IconLeftPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconLeftPos dependency property. Gets or sets the IconLeftPos value.
        /// </summary>
        public static readonly DependencyProperty IconLeftPosProperty = 
            DependencyProperty.Register(nameof(IconLeftPos),
            typeof(double),
            typeof(BasicLabel));

        /// <summary>
        /// KO: TextBlockLeftPos 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockLeftPos dependency property. Gets or sets the TextBlockLeftPos value.
        /// </summary>
        public static readonly DependencyProperty TextBlockLeftPosProperty = 
            DependencyProperty.Register(nameof(TextBlockLeftPos),
            typeof(double),
            typeof(BasicLabel));

        /// <summary>
        /// KO: TextBlockWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty TextBlockWidthProperty = 
            DependencyProperty.Register(nameof(TextBlockWidth),
            typeof(double),
            typeof(BasicLabel));

        /// <summary>
        /// KO: TextBlockHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty TextBlockHeightProperty = 
            DependencyProperty.Register(nameof(TextBlockHeight),
            typeof(double),
            typeof(BasicLabel));


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
        /// EN: Gets or sets the icon.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the icon.")]
        public ImageSource? IconImage
        {
            get => (ImageSource?)GetValue(IconImageProperty);
            set => SetValue(IconImageProperty, value);
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
        /// EN: Gets or sets the TextBlockLeftPos value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the TextBlockLeftPos value.")]
        public double TextBlockLeftPos
        {
            get => (double)GetValue(TextBlockLeftPosProperty);
            set => SetValue(TextBlockLeftPosProperty, value);
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

        /// <summary>KO: IDLE 상태 텍스트 DP입니다. EN: Dependency property for IDLE state text.</summary>
        public static readonly DependencyProperty IdleTextProperty = DependencyProperty.Register(nameof(IdleText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty BusyTextProperty = DependencyProperty.Register(nameof(BusyText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty ReadyTextProperty = DependencyProperty.Register(nameof(ReadyText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty BuildTextProperty = DependencyProperty.Register(nameof(BuildText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty PauseTextProperty = DependencyProperty.Register(nameof(PauseText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty DoneTextProperty = DependencyProperty.Register(nameof(DoneText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty StopTextProperty = DependencyProperty.Register(nameof(StopText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty WarningTextProperty = DependencyProperty.Register(nameof(WarningText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));
        public static readonly DependencyProperty ErrorTextProperty = DependencyProperty.Register(nameof(ErrorText), typeof(string), typeof(BasicLabel), new PropertyMetadata(""));

        public string IdleText { get => (string)GetValue(IdleTextProperty); set => SetValue(IdleTextProperty, value); }
        public string BusyText { get => (string)GetValue(BusyTextProperty); set => SetValue(BusyTextProperty, value); }
        public string ReadyText { get => (string)GetValue(ReadyTextProperty); set => SetValue(ReadyTextProperty, value); }
        public string BuildText { get => (string)GetValue(BuildTextProperty); set => SetValue(BuildTextProperty, value); }
        public string PauseText { get => (string)GetValue(PauseTextProperty); set => SetValue(PauseTextProperty, value); }
        public string DoneText { get => (string)GetValue(DoneTextProperty); set => SetValue(DoneTextProperty, value); }
        public string StopText { get => (string)GetValue(StopTextProperty); set => SetValue(StopTextProperty, value); }
        public string WarningText { get => (string)GetValue(WarningTextProperty); set => SetValue(WarningTextProperty, value); }
        public string ErrorText { get => (string)GetValue(ErrorTextProperty); set => SetValue(ErrorTextProperty, value); }
    }
}

