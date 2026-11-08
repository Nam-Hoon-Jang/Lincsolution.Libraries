using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Lincsolution.UI.Base.Common;
using Lincsolution.UI.Controls;
using Lincsolution.UI.Base.Popup;
using Brush = System.Windows.Media.Brush;
using UserControl = System.Windows.Controls.UserControl;
using Point = System.Windows.Point;

namespace Lincsolution.UI
{
    /// <summary>
    /// BaseView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BaseView : UserControl
    {
        private static readonly RoutedCommand DefaultAlarmListClearCommand = new();
        private static readonly RoutedCommand DefaultPopupResponseCommand = new();

        public BaseView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SetBinding(AlarmItemsSourceProperty, new Binding("Shared.AlarmList"));
            SetBinding(EmergencyCommandProperty, new Binding("EmergencyToggleCommand"));
            CommandBindings.Add(new CommandBinding(DefaultAlarmListClearCommand, ClearAlarmList));
            CommandBindings.Add(new CommandBinding(DefaultPopupResponseCommand, HandlePopupResponse));
            SetCurrentValue(AlarmListClearCommandProperty, DefaultAlarmListClearCommand);
            SetResourceReference(PopupBackgroundProperty, "Brush.MainBackground");
            SetResourceReference(PopupForegroundProperty, "Brush.ForegroundNormal");
            SetResourceReference(PopupOverlayBackgroundProperty, "Brush.PopupBackground");
        }

