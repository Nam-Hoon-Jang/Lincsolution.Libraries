using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 비상 정지 상태와 상태별 이미지를 표시하는 토글 버튼입니다.
    /// EN: A toggle button that displays the emergency-stop state and state-specific images.
    /// </summary>
    [LocalizedDescription("비상 정지 상태와 상태별 이미지를 표시하는 토글 버튼입니다.", "A toggle button that displays the emergency-stop state and state-specific images.")]
    public class EmergencyToggleButton : ToggleButton
    {
        /// <summary>
        /// KO: EmergencyToggleButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the EmergencyToggleButton control.
        /// </summary>
        static EmergencyToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EmergencyToggleButton), new FrameworkPropertyMetadata(typeof(EmergencyToggleButton)));
        }

        /// <summary>
        /// KO: IconWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty IconWidthProperty = DependencyProperty.Register(nameof(IconWidth), typeof(double), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: IconHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty IconHeightProperty = DependencyProperty.Register(nameof(IconHeight), typeof(double), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: IconMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty IconMarginProperty = DependencyProperty.Register(nameof(IconMargin), typeof(Thickness), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: IdleIcon 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IdleIcon dependency property. Gets or sets the icon.
        /// </summary>
        public static readonly DependencyProperty IdleIconProperty = DependencyProperty.Register(nameof(IdleIcon), typeof(ImageSource), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: PressedIcon 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the PressedIcon dependency property. Gets or sets the icon.
        /// </summary>
        public static readonly DependencyProperty PressedIconProperty = DependencyProperty.Register(nameof(PressedIcon), typeof(ImageSource), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: IdleContent 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IdleContent dependency property. Gets or sets the display content.
        /// </summary>
        public static readonly DependencyProperty IdleContentProperty = DependencyProperty.Register(nameof(IdleContent), typeof(object), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: PressedContent 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the PressedContent dependency property. Gets or sets the display content.
        /// </summary>
        public static readonly DependencyProperty PressedContentProperty = DependencyProperty.Register(nameof(PressedContent), typeof(object), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: ContentWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(nameof(ContentWidth), typeof(double), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: ContentHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: ContentMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(EmergencyToggleButton));
        /// <summary>
        /// KO: 체크 상태로 전환될 때 명령에 전달할 매개변수입니다.<br/>
        /// EN: Identifies the command parameter used when the button becomes checked.
        /// </summary>
        public static readonly DependencyProperty CheckedCommandParameterProperty = DependencyProperty.Register(
            nameof(CheckedCommandParameter),
            typeof(object),
            typeof(EmergencyToggleButton),
            new FrameworkPropertyMetadata(null, OnStateCommandParameterChanged));
        /// <summary>
        /// KO: 체크 해제 상태로 전환될 때 명령에 전달할 매개변수입니다.<br/>
        /// EN: Identifies the command parameter used when the button becomes unchecked.
        /// </summary>
        public static readonly DependencyProperty UncheckedCommandParameterProperty = DependencyProperty.Register(
            nameof(UncheckedCommandParameter),
            typeof(object),
            typeof(EmergencyToggleButton),
            new FrameworkPropertyMetadata(null, OnStateCommandParameterChanged));

        private static void OnStateCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (EmergencyToggleButton)d;
            button.SetCurrentValue(
                CommandParameterProperty,
                button.IsChecked == true ? button.UncheckedCommandParameter : button.CheckedCommandParameter);
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
        /// EN: Gets or sets the icon.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the icon.")]
        public ImageSource? IdleIcon
        {
            get => (ImageSource?)GetValue(IdleIconProperty);
            set => SetValue(IdleIconProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the icon.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the icon.")]
        public ImageSource? PressedIcon
        {
            get => (ImageSource?)GetValue(PressedIconProperty);
            set => SetValue(PressedIconProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the display content.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the display content.")]
        public object? IdleContent
        {
            get => GetValue(IdleContentProperty);
            set => SetValue(IdleContentProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the display content.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the display content.")]
        public object? PressedContent
        {
            get => GetValue(PressedContentProperty);
            set => SetValue(PressedContentProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ContentWidth
        {
            get => (double)GetValue(ContentWidthProperty);
            set => SetValue(ContentWidthProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ContentHeight
        {
            get => (double)GetValue(ContentHeightProperty);
            set => SetValue(ContentHeightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness ContentMargin
        {
            get => (Thickness)GetValue(ContentMarginProperty);
            set => SetValue(ContentMarginProperty, value);
        }

        /// <summary>
        /// KO: 체크 상태로 전환될 때 명령에 전달할 매개변수를 가져오거나 설정합니다.<br/>
        /// EN: Gets or sets the command parameter used when the button becomes checked.
        /// </summary>
        [LocalizedDescription("체크 상태로 전환될 때 명령에 전달할 매개변수를 가져오거나 설정합니다.", "Gets or sets the command parameter used when the button becomes checked.")]
        public object? CheckedCommandParameter
        {
            get => GetValue(CheckedCommandParameterProperty);
            set => SetValue(CheckedCommandParameterProperty, value);
        }

        /// <summary>
        /// KO: 체크 해제 상태로 전환될 때 명령에 전달할 매개변수를 가져오거나 설정합니다.<br/>
        /// EN: Gets or sets the command parameter used when the button becomes unchecked.
        /// </summary>
        [LocalizedDescription("체크 해제 상태로 전환될 때 명령에 전달할 매개변수를 가져오거나 설정합니다.", "Gets or sets the command parameter used when the button becomes unchecked.")]
        public object? UncheckedCommandParameter
        {
            get => GetValue(UncheckedCommandParameterProperty);
            set => SetValue(UncheckedCommandParameterProperty, value);
        }

        /// <summary>
        /// KO: 다음 토글 상태에 맞는 명령 매개변수를 선택하고 클릭을 처리합니다.<br/>
        /// EN: Selects the command parameter for the next toggle state and processes the click.
        /// </summary>
        protected override void OnClick()
        {
            SetCurrentValue(
                CommandParameterProperty,
                IsChecked == true ? UncheckedCommandParameter : CheckedCommandParameter);

            base.OnClick();
        }
    }
}
