using System.ComponentModel;

namespace Lincsolution.UI.Base.Navigation;

/// <summary>
/// 현재 ViewModel, 화면 전환, 뒤로 가기 이력을 관리하는 Navigation 계약이다.
/// </summary>
public interface INavigationService : INotifyPropertyChanged
{
    /// <summary>현재 화면에 표시할 ViewModel을 가져온다.</summary>
    object? CurrentViewModel { get; }

    /// <summary>
    /// DI 컨테이너에서 ViewModel을 가져와 현재 화면으로 전환한다.
    /// </summary>
    /// <typeparam name="TViewModel">전환할 ViewModel 형식이다.</typeparam>
    /// <param name="parameter">새 ViewModel에 전달할 선택적 데이터다.</param>
    /// <param name="forceRefresh">현재 ViewModel과 같아도 수명주기 알림을 다시 실행할지 여부다.</param>
    void NavigateTo<TViewModel>(object? parameter = null, bool forceRefresh = false)
        where TViewModel : class;

    /// <summary>
    /// 호출자가 제공한 팩터리로 ViewModel을 만든 뒤 현재 화면으로 전환한다.
    /// </summary>
    /// <typeparam name="TViewModel">전환할 ViewModel 형식이다.</typeparam>
    /// <param name="factory">전환 대상 ViewModel을 반환하는 함수다.</param>
    /// <param name="parameter">새 ViewModel에 전달할 선택적 데이터다.</param>
    /// <param name="forceRefresh">현재 ViewModel과 같아도 수명주기 알림을 다시 실행할지 여부다.</param>
    void NavigateTo<TViewModel>(
        Func<TViewModel> factory,
        object? parameter = null,
        bool forceRefresh = false)
        where TViewModel : class;

    /// <summary>
    /// 이력에 저장된 이전 ViewModel로 돌아간다.
    /// </summary>
    /// <param name="parameter">복원되는 ViewModel에 전달할 선택적 데이터다.</param>
    void GoBack(object? parameter = null);

    /// <summary>
    /// 현재 ViewModel은 유지하고 뒤로 가기 이력만 제거한다.
    /// </summary>
    void ClearHistory();
}
