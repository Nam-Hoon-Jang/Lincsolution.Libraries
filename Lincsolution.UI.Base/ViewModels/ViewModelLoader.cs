using System.Collections.Concurrent;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace Lincsolution.UI.Base.ViewModels;

/// <summary>
/// ViewModel을 DI 컨테이너에서 생성하고 캐시하며, 실행 환경에 따라 일반 ViewModel을 TestViewModel로 치환한다.
/// TestViewModel 매핑은 라이브러리에 하드코딩하지 않고 실행 애플리케이션의 시작 지점에서 등록한다.
/// </summary>
/// <remarks>
/// App.xaml.cs 시작 과정에서 다음 순서로 설정한다.
/// <code>
/// ViewModelLoader.RegisterTestViewModel&lt;MainViewModel, TestMainViewModel&gt;();
/// ViewModelLoader.RegisterTestViewModel&lt;DashboardViewModel, TestDashboardViewModel&gt;();
/// ViewModelLoader.SetTestMode(enabled: true);
///
/// var mainViewModel = ViewModelLoader.Load&lt;MainViewModel&gt;(Services);
/// </code>
/// 일반 실행에서는 매핑을 그대로 등록해도 SetTestMode(false)이면 실제 ViewModel이 로드된다.
/// WPF 디자인 모드에서는 등록된 TestViewModel 매핑을 자동으로 사용한다.
/// </remarks>
public static class ViewModelLoader
{
    /// <summary>실제 ViewModel 형식을 키로 사용하여 생성된 인스턴스를 보관하는 캐시다.</summary>
    private static readonly ConcurrentDictionary<Type, object> CacheStore = new();

    /// <summary>실제 ViewModel 형식과 이를 대체할 TestViewModel 형식의 매핑을 보관한다.</summary>
    private static readonly ConcurrentDictionary<Type, Type> TestTypeMap = new();

    /// <summary>런타임에서도 TestViewModel을 사용할지 나타낸다. 디자인 모드에서는 이 값과 무관하게 TestViewModel을 우선한다.</summary>
    private static volatile bool _isTestModeEnabled;

    /// <summary>
    /// 현재 런타임 TestViewModel 사용 여부를 가져온다.
    /// </summary>
    public static bool IsTestModeEnabled => _isTestModeEnabled;

    /// <summary>
    /// 실제 ViewModel과 이를 대신할 TestViewModel 형식을 등록한다.
    /// </summary>
    /// <typeparam name="TViewModel">View와 바인딩되는 실제 ViewModel 형식이다.</typeparam>
    /// <typeparam name="TTestViewModel">실제 ViewModel을 상속하는 테스트용 ViewModel 형식이다.</typeparam>
    public static void RegisterTestViewModel<TViewModel, TTestViewModel>()
        where TViewModel : class
        where TTestViewModel : class, TViewModel
    {
        RegisterTestViewModel(typeof(TViewModel), typeof(TTestViewModel));
    }

    /// <summary>
    /// 실행 시 결정된 형식 정보를 사용하여 실제 ViewModel과 TestViewModel 매핑을 등록한다.
    /// </summary>
    /// <param name="viewModelType">View와 바인딩되는 실제 ViewModel 형식이다.</param>
    /// <param name="testViewModelType">실제 ViewModel을 상속하는 테스트용 ViewModel 형식이다.</param>
    /// <exception cref="ArgumentException">TestViewModel이 실제 ViewModel에 할당될 수 없을 때 발생한다.</exception>
    public static void RegisterTestViewModel(Type viewModelType, Type testViewModelType)
    {
        ArgumentNullException.ThrowIfNull(viewModelType);
        ArgumentNullException.ThrowIfNull(testViewModelType);

        // Load<T>의 반환 형식을 보장하려면 TestViewModel이 실제 ViewModel을 상속해야 한다.
        if (!viewModelType.IsAssignableFrom(testViewModelType))
        {
            throw new ArgumentException(
                $"{testViewModelType.FullName}은(는) {viewModelType.FullName}에 할당할 수 없습니다.",
                nameof(testViewModelType));
        }

        TestTypeMap[viewModelType] = testViewModelType;
    }

