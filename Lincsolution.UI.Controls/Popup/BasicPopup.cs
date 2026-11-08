using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AlarmTypeEnum = Lincsolution.UI.Base.Common.AlarmType;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 알람 종류와 단일 또는 이중 버튼 구성을 지원하는 기본 팝업입니다.
    /// EN: A basic popup supporting alarm types and one- or two-button layouts.
    /// </summary>
    [LocalizedDescription("알람 종류와 단일 또는 이중 버튼 구성을 지원하는 기본 팝업입니다.", "A basic popup supporting alarm types and one- or two-button layouts.")]
    public class BasicPopup : Control
    {
        /// <summary>
        /// KO: BasicPopup 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicPopup control.
        /// </summary>
        static BasicPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(typeof(BasicPopup)));
        }

        /// <summary>
        /// KO: AlarmType 종속성 속성 식별자입니다. 표시할 알람 종류를 가져오거나 설정합니다.
        /// EN: Identifies the AlarmType dependency property. Gets or sets the alarm type to display.
        /// </summary>
        public static readonly DependencyProperty AlarmTypeProperty =
            DependencyProperty.Register(
                nameof(AlarmType),
                typeof(AlarmTypeEnum),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(AlarmTypeEnum.INFO));

        /// <summary>
        /// KO: IsSingleButton 종속성 속성 식별자입니다. 팝업이 버튼 하나만 사용할지 여부를 가져오거나 설정합니다.
        /// EN: Identifies the IsSingleButton dependency property. Gets or sets whether the popup uses a single button.
        /// </summary>
        public static readonly DependencyProperty IsSingleButtonProperty =
            DependencyProperty.Register(
                nameof(IsSingleButton),
                typeof(bool),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// KO: PopupContent 종속성 속성 식별자입니다. 팝업 본문에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Identifies the PopupContent dependency property. Gets or sets the content displayed in the popup body.
        /// </summary>
        public static readonly DependencyProperty PopupContentProperty =
            DependencyProperty.Register(
                nameof(PopupContent),
                typeof(object),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: Command 종속성 속성 식별자입니다. 버튼에서 실행할 명령을 가져오거나 설정합니다.
        /// EN: Identifies the Command dependency property. Gets or sets the command executed by the buttons.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: CmdParBtnLeft 종속성 속성 식별자입니다. 왼쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Identifies the CmdParBtnLeft dependency property. Gets or sets the parameter passed to the left button command.
        /// </summary>
        public static readonly DependencyProperty CmdParBtnLeftProperty =
            DependencyProperty.Register(
                nameof(CmdParBtnLeft),
                typeof(object),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: CmdParBtnRight 종속성 속성 식별자입니다. 오른쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Identifies the CmdParBtnRight dependency property. Gets or sets the parameter passed to the right button command.
        /// </summary>
        public static readonly DependencyProperty CmdParBtnRightProperty =
            DependencyProperty.Register(
                nameof(CmdParBtnRight),
                typeof(object),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: ButtonWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ButtonWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(
                nameof(ButtonWidth),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(100d));

        /// <summary>
        /// KO: ButtonHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ButtonHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(
                nameof(ButtonHeight),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(40d));

        /// <summary>
        /// KO: TitleFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TitleFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty TitleFontSizeProperty =
            DependencyProperty.Register(
                nameof(TitleFontSize),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(20d));

        /// <summary>
        /// KO: ContentFontSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ContentFontSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty ContentFontSizeProperty =
            DependencyProperty.Register(
                nameof(ContentFontSize),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(16d));

        /// <summary>
        /// KO: ButtonSpacing 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ButtonSpacing dependency property. Gets or sets the ButtonSpacing value.
        /// </summary>
        public static readonly DependencyProperty ButtonSpacingProperty =
            DependencyProperty.Register(
                nameof(ButtonSpacing),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(16d));

        /// <summary>
        /// KO: AccentHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the AccentHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty AccentHeightProperty =
            DependencyProperty.Register(
                nameof(AccentHeight),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(12d));

        /// <summary>
        /// KO: PopupWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the PopupWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register(
                nameof(PopupWidth),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(380d));

        /// <summary>
        /// KO: PopupHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the PopupHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty PopupHeightProperty =
            DependencyProperty.Register(
                nameof(PopupHeight),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(300d));

        /// <summary>
        /// KO: OverlayBackground 종속성 속성 식별자입니다. 팝업 뒤 오버레이의 배경 브러시를 가져오거나 설정합니다.
        /// EN: Identifies the OverlayBackground dependency property. Gets or sets the background brush of the popup overlay.
        /// </summary>
        public static readonly DependencyProperty OverlayBackgroundProperty =
            DependencyProperty.Register(
                nameof(OverlayBackground),
                typeof(Brush),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// KO: OverlayWidth 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the OverlayWidth dependency property. Gets or sets the width.
        /// </summary>
        public static readonly DependencyProperty OverlayWidthProperty =
            DependencyProperty.Register(
                nameof(OverlayWidth),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(double.NaN));

        /// <summary>
        /// KO: OverlayHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the OverlayHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty OverlayHeightProperty =
            DependencyProperty.Register(
                nameof(OverlayHeight),
                typeof(double),
                typeof(BasicPopup),
                new FrameworkPropertyMetadata(double.NaN));

        /// <summary>
        /// KO: 표시할 알람 종류를 가져오거나 설정합니다.
        /// EN: Gets or sets the alarm type to display.
        /// </summary>
        [LocalizedDescription("표시할 알람 종류를 가져오거나 설정합니다.", "Gets or sets the alarm type to display.")]
        public AlarmTypeEnum AlarmType
        {
            get => (AlarmTypeEnum)GetValue(AlarmTypeProperty);
            set => SetValue(AlarmTypeProperty, value);
        }

        /// <summary>
        /// KO: 팝업이 버튼 하나만 사용할지 여부를 가져오거나 설정합니다.
        /// EN: Gets or sets whether the popup uses a single button.
        /// </summary>
        [LocalizedDescription("팝업이 버튼 하나만 사용할지 여부를 가져오거나 설정합니다.", "Gets or sets whether the popup uses a single button.")]
        public bool IsSingleButton
        {
            get => (bool)GetValue(IsSingleButtonProperty);
            set => SetValue(IsSingleButtonProperty, value);
        }

        /// <summary>
        /// KO: 팝업 본문에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Gets or sets the content displayed in the popup body.
        /// </summary>
        [LocalizedDescription("팝업 본문에 표시할 내용을 가져오거나 설정합니다.", "Gets or sets the content displayed in the popup body.")]
        public object? PopupContent
        {
            get => GetValue(PopupContentProperty);
            set => SetValue(PopupContentProperty, value);
        }

        /// <summary>
        /// KO: 버튼에서 실행할 명령을 가져오거나 설정합니다.
        /// EN: Gets or sets the command executed by the buttons.
        /// </summary>
        [LocalizedDescription("버튼에서 실행할 명령을 가져오거나 설정합니다.", "Gets or sets the command executed by the buttons.")]
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// KO: 왼쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Gets or sets the parameter passed to the left button command.
        /// </summary>
        [LocalizedDescription("왼쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.", "Gets or sets the parameter passed to the left button command.")]
        public object? CmdParBtnLeft
        {
            get => GetValue(CmdParBtnLeftProperty);
            set => SetValue(CmdParBtnLeftProperty, value);
        }

        /// <summary>
        /// KO: 오른쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Gets or sets the parameter passed to the right button command.
        /// </summary>
        [LocalizedDescription("오른쪽 버튼 명령에 전달할 매개변수를 가져오거나 설정합니다.", "Gets or sets the parameter passed to the right button command.")]
        public object? CmdParBtnRight
        {
            get => GetValue(CmdParBtnRightProperty);
            set => SetValue(CmdParBtnRightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double ButtonWidth
        {
            get => (double)GetValue(ButtonWidthProperty);
            set => SetValue(ButtonWidthProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ButtonHeight
        {
            get => (double)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set => SetValue(TitleFontSizeProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double ContentFontSize
        {
            get => (double)GetValue(ContentFontSizeProperty);
            set => SetValue(ContentFontSizeProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the ButtonSpacing value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the ButtonSpacing value.")]
        public double ButtonSpacing
        {
            get => (double)GetValue(ButtonSpacingProperty);
            set => SetValue(ButtonSpacingProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double AccentHeight
        {
            get => (double)GetValue(AccentHeightProperty);
            set => SetValue(AccentHeightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double PopupWidth
        {
            get => (double)GetValue(PopupWidthProperty);
            set => SetValue(PopupWidthProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double PopupHeight
        {
            get => (double)GetValue(PopupHeightProperty);
            set => SetValue(PopupHeightProperty, value);
        }

        /// <summary>
        /// KO: 팝업 뒤 오버레이의 배경 브러시를 가져오거나 설정합니다.
        /// EN: Gets or sets the background brush of the popup overlay.
        /// </summary>
        [LocalizedDescription("팝업 뒤 오버레이의 배경 브러시를 가져오거나 설정합니다.", "Gets or sets the background brush of the popup overlay.")]
        public Brush? OverlayBackground
        {
            get => (Brush?)GetValue(OverlayBackgroundProperty);
            set => SetValue(OverlayBackgroundProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the width.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the width.")]
        public double OverlayWidth
        {
            get => (double)GetValue(OverlayWidthProperty);
            set => SetValue(OverlayWidthProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double OverlayHeight
        {
            get => (double)GetValue(OverlayHeightProperty);
            set => SetValue(OverlayHeightProperty, value);
        }
    }
}
