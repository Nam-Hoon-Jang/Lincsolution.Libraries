using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Lincsolution.Core.Base.Alarms;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Base.Shared;

/// <summary>
/// 전체 View와 ViewModel에서 공유하는 UI 상태, Alarm 목록, Popup 상태와 시스템 시각 갱신을 구현한다.
/// 실행 애플리케이션은 이 클래스를 상속해 제품별 SharedModel을 만든다.
/// </summary>
/// <typeparam name="TAlarm">Core.Base의 AlarmBase를 상속하는 제품별 Alarm 형식이다.</typeparam>
public partial class BaseSharedModel<TAlarm> : ObservableObject, IBaseSharedModel<TAlarm>, IDisposable
    where TAlarm : AlarmBase
{
    /// <summary>시스템 시각 PropertyChanged를 주기적으로 발생시키는 WPF DispatcherTimer다.</summary>
    private readonly DispatcherTimer _systemTimer;

    /// <summary>중복 Dispose를 방지하기 위한 상태다.</summary>
    private bool _disposed;

    /// <summary>여러 View가 공통으로 표시할 현재 UI 상태다.</summary>
    [ObservableProperty]
    private UiState _uiState = UiState.IDLE;

    /// <summary>공통 Popup이 열려 있는지 나타낸다.</summary>
    [ObservableProperty]
    private bool _isPopupOpen;

    /// <summary>
    /// 시스템 시각 갱신 간격을 받아 SharedModel 기반을 초기화하고 Timer를 시작한다.
    /// </summary>
    /// <param name="timerIntervalMilliseconds">SystemTime 변경 알림 간격이다. 1ms 이상이어야 한다.</param>
    /// <exception cref="ArgumentOutOfRangeException">갱신 간격이 1ms보다 작을 때 발생한다.</exception>
    protected BaseSharedModel(int timerIntervalMilliseconds = 500)
    {
        if (timerIntervalMilliseconds < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timerIntervalMilliseconds),
                "시스템 시각 갱신 간격은 1ms 이상이어야 합니다.");
        }

        AlarmList = new ObservableCollection<TAlarm>();
        _systemTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(timerIntervalMilliseconds),
            DispatcherPriority.Background,
            OnSystemTimerTick,
            Dispatcher.CurrentDispatcher);
        _systemTimer.Start();
    }

    /// <summary>UI에서 표시하고 관리할 Alarm 목록을 가져온다.</summary>
    public ObservableCollection<TAlarm> AlarmList { get; }

    /// <summary>
    /// UI에서 표시할 현재 시스템 시각을 가져온다.
    /// Timer Tick마다 이 속성의 PropertyChanged 이벤트가 발생한다.
    /// </summary>
    public DateTimeOffset SystemTime => DateTimeOffset.Now;

    /// <summary>
    /// Alarm을 UI Dispatcher에서 공용 목록에 추가한다.
    /// 다른 Thread에서 호출해도 ObservableCollection의 Thread 예외가 발생하지 않도록 처리한다.
    /// </summary>
    /// <param name="alarm">추가할 제품별 Alarm 인스턴스다.</param>
    /// <exception cref="ObjectDisposedException">SharedModel이 이미 Dispose됐을 때 발생한다.</exception>
    public void AddAlarm(TAlarm alarm)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(alarm);

        InvokeOnDispatcher(() => AlarmList.Add(alarm));
    }

    /// <summary>
    /// 지정한 Alarm을 UI Dispatcher에서 공용 목록에서 제거한다.
    /// </summary>
    /// <param name="alarm">제거할 제품별 Alarm 인스턴스다.</param>
    /// <returns>목록에서 Alarm을 제거했으면 true, 존재하지 않으면 false다.</returns>
    /// <exception cref="ObjectDisposedException">SharedModel이 이미 Dispose됐을 때 발생한다.</exception>
    public bool RemoveAlarm(TAlarm alarm)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(alarm);

        return InvokeOnDispatcher(() => AlarmList.Remove(alarm));
    }

    /// <summary>
    /// IsResolved가 true인 Alarm을 UI Dispatcher에서 모두 제거한다.
    /// </summary>
    /// <returns>목록에서 제거한 Alarm 수다.</returns>
    /// <exception cref="ObjectDisposedException">SharedModel이 이미 Dispose됐을 때 발생한다.</exception>
    public int ClearResolvedAlarms()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        return InvokeOnDispatcher(() =>
        {
            var resolvedAlarms = AlarmList.Where(alarm => alarm.IsResolved).ToArray();

            foreach (var alarm in resolvedAlarms)
            {
                AlarmList.Remove(alarm);
            }

            return resolvedAlarms.Length;
        });
    }

    /// <summary>
    /// 시스템 시각 Timer를 중지하고 이벤트 연결을 해제한다.
    /// 여러 번 호출해도 정리 작업은 한 번만 실행된다.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _systemTimer.Stop();
        _systemTimer.Tick -= OnSystemTimerTick;
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Timer Tick마다 SystemTime 속성이 변경됐음을 View에 알린다.
    /// </summary>
    /// <param name="sender">Tick을 발생시킨 DispatcherTimer다.</param>
    /// <param name="e">Timer Tick 이벤트 데이터다.</param>
    private void OnSystemTimerTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(SystemTime));
    }

    /// <summary>
    /// 현재 Thread가 UI Dispatcher Thread면 즉시 실행하고, 아니면 Dispatcher에 동기 호출한다.
    /// </summary>
    /// <param name="action">UI Dispatcher에서 실행할 작업이다.</param>
    private static void InvokeOnDispatcher(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        if (dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }

    /// <summary>
    /// 현재 Thread가 UI Dispatcher Thread면 함수를 즉시 실행하고, 아니면 Dispatcher에 동기 호출한다.
    /// </summary>
    /// <typeparam name="TResult">호출 결과 형식이다.</typeparam>
    /// <param name="function">UI Dispatcher에서 실행하고 결과를 반환할 함수다.</param>
    /// <returns>Dispatcher에서 실행된 함수의 결과다.</returns>
    private static TResult InvokeOnDispatcher<TResult>(Func<TResult> function)
    {
        var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

        return dispatcher.CheckAccess()
            ? function()
            : dispatcher.Invoke(function);
    }
}