    /// <summary>
    /// 여러 ViewModel/TestViewModel 매핑을 한 번에 등록한다.
    /// </summary>
    /// <param name="mappings">실제 ViewModel 형식과 TestViewModel 형식의 쌍이다.</param>
    public static void RegisterTestViewModels(IEnumerable<KeyValuePair<Type, Type>> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        foreach (var mapping in mappings)
        {
            RegisterTestViewModel(mapping.Key, mapping.Value);
        }
    }

    /// <summary>
    /// 지정한 실제 ViewModel에 등록된 TestViewModel 매핑을 제거한다.
    /// </summary>
    /// <typeparam name="TViewModel">매핑을 제거할 실제 ViewModel 형식이다.</typeparam>
    /// <returns>기존 매핑을 제거했으면 true, 등록된 매핑이 없으면 false다.</returns>
    public static bool RemoveTestViewModel<TViewModel>()
        where TViewModel : class
    {
        return TestTypeMap.TryRemove(typeof(TViewModel), out _);
    }

    /// <summary>
    /// 등록된 모든 TestViewModel 매핑을 제거한다.
    /// </summary>
    public static void ClearTestViewModelMappings()
    {
        TestTypeMap.Clear();
    }

    /// <summary>
    /// 런타임 TestViewModel 사용 여부를 설정한다.
    /// </summary>
    /// <param name="enabled">true면 런타임에서도 매핑된 TestViewModel을 사용한다.</param>
    /// <param name="clearCache">
    /// true면 기존 실제/Test 인스턴스가 남지 않도록 ViewModel 캐시를 비운다.
    /// 애플리케이션 시작 시 최초 설정하는 경우에도 true 사용을 권장한다.
    /// </param>
    public static void SetTestMode(bool enabled, bool clearCache = true)
    {
        _isTestModeEnabled = enabled;

        // 같은 ViewModel 키 아래 실제 인스턴스와 테스트 인스턴스가 섞이지 않도록 모드 전환 시 캐시를 초기화한다.
        if (clearCache)
        {
            ClearAll();
        }
    }

    /// <summary>
    /// 지정한 형식의 ViewModel 인스턴스를 캐시에 직접 등록한다.
    /// </summary>
    /// <param name="type">캐시 조회에 사용할 실제 ViewModel 형식이다.</param>
    /// <param name="instance">캐시에 저장할 ViewModel 인스턴스다.</param>
    /// <exception cref="ArgumentException">인스턴스가 지정한 형식에 할당될 수 없을 때 발생한다.</exception>
    public static void Cache(Type type, object instance)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(instance);

        if (!type.IsInstanceOfType(instance))
        {
            throw new ArgumentException(
                $"{instance.GetType().FullName} is not assignable to {type.FullName}.",
                nameof(instance));
        }

