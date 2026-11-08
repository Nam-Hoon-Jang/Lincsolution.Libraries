using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Lincsolution.Core.Base.Alarms;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Base.Shared;

/// <summary>
/// WPF 디자인뷰와 단위 테스트에서 실제 Timer나 Dispatcher 작업 없이 사용할 SharedModel 기반 구현이다.
/// 실행 애플리케이션은 필요하면 이 클래스를 상속해 디자인용 샘플 데이터를 추가한다.
/// </summary>
/// <typeparam name="TAlarm">Core.Base의 AlarmBase를 상속하는 제품별 Alarm 형식이다.</typeparam>
public partial class BaseDummySharedModel<TAlarm> : ObservableObject, IBaseSharedModel<TAlarm>
    where TAlarm : AlarmBase
{
    /// <summary>여러 View가 공통으로 표시할 디자인용 UI 상태다.</summary>
    [ObservableProperty]
    private UiState _uiState = UiState.IDLE;

    /// <summary>디자인뷰에서 공통 Popup이 열려 있는지 나타낸다.</summary>
    [ObservableProperty]
    private bool _isPopupOpen;

    /// <summary>
    /// 비어 있는 Alarm 목록과 현재 시각으로 디자인용 SharedModel을 초기화한다.
    /// </summary>
    public BaseDummySharedModel()
    {
        AlarmList = new ObservableCollection<TAlarm>();
        SystemTime = DateTimeOffset.Now;
    }

    /// <summary>디자인뷰에서 표시할 Alarm 목록을 가져온다.</summary>
    public ObservableCollection<TAlarm> AlarmList { get; }

    /// <summary>디자인뷰에 고정해서 표시할 시스템 시각을 가져오거나 설정한다.</summary>
    public DateTimeOffset SystemTime { get; set; }

    /// <summary>
    /// Alarm을 디자인용 목록에 추가한다.
    /// </summary>
    /// <param name="alarm">추가할 제품별 Alarm 인스턴스다.</param>
    public void AddAlarm(TAlarm alarm)
    {
        ArgumentNullException.ThrowIfNull(alarm);
        AlarmList.Add(alarm);
    }

    /// <summary>
    /// 지정한 Alarm을 디자인용 목록에서 제거한다.
    /// </summary>
    /// <param name="alarm">제거할 제품별 Alarm 인스턴스다.</param>
    /// <returns>목록에서 Alarm을 제거했으면 true, 존재하지 않으면 false다.</returns>
    public bool RemoveAlarm(TAlarm alarm)
    {
        ArgumentNullException.ThrowIfNull(alarm);
        return AlarmList.Remove(alarm);
    }

    /// <summary>
    /// IsResolved가 true인 Alarm을 디자인용 목록에서 모두 제거한다.
    /// </summary>
    /// <returns>목록에서 제거한 Alarm 수다.</returns>
    public int ClearResolvedAlarms()
    {
        var resolvedAlarms = AlarmList.Where(alarm => alarm.IsResolved).ToArray();

        foreach (var alarm in resolvedAlarms)
        {
            AlarmList.Remove(alarm);
        }

        return resolvedAlarms.Length;
    }
}
