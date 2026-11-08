using System.Collections.ObjectModel;
using System.ComponentModel;
using Lincsolution.Core.Base.Alarms;
using Lincsolution.UI.Base.Common;

namespace Lincsolution.UI.Base.Shared;

/// <summary>
/// 여러 View와 ViewModel이 공유하는 최소 UI 상태, Alarm 목록, 시스템 시각을 정의한다.
/// 실행 애플리케이션은 이 인터페이스를 상속해 제품별 ISharedModel 계약을 만든다.
/// </summary>
/// <typeparam name="TAlarm">Core.Base의 AlarmBase를 상속하는 제품별 Alarm 형식이다.</typeparam>
public interface IBaseSharedModel<TAlarm> : INotifyPropertyChanged
    where TAlarm : AlarmBase
{
    /// <summary>여러 View가 공통으로 표시할 현재 UI 상태를 가져오거나 설정한다.</summary>
    UiState UiState { get; set; }

    /// <summary>UI에서 표시하고 관리할 Alarm 목록을 가져온다.</summary>
    ObservableCollection<TAlarm> AlarmList { get; }

    /// <summary>UI에서 표시할 현재 시스템 시각을 가져온다.</summary>
    DateTimeOffset SystemTime { get; }

    /// <summary>공통 Popup이 열려 있는지 가져오거나 설정한다.</summary>
    bool IsPopupOpen { get; set; }

    /// <summary>
    /// Alarm을 UI Thread에서 공용 목록에 추가한다.
    /// </summary>
    /// <param name="alarm">추가할 제품별 Alarm 인스턴스다.</param>
    void AddAlarm(TAlarm alarm);

    /// <summary>
    /// 지정한 Alarm을 공용 목록에서 제거한다.
    /// </summary>
    /// <param name="alarm">제거할 제품별 Alarm 인스턴스다.</param>
    /// <returns>목록에서 Alarm을 제거했으면 true, 존재하지 않으면 false다.</returns>
    bool RemoveAlarm(TAlarm alarm);

    /// <summary>
    /// 해제 상태인 Alarm을 공용 목록에서 모두 제거한다.
    /// </summary>
    /// <returns>목록에서 제거한 Alarm 수다.</returns>
    int ClearResolvedAlarms();
}
