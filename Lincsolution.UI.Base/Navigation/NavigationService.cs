using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Lincsolution.UI.Base.ViewModels;

namespace Lincsolution.UI.Base.Navigation;

/// <summary>
/// DI 기반 ViewModel 생성, 현재 ViewModel 변경, 이전 화면 이력과 Navigation 수명주기 알림을 관리한다.
/// </summary>
public partial class NavigationService : ObservableObject, INavigationService
{
    /// <summary>형식으로 요청된 ViewModel을 조회하는 DI 서비스 공급자다.</summary>
    private readonly IServiceProvider _provider;

    /// <summary>뒤로 가기 시 복원할 이전 ViewModel 인스턴스를 순서대로 보관한다.</summary>
    private readonly Stack<object> _history = new();

    /// <summary>현재 화면에 표시할 ViewModel이다. Source Generator가 CurrentViewModel 속성을 생성한다.</summary>
    [ObservableProperty]
    private object? _currentViewModel;

    /// <summary>
    /// ViewModel을 조회할 DI 서비스 공급자를 받아 Navigation 서비스를 초기화한다.
    /// </summary>
    /// <param name="provider">ViewModel 등록 정보를 가진 DI 서비스 공급자다.</param>
    public NavigationService(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>
    /// DI 컨테이너에서 지정한 ViewModel을 조회한 뒤 현재 화면으로 전환한다.
    /// </summary>
    /// <typeparam name="TViewModel">전환할 ViewModel 형식이다.</typeparam>
    /// <param name="parameter">새 ViewModel에 전달할 선택적 데이터다.</param>
    /// <param name="forceRefresh">현재 ViewModel과 같아도 수명주기 알림을 다시 실행할지 여부다.</param>
    public void NavigateTo<TViewModel>(object? parameter = null, bool forceRefresh = false)
        where TViewModel : class
    {
        NavigateCore(ViewModelLoader.Load<TViewModel>(_provider), parameter, forceRefresh);
    }

    /// <summary>
    /// 팩터리에서 ViewModel을 생성하거나 조회한 뒤 현재 화면으로 전환한다.
    /// ViewModelLoader와 함께 사용할 때 캐시된 ViewModel을 전달하는 용도로 사용할 수 있다.
    /// </summary>
    /// <typeparam name="TViewModel">전환할 ViewModel 형식이다.</typeparam>
    /// <param name="factory">전환 대상 ViewModel을 반환하는 함수다.</param>
    /// <param name="parameter">새 ViewModel에 전달할 선택적 데이터다.</param>
    /// <param name="forceRefresh">현재 ViewModel과 같아도 수명주기 알림을 다시 실행할지 여부다.</param>
    public void NavigateTo<TViewModel>(
        Func<TViewModel> factory,
        object? parameter = null,
        bool forceRefresh = false)
        where TViewModel : class
    {
        ArgumentNullException.ThrowIfNull(factory);
        NavigateCore(factory(), parameter, forceRefresh);
    }

    /// <summary>
    /// 현재 ViewModel을 이탈시키고 이력의 가장 최근 ViewModel을 복원한다.
    /// </summary>
    /// <param name="parameter">복원되는 ViewModel에 전달할 선택적 데이터다.</param>
    public void GoBack(object? parameter = null)
    {
        if (_history.Count == 0)
        {
            return;
        }

        // 현재 화면에 이탈을 알린 후 이전 화면을 복원하고 진입을 알린다.
        NotifyNavigatedFrom(CurrentViewModel);
        CurrentViewModel = _history.Pop();
        NotifyNavigatedTo(CurrentViewModel, parameter);
    }

    /// <summary>
    /// 현재 ViewModel은 유지하고 저장된 뒤로 가기 이력만 제거한다.
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
    }

    /// <summary>
    /// 중복 전환을 검사하고 기존 ViewModel을 이력에 저장한 뒤 새 ViewModel로 교체한다.
    /// </summary>
    /// <param name="viewModel">새로 표시할 ViewModel 인스턴스다.</param>
    /// <param name="parameter">새 ViewModel에 전달할 선택적 데이터다.</param>
    /// <param name="forceRefresh">같은 인스턴스로의 전환도 강제로 실행할지 여부다.</param>
    private void NavigateCore(object viewModel, object? parameter, bool forceRefresh)
    {
        // 강제 새로 고침이 아니면 동일 인스턴스로의 중복 전환을 무시한다.
        if (!forceRefresh && ReferenceEquals(viewModel, CurrentViewModel))
        {
            return;
        }

        // 현재 화면에 이탈을 알리고 뒤로 가기 이력에 저장한다.
        if (CurrentViewModel is not null)
        {
            NotifyNavigatedFrom(CurrentViewModel);
            _history.Push(CurrentViewModel);
        }

        // 새 화면을 바인딩 대상으로 설정한 뒤 진입 수명주기를 호출한다.
        CurrentViewModel = viewModel;
        NotifyNavigatedTo(CurrentViewModel, parameter);
    }

    /// <summary>
    /// ViewModel이 Navigation 알림을 지원하면 화면 진입 콜백을 호출한다.
    /// </summary>
    /// <param name="viewModel">진입할 ViewModel 인스턴스다.</param>
    /// <param name="parameter">진입 콜백에 전달할 선택적 데이터다.</param>
    private static void NotifyNavigatedTo(object? viewModel, object? parameter)
    {
        if (viewModel is INavigationNotice notice)
        {
            notice.OnNavigatedTo(parameter);
        }
    }

    /// <summary>
    /// ViewModel이 Navigation 알림을 지원하면 화면 이탈 콜백을 호출한다.
    /// </summary>
    /// <param name="viewModel">이탈할 ViewModel 인스턴스다.</param>
    private static void NotifyNavigatedFrom(object? viewModel)
    {
        if (viewModel is INavigationNotice notice)
        {
            notice.OnNavigatedFrom();
        }
    }
}
