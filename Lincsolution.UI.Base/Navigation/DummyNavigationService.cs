using CommunityToolkit.Mvvm.ComponentModel;

namespace Lincsolution.UI.Base.Navigation;

/// <summary>
/// 실제 화면 전환 없이 디자인뷰나 단위 테스트에서 Navigation 의존성을 대체하는 간이 구현이다.
/// </summary>
public partial class DummyNavigationService : ObservableObject, INavigationService
{
    /// <summary>팩터리 호출 결과를 확인할 수 있도록 마지막 ViewModel을 보관한다.</summary>
    [ObservableProperty]
    private object? _currentViewModel;

    /// <summary>
    /// DI 조회 없이 호출만 허용한다. 디자인뷰에서 실제 화면 전환은 수행하지 않는다.
    /// </summary>
    /// <typeparam name="TViewModel">요청된 ViewModel 형식이다.</typeparam>
    /// <param name="parameter">사용하지 않는 선택적 전달 데이터다.</param>
    /// <param name="forceRefresh">사용하지 않는 강제 새로 고침 값이다.</param>
    public void NavigateTo<TViewModel>(object? parameter = null, bool forceRefresh = false)
        where TViewModel : class
    {
    }

    /// <summary>
    /// 전달된 팩터리를 실행하고 반환된 ViewModel을 CurrentViewModel에 저장한다.
    /// </summary>
    /// <typeparam name="TViewModel">생성할 ViewModel 형식이다.</typeparam>
    /// <param name="factory">ViewModel을 반환하는 함수다.</param>
    /// <param name="parameter">사용하지 않는 선택적 전달 데이터다.</param>
    /// <param name="forceRefresh">사용하지 않는 강제 새로 고침 값이다.</param>
    public void NavigateTo<TViewModel>(
        Func<TViewModel> factory,
        object? parameter = null,
        bool forceRefresh = false)
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        CurrentViewModel = factory();
    }

    /// <summary>
    /// 디자인뷰용 구현이므로 이전 화면 복원을 수행하지 않는다.
    /// </summary>
    /// <param name="parameter">사용하지 않는 선택적 전달 데이터다.</param>
    public void GoBack(object? parameter = null)
    {
    }

    /// <summary>
    /// 디자인뷰용 구현은 이력을 보관하지 않으므로 별도 작업을 수행하지 않는다.
    /// </summary>
    public void ClearHistory()
    {
    }
}