        CacheStore[type] = instance;
    }

    /// <summary>
    /// 제네릭 형식으로 ViewModel 인스턴스를 캐시에 직접 등록한다.
    /// </summary>
    /// <typeparam name="T">캐시 조회에 사용할 실제 ViewModel 형식이다.</typeparam>
    /// <param name="instance">캐시에 저장할 ViewModel 인스턴스다.</param>
    public static void Cache<T>(T instance)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        CacheStore[typeof(T)] = instance;
    }

    /// <summary>
    /// 지정한 ViewModel 형식의 캐시를 제거한다.
    /// </summary>
    /// <typeparam name="T">캐시에서 제거할 실제 ViewModel 형식이다.</typeparam>
    /// <returns>인스턴스를 제거했으면 true, 캐시가 없으면 false다.</returns>
    public static bool Remove<T>()
        where T : class
    {
        return CacheStore.TryRemove(typeof(T), out _);
    }

    /// <summary>
    /// 생성되어 보관 중인 모든 ViewModel 인스턴스를 캐시에서 제거한다.
    /// TestViewModel 형식 매핑은 유지된다.
    /// </summary>
    public static void ClearAll()
    {
        CacheStore.Clear();
    }

    /// <summary>
    /// 실제 ViewModel 형식을 키로 캐시를 조회하고, 없으면 현재 모드에 맞는 구현 형식을 DI 컨테이너에서 생성한다.
    /// </summary>
    /// <typeparam name="T">View와 바인딩되는 실제 ViewModel 형식이다.</typeparam>
    /// <param name="provider">ViewModel과 생성자 의존성을 제공하는 DI 서비스 공급자다.</param>
    /// <returns>캐시된 실제 ViewModel 또는 매핑된 TestViewModel 인스턴스다.</returns>
    /// <exception cref="InvalidOperationException">ViewModel을 DI 컨테이너에서 생성할 수 없을 때 발생한다.</exception>
    public static T Load<T>(IServiceProvider provider)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(provider);

        try
        {
            // 캐시는 항상 실제 ViewModel 형식을 키로 사용한다.
            // 따라서 호출자는 테스트 모드 여부와 관계없이 Load<MainViewModel>() 형태를 유지할 수 있다.
            var instance = CacheStore.GetOrAdd(
                typeof(T),
                requestedType => CreateViewModel(provider, requestedType));

            return (T)instance;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"ViewModelLoader could not create {typeof(T).FullName}. " +
                "Register the ViewModel with the service provider or cache an instance first.",
                exception);
        }
    }

    /// <summary>
    /// 현재 프로세스가 WPF 디자인 도구 안에서 실행 중인지 확인한다.
    /// </summary>
    /// <returns>Visual Studio/Blend 디자인 모드면 true, 일반 런타임이면 false다.</returns>
    public static bool IsInDesignMode()
    {
        return DesignerProperties.GetIsInDesignMode(new DependencyObject());
    }

    /// <summary>
    /// 현재 실행 모드와 등록된 매핑을 기준으로 실제 생성할 ViewModel 형식을 선택한다.
    /// </summary>
    /// <param name="requestedType">호출자가 요청한 실제 ViewModel 형식이다.</param>
    /// <returns>실제로 생성할 실제 ViewModel 또는 TestViewModel 형식이다.</returns>
    private static Type ResolveImplementationType(Type requestedType)
    {
        // 디자인뷰는 테스트 데이터 표시가 목적이므로 TestViewModel 매핑을 자동으로 사용한다.
        // 일반 런타임에서는 App 시작 시 SetTestMode(true)를 호출한 경우에만 테스트 형식으로 치환한다.
        var useTestViewModel = IsInDesignMode() || IsTestModeEnabled;

        if (useTestViewModel &&
            TestTypeMap.TryGetValue(requestedType, out var testViewModelType))
        {
            return testViewModelType;
        }

        return requestedType;
    }

    /// <summary>
    /// 선택된 ViewModel 형식을 DI 컨테이너에서 가져오거나 ActivatorUtilities로 생성한다.
    /// </summary>
    /// <param name="provider">생성자 의존성을 제공하는 DI 서비스 공급자다.</param>
    /// <param name="requestedType">호출자가 요청한 실제 ViewModel 형식이다.</param>
    /// <returns>현재 실행 모드에 맞게 생성된 ViewModel 인스턴스다.</returns>
    private static object CreateViewModel(IServiceProvider provider, Type requestedType)
    {
        var implementationType = ResolveImplementationType(requestedType);

        // DI에 구현 형식이 직접 등록되어 있으면 해당 수명 주기 설정을 존중한다.
        // 등록되지 않은 경우에도 생성자 의존성을 DI에서 받아 만들 수 있도록 ActivatorUtilities를 사용한다.
        var instance = provider.GetService(implementationType) ??
                       ActivatorUtilities.CreateInstance(provider, implementationType);

        if (!requestedType.IsInstanceOfType(instance))
        {
            throw new InvalidOperationException(
                $"{implementationType.FullName} 인스턴스를 {requestedType.FullName}(으)로 사용할 수 없습니다.");
        }

        return instance;
    }
}
