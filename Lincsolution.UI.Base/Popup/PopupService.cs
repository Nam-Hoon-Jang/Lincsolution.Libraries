using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Base.Popup
{
    /// <summary>
    /// KO: ViewModel에서 공용 팝업을 표시하는 서비스 계약입니다.
    /// EN: Defines the service used by ViewModels to display the shared popup.
    /// </summary>
    public interface IPopupService : INotifyPropertyChanged
    {
        Visibility Visibility { get; }
        AlarmType AlarmType { get; }
        bool IsSingleButton { get; }
        object? Content { get; }
        ICommand ResponseCommand { get; }

        /// <summary>
        /// KO: 팝업을 표시하고 사용자의 응답을 비동기로 반환합니다.
        /// EN: Displays a popup and asynchronously returns the user's response.
        /// </summary>
        Task<PopupResult> ShowAsync(
            object? content,
            AlarmType alarmType = AlarmType.INFO,
            bool isSingleButton = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// KO: 현재 팝업을 지정한 결과로 닫습니다.
        /// EN: Closes the current popup with the specified result.
        /// </summary>
        void Close(PopupResult result = PopupResult.Cancelled);
    }

    /// <summary>
    /// KO: 팝업에서 선택된 결과입니다.
    /// EN: Defines the result selected in a popup.
    /// </summary>
    public enum PopupResult
    {
        Confirmed,
        Cancelled
    }

    /// <summary>
    /// KO: 애플리케이션 전체에서 하나의 BaseView 팝업을 공유하는 서비스입니다.
    /// EN: Shares one BaseView popup across the application.
    /// </summary>
    public sealed partial class PopupService : ObservableObject, IPopupService
    {
        private readonly SemaphoreSlim _queue = new(1, 1);
        private TaskCompletionSource<PopupResult>? _completion;

        [ObservableProperty]
        private Visibility _visibility = Visibility.Collapsed;

        [ObservableProperty]
        private AlarmType _alarmType = AlarmType.INFO;

        [ObservableProperty]
        private bool _isSingleButton;

        [ObservableProperty]
        private object? _content;

        public ICommand ResponseCommand { get; }

        public PopupService()
        {
            ResponseCommand = new RelayCommand<PopupResult>(Close);
        }

        public async Task<PopupResult> ShowAsync(
            object? content,
            AlarmType alarmType = AlarmType.INFO,
            bool isSingleButton = false,
            CancellationToken cancellationToken = default)
        {
            await _queue.WaitAsync(cancellationToken);

            try
            {
                var completion = new TaskCompletionSource<PopupResult>(TaskCreationOptions.RunContinuationsAsynchronously);
                _completion = completion;

                await RunOnUiThreadAsync(() =>
                {
                    Content = content;
                    AlarmType = alarmType;
                    IsSingleButton = isSingleButton;
                    Visibility = Visibility.Visible;
                });

                using var registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
                return await completion.Task;
            }
            finally
            {
                await RunOnUiThreadAsync(() =>
                {
                    Visibility = Visibility.Collapsed;
                    Content = null;
                });
                _completion = null;
                _queue.Release();
            }
        }

        public void Close(PopupResult result = PopupResult.Cancelled)
        {
            _completion?.TrySetResult(result);
        }

        private static Task RunOnUiThreadAsync(Action action)
        {
            var application = Application.Current
                ?? throw new InvalidOperationException("A WPF Application must exist before displaying a popup.");

            if(application.Dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return application.Dispatcher.InvokeAsync(action).Task;
        }
    }
}