        public static readonly DependencyProperty EmergencyCommandProperty = DependencyProperty.Register(nameof(EmergencyCommand), typeof(ICommand), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty EmergencyCommandParameterProperty = DependencyProperty.Register(nameof(EmergencyCommandParameter), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty EmergencyResetCommandParameterProperty = DependencyProperty.Register(nameof(EmergencyResetCommandParameter), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(UiState), typeof(BaseView), new FrameworkPropertyMetadata(UiState.IDLE));
        public static readonly DependencyProperty ProgressValueProperty = DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(0.0d));
        public static readonly DependencyProperty MainButtonContentProperty = DependencyProperty.Register(nameof(MainButtonContent), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty SubViewContentProperty = DependencyProperty.Register(nameof(SubViewContent), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupContentProperty = DependencyProperty.Register(nameof(PopupContent), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupVisibilityProperty = DependencyProperty.Register(
            nameof(PopupVisibility),
            typeof(Visibility),
            typeof(BaseView),
            new FrameworkPropertyMetadata(
                Visibility.Collapsed,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty AlarmTypeProperty = DependencyProperty.Register(nameof(AlarmType), typeof(AlarmType), typeof(BaseView), new FrameworkPropertyMetadata(AlarmType.INFO));
        public static readonly DependencyProperty IsSingleButtonProperty = DependencyProperty.Register(nameof(IsSingleButton), typeof(bool), typeof(BaseView), new FrameworkPropertyMetadata(false));
        public static readonly DependencyProperty PopupCommandProperty = DependencyProperty.Register(nameof(PopupCommand), typeof(ICommand), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupLeftCommandParameterProperty = DependencyProperty.Register(nameof(PopupLeftCommandParameter), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupRightCommandParameterProperty = DependencyProperty.Register(nameof(PopupRightCommandParameter), typeof(object), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupWidthProperty = DependencyProperty.Register(nameof(PopupWidth), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(380d));
        public static readonly DependencyProperty PopupHeightProperty = DependencyProperty.Register(nameof(PopupHeight), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(300d));
        public static readonly DependencyProperty PopupButtonWidthProperty = DependencyProperty.Register(nameof(PopupButtonWidth), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(100d));
        public static readonly DependencyProperty PopupButtonHeightProperty = DependencyProperty.Register(nameof(PopupButtonHeight), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(40d));
        public static readonly DependencyProperty PopupTitleFontSizeProperty = DependencyProperty.Register(nameof(PopupTitleFontSize), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(20d));
        public static readonly DependencyProperty PopupContentFontSizeProperty = DependencyProperty.Register(nameof(PopupContentFontSize), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(16d));
        public static readonly DependencyProperty PopupButtonSpacingProperty = DependencyProperty.Register(nameof(PopupButtonSpacing), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(16d));
        public static readonly DependencyProperty PopupAccentHeightProperty = DependencyProperty.Register(nameof(PopupAccentHeight), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(12d));
        public static readonly DependencyProperty PopupBackgroundProperty = DependencyProperty.Register(nameof(PopupBackground), typeof(Brush), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupForegroundProperty = DependencyProperty.Register(nameof(PopupForeground), typeof(Brush), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupOverlayBackgroundProperty = DependencyProperty.Register(nameof(PopupOverlayBackground), typeof(Brush), typeof(BaseView), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty PopupOverlayWidthProperty = DependencyProperty.Register(nameof(PopupOverlayWidth), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty PopupOverlayHeightProperty = DependencyProperty.Register(nameof(PopupOverlayHeight), typeof(double), typeof(BaseView), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty ResizableMainColumnWidthProperty = DependencyProperty.Register(
            nameof(ResizableMainColumnWidth),
            typeof(GridLength),
            typeof(BaseView),
            new FrameworkPropertyMetadata(new GridLength(280d)));
        public static readonly DependencyProperty AlarmItemsSourceProperty = DependencyProperty.Register(
            nameof(AlarmItemsSource),
            typeof(IEnumerable),
            typeof(BaseView));
        public static readonly DependencyProperty AlarmListClearCommandProperty = DependencyProperty.Register(
            nameof(AlarmListClearCommand),
            typeof(ICommand),
            typeof(BaseView));
        public static readonly DependencyProperty PopupServiceProperty = DependencyProperty.Register(
            nameof(PopupService),
            typeof(IPopupService),
            typeof(BaseView),
            new FrameworkPropertyMetadata(null, OnPopupServiceChanged));


        public ICommand EmergencyCommand
        {
            get => (ICommand)GetValue(EmergencyCommandProperty);
            set => SetValue(EmergencyCommandProperty, value);
        }

        public object EmergencyCommandParameter
        {
            get => (object)GetValue(EmergencyCommandParameterProperty);
            set => SetValue(EmergencyCommandParameterProperty, value);
        }

        public object? EmergencyResetCommandParameter
        {
            get => GetValue(EmergencyResetCommandParameterProperty);
            set => SetValue(EmergencyResetCommandParameterProperty, value);
        }

        public UiState Status
        {
            get => (UiState)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public object? MainButtonContent
        {
            get => GetValue(MainButtonContentProperty);
            set => SetValue(MainButtonContentProperty, value);
        }

        public object? SubViewContent
        {
            get => GetValue(SubViewContentProperty);
            set => SetValue(SubViewContentProperty, value);
        }

        public object? PopupContent
        {
            get => GetValue(PopupContentProperty);
            set => SetValue(PopupContentProperty, value);
        }

        public Visibility PopupVisibility
        {
            get => (Visibility)GetValue(PopupVisibilityProperty);
            set => SetValue(PopupVisibilityProperty, value);
        }

        public AlarmType AlarmType
        {
            get => (AlarmType)GetValue(AlarmTypeProperty);
            set => SetValue(AlarmTypeProperty, value);
        }

        public bool IsSingleButton
        {
            get => (bool)GetValue(IsSingleButtonProperty);
            set => SetValue(IsSingleButtonProperty, value);
        }

        public ICommand? PopupCommand
        {
            get => (ICommand?)GetValue(PopupCommandProperty);
            set => SetValue(PopupCommandProperty, value);
        }

        /// <summary>
        /// KO: 팝업 응답을 외부 명령에 전달한 뒤 팝업을 닫는 내부 연결 명령입니다.<br/>
        /// EN: Gets the bridge command that forwards a popup response and then closes the popup.
        /// </summary>
        public ICommand PopupResponseCommand => DefaultPopupResponseCommand;

        public object? PopupLeftCommandParameter
        {
            get => GetValue(PopupLeftCommandParameterProperty);
            set => SetValue(PopupLeftCommandParameterProperty, value);
        }

        public object? PopupRightCommandParameter
        {
            get => GetValue(PopupRightCommandParameterProperty);
            set => SetValue(PopupRightCommandParameterProperty, value);
        }

        public double PopupWidth
        {
            get => (double)GetValue(PopupWidthProperty);
            set => SetValue(PopupWidthProperty, value);
        }

        public double PopupHeight
        {
            get => (double)GetValue(PopupHeightProperty);
            set => SetValue(PopupHeightProperty, value);
        }

        public double PopupButtonWidth
        {
            get => (double)GetValue(PopupButtonWidthProperty);
            set => SetValue(PopupButtonWidthProperty, value);
        }

        public double PopupButtonHeight
        {
            get => (double)GetValue(PopupButtonHeightProperty);
            set => SetValue(PopupButtonHeightProperty, value);
        }

        public double PopupTitleFontSize
        {
            get => (double)GetValue(PopupTitleFontSizeProperty);
            set => SetValue(PopupTitleFontSizeProperty, value);
        }

        public double PopupContentFontSize
        {
            get => (double)GetValue(PopupContentFontSizeProperty);
            set => SetValue(PopupContentFontSizeProperty, value);
        }

        public double PopupButtonSpacing
        {
            get => (double)GetValue(PopupButtonSpacingProperty);
            set => SetValue(PopupButtonSpacingProperty, value);
        }

        public double PopupAccentHeight
        {
            get => (double)GetValue(PopupAccentHeightProperty);
            set => SetValue(PopupAccentHeightProperty, value);
        }

        public Brush? PopupBackground
        {
            get => (Brush?)GetValue(PopupBackgroundProperty);
            set => SetValue(PopupBackgroundProperty, value);
        }

        public Brush? PopupForeground
        {
            get => (Brush?)GetValue(PopupForegroundProperty);
            set => SetValue(PopupForegroundProperty, value);
        }

        public Brush? PopupOverlayBackground
        {
            get => (Brush?)GetValue(PopupOverlayBackgroundProperty);
            set => SetValue(PopupOverlayBackgroundProperty, value);
        }

        public double PopupOverlayWidth
        {
            get => (double)GetValue(PopupOverlayWidthProperty);
            set => SetValue(PopupOverlayWidthProperty, value);
        }

        public double PopupOverlayHeight
        {
            get => (double)GetValue(PopupOverlayHeightProperty);
            set => SetValue(PopupOverlayHeightProperty, value);
        }

        /// <summary>
        /// KO: 왼쪽 메인 열의 너비를 가져오거나 설정합니다. 값 변경은 즉시 레이아웃에 반영됩니다.
        /// EN: Gets or sets the width of the left main column. Changes are applied to the layout immediately.
        /// </summary>
        [LocalizedDescription("왼쪽 메인 열의 너비를 가져오거나 설정합니다.", "Gets or sets the width of the left main column.")]
        public GridLength ResizableMainColumnWidth
        {
            get => (GridLength)GetValue(ResizableMainColumnWidthProperty);
            set => SetValue(ResizableMainColumnWidthProperty, value);
        }

        /// <summary>
        /// KO: 상단 알람 목록 팝업에 표시할 항목을 가져오거나 설정합니다.<br/>
        /// EN: Gets or sets the items displayed in the top alarm-list popup.
        /// </summary>
        [LocalizedDescription("상단 알람 목록 팝업에 표시할 항목을 가져오거나 설정합니다.", "Gets or sets the items displayed in the top alarm-list popup.")]
        public IEnumerable? AlarmItemsSource
        {
            get => (IEnumerable?)GetValue(AlarmItemsSourceProperty);
            set => SetValue(AlarmItemsSourceProperty, value);
        }

        /// <summary>
        /// KO: 알람 목록의 전체 정리 버튼에서 실행할 명령을 가져오거나 설정합니다.<br/>
        /// EN: Gets or sets the command executed by the alarm-list clear button.
        /// </summary>
        [LocalizedDescription("알람 목록의 전체 정리 버튼에서 실행할 명령을 가져오거나 설정합니다.", "Gets or sets the command executed by the alarm-list clear button.")]
        public ICommand? AlarmListClearCommand
        {
            get => (ICommand?)GetValue(AlarmListClearCommandProperty);
            set => SetValue(AlarmListClearCommandProperty, value);
        }

        /// <summary>
        /// KO: BaseView에 표시할 공용 팝업 서비스를 가져오거나 설정합니다.
        /// EN: Gets or sets the shared popup service displayed by BaseView.
        /// </summary>
        [LocalizedDescription("BaseView에 표시할 공용 팝업 서비스를 가져오거나 설정합니다.", "Gets or sets the shared popup service displayed by BaseView.")]
        public IPopupService? PopupService
        {
            get => (IPopupService?)GetValue(PopupServiceProperty);
            set => SetValue(PopupServiceProperty, value);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if(PopupService is null && Application.Current is BaseApplication application && application.Services is not null)
            {
                SetCurrentValue(PopupServiceProperty, application.Services.GetService(typeof(IPopupService)) as IPopupService);
            }
        }

        private void ClearAlarmList(object sender, ExecutedRoutedEventArgs e)
        {
            if(AlarmItemsSource is IList items)
                items.Clear();
        }

        private void HandlePopupResponse(object sender, ExecutedRoutedEventArgs e)
        {
            if(PopupCommand?.CanExecute(e.Parameter) == true)
                PopupCommand.Execute(e.Parameter);

            SetCurrentValue(PopupVisibilityProperty, Visibility.Collapsed);
            GetBindingExpression(PopupVisibilityProperty)?.UpdateSource();
        }

        private static void OnPopupServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (BaseView)d;

            if(e.OldValue is IPopupService oldService)
            {
                oldService.PropertyChanged -= view.OnPopupServicePropertyChanged;
            }

            if(e.NewValue is IPopupService newService)
            {
                newService.PropertyChanged += view.OnPopupServicePropertyChanged;
                view.ApplyPopupService(newService);
            }
        }

        private void OnPopupServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is IPopupService service)
            {
                ApplyPopupService(service);
            }
        }

        private void ApplyPopupService(IPopupService service)
        {
            SetCurrentValue(PopupVisibilityProperty, service.Visibility);
            SetCurrentValue(AlarmTypeProperty, service.AlarmType);
            SetCurrentValue(IsSingleButtonProperty, service.IsSingleButton);
            SetCurrentValue(PopupContentProperty, service.Content);
            SetCurrentValue(PopupCommandProperty, service.ResponseCommand);
            SetCurrentValue(PopupLeftCommandParameterProperty, PopupResult.Confirmed);
            SetCurrentValue(PopupRightCommandParameterProperty, PopupResult.Cancelled);
        }

        #region EventFunctions
        private Window? HostWindow => Window.GetWindow(this);
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            if(HostWindow is not Window window || e.ChangedButton != MouseButton.Left)
            {
                return;
            }
            if(e.ClickCount >= 2)
            {
                window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
                e.Handled = true;
                return;
            }
            if(e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            if(window.WindowState == WindowState.Maximized)
            {
                RestoreFromMaximized(window, e);
            }
            else
            {
                window.DragMove();
            }
        }
        private void RestoreFromMaximized(Window window, MouseButtonEventArgs e)
        {
            if(window.ActualWidth <= 0)
            {
                return;
            }
            var mousePos = e.GetPosition(window);
            var windowPos = window.PointToScreen(mousePos);
            var dpi = VisualTreeHelper.GetDpi(window);

            var windowDip = new Point(windowPos.X / dpi.DpiScaleX, windowPos.Y / dpi.DpiScaleY);
            var widthRatio = Math.Clamp(mousePos.X / window.ActualWidth, 0, 1);

            window.WindowState = WindowState.Normal;
            window.Left = windowDip.X - (window.RestoreBounds.Width * widthRatio);
            window.Top = windowDip.Y - mousePos.Y;
            window.DragMove();
        }
        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            if(HostWindow is Window window)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if(HostWindow is Window window)
            {
                HostWindow.Close();
            }
        }
        #endregion
    }
}

