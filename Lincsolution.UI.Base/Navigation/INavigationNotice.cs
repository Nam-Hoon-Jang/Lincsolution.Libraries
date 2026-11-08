namespace Lincsolution.UI.Base.Navigation;

/// <summary>
/// ViewModel이 화면 전환 진입과 이탈 시점을 통지받기 위한 계약이다.
/// </summary>
public interface INavigationNotice
{
    /// <summary>
    /// ViewModel이 현재 화면으로 전환된 직후 호출된다.
    /// </summary>
    /// <param name="parameter">이전 화면이나 호출자가 전달한 선택적 데이터다.</param>
    void OnNavigatedTo(object? parameter = null);

    /// <summary>
    /// ViewModel이 현재 화면에서 이탈하기 직전에 호출된다.
    /// </summary>
    void OnNavigatedFrom();
}
