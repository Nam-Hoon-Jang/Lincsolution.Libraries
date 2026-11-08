using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 알람 목록과 전체 삭제 명령을 표시하는 팝업입니다.
    /// EN: A popup that displays an alarm list and a clear-all command.
    /// </summary>
    [LocalizedDescription("알람 목록과 전체 삭제 명령을 표시하는 팝업입니다.", "A popup that displays an alarm list and a clear-all command.")]
    [TemplatePart(Name = ContentListPartName, Type = typeof(ListView))]
    public class AlarmListPopup : Control
    {
        private const string ContentListPartName = "PART_ContentList";
        private ListView? _contentList;

        /// <summary>
        /// KO: AlarmListPopup 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the AlarmListPopup control.
        /// </summary>
        static AlarmListPopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(AlarmListPopup),
                new FrameworkPropertyMetadata(typeof(AlarmListPopup)));
        }

        /// <summary>
        /// KO: AlarmListPopup 인스턴스를 초기화하고 버튼 클릭 처리기를 등록합니다.
        /// EN: Initializes an AlarmListPopup instance and registers its button click handler.
        /// </summary>
        public AlarmListPopup()
        {
            AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
        }

        /// <summary>
        /// KO: ItemsSource 종속성 속성 식별자입니다. 표시할 항목 컬렉션을 가져오거나 설정합니다.
        /// EN: Identifies the ItemsSource dependency property. Gets or sets the collection of items to display.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(IEnumerable),
                typeof(AlarmListPopup));

        /// <summary>
        /// KO: Command 종속성 속성 식별자입니다. 버튼에서 실행할 명령을 가져오거나 설정합니다.
        /// EN: Identifies the Command dependency property. Gets or sets the command executed by the buttons.
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(AlarmListPopup));

        /// <summary>
        /// KO: ClearCommandParameter 종속성 속성 식별자입니다. 전체 삭제 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Identifies the ClearCommandParameter dependency property. Gets or sets the parameter passed to the clear-all command.
        /// </summary>
        public static readonly DependencyProperty ClearCommandParameterProperty =
            DependencyProperty.Register(
                nameof(ClearCommandParameter),
                typeof(object),
                typeof(AlarmListPopup));

        /// <summary>
        /// KO: ClearContent 종속성 속성 식별자입니다. 전체 삭제 버튼에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Identifies the ClearContent dependency property. Gets or sets the content displayed by the clear-all button.
        /// </summary>
        public static readonly DependencyProperty ClearContentProperty =
            DependencyProperty.Register(
                nameof(ClearContent),
                typeof(object),
                typeof(AlarmListPopup),
                new FrameworkPropertyMetadata("Clear"));

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(AlarmListPopup),
                new FrameworkPropertyMetadata(new CornerRadius(10)));

        /// <summary>
        /// KO: HeaderHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the HeaderHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(GridLength),
                typeof(AlarmListPopup),
                new FrameworkPropertyMetadata(new GridLength(48)));

        /// <summary>
        /// KO: ItemHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ItemHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                nameof(ItemHeight),
                typeof(double),
                typeof(AlarmListPopup),
                new FrameworkPropertyMetadata(60d));

        /// <summary>
        /// KO: 표시할 항목 컬렉션을 가져오거나 설정합니다.
        /// EN: Gets or sets the collection of items to display.
        /// </summary>
        [LocalizedDescription("표시할 항목 컬렉션을 가져오거나 설정합니다.", "Gets or sets the collection of items to display.")]
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
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
        /// KO: 전체 삭제 명령에 전달할 매개변수를 가져오거나 설정합니다.
        /// EN: Gets or sets the parameter passed to the clear-all command.
        /// </summary>
        [LocalizedDescription("전체 삭제 명령에 전달할 매개변수를 가져오거나 설정합니다.", "Gets or sets the parameter passed to the clear-all command.")]
        public object? ClearCommandParameter
        {
            get => GetValue(ClearCommandParameterProperty);
            set => SetValue(ClearCommandParameterProperty, value);
        }

        /// <summary>
        /// KO: 전체 삭제 버튼에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Gets or sets the content displayed by the clear-all button.
        /// </summary>
        [LocalizedDescription("전체 삭제 버튼에 표시할 내용을 가져오거나 설정합니다.", "Gets or sets the content displayed by the clear-all button.")]
        public object? ClearContent
        {
            get => GetValue(ClearContentProperty);
            set => SetValue(ClearContentProperty, value);
        }

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
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public GridLength HeaderHeight
        {
            get => (GridLength)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public event EventHandler<int>? ButtonClickedEvent;

        /// <summary>
        /// KO: 컨트롤 템플릿을 적용하고 필요한 템플릿 요소와 표시 상태를 초기화합니다.
        /// EN: Applies the control template and initializes the required template parts and visual state.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentList = GetTemplateChild(ContentListPartName) as ListView;
        }

        /// <summary>
        /// KO: 컨트롤 내부 버튼 클릭을 처리합니다.
        /// EN: Handles a button click raised within the control.
        /// </summary>
        /// <param name="sender">KO: 이벤트를 발생시킨 개체입니다. EN: The object that raised the event.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not Button { Tag: string tag } button ||
                tag != "AlarmItem" ||
                _contentList is null)
            {
                return;
            }

            int index = _contentList.Items.IndexOf(button.DataContext);
            if (index >= 0)
                ButtonClickedEvent?.Invoke(this, index);
        }
    }

}
