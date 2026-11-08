using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lincsolution.UI.Base.Models;
using Lincsolution.UI.Base.Navigation;

namespace Lincsolution.UI.Base.ViewModels;

/// <summary>
/// 별도 Model이나 공용 데이터 없이 Command와 Navigation 수명주기만 필요한 ViewModel 기반 클래스다.
/// </summary>
/// <typeparam name="TCommand">View에서 전달하는 Command 종류를 정의한 Enum 형식이다.</typeparam>
public abstract class BaseViewModel<TCommand> : ObservableValidator, INavigationNotice
    where TCommand : struct, Enum
{
    /// <summary>
    /// RelayCommand를 구성하고 파생 ViewModel의 초기화 과정을 실행한다.
    /// </summary>
    /// <summary>Use this constructor when child View navigation is not required.</summary>
    protected BaseViewModel()
    {
        // View의 CommandParameter로 전달된 Enum을 파생 클래스의 Execute 구현에 연결한다.
        Command = new RelayCommand<TCommand>(Execute);
        Initialise();
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    protected BaseViewModel(INavigationService navigation)
        : this()
    {
        AttachNavigation(navigation);
    }

    private void AttachNavigation(INavigationService navigation)
    {
        Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        Navigation.PropertyChanged += OnNavigationPropertyChanged;
    }

    private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    /// <summary>View에서 바인딩해 사용자 동작을 전달하는 공통 Command다.</summary>
    public ICommand Command { get; }

    /// <summary>
    /// Navigation 서비스의 현재 ViewModel을 반환한다.
    /// 자기 자신이 현재 ViewModel이면 재귀 DataContext 구성을 막기 위해 null을 반환하고,
    /// Navigation 대상이 없으면 디자인용 ViewModel을 반환한다.
    /// </summary>
    public object? CurrentViewModel
    {
        get
        {
            var current = Navigation?.CurrentViewModel;
            return ReferenceEquals(current, this) ? null : current ?? DesignTimeViewModel;
        }
    }

    /// <summary>WPF 디자인뷰에서 CurrentViewModel 대신 표시할 선택적 ViewModel이다.</summary>
    protected virtual object? DesignTimeViewModel { get; set; }

    /// <summary>화면 전환이 필요한 파생 ViewModel에서 연결할 Navigation 서비스다.</summary>
    protected INavigationService? Navigation { get; set; }

    /// <summary>
    /// 파생 ViewModel의 초기 속성, 이벤트, 기본 상태를 구성한다.
    /// </summary>
    protected virtual void Initialise() { }

    /// <summary>
    /// View에서 전달한 Enum Command를 파생 ViewModel의 기능으로 분기한다.
    /// </summary>
    /// <param name="command">사용자가 실행한 Command 종류다.</param>
    protected abstract void Execute(TCommand command);

    /// <summary>
    /// 현재 화면으로 전환된 직후 실행할 동작을 재정의한다.
    /// </summary>
    /// <param name="parameter">이전 화면이나 호출자가 전달한 선택적 데이터다.</param>
    public virtual void OnNavigatedTo(object? parameter = null)
    {
    }

    /// <summary>
    /// 현재 화면에서 이탈하기 직전에 실행할 동작을 재정의한다.
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
    }
}

/// <summary>
/// 공용 데이터 모델을 사용하며 Command, 속성 매핑, Navigation 수명주기를 제공하는 ViewModel 기반 클래스다.
/// </summary>
/// <typeparam name="TShared">여러 ViewModel이 함께 사용하는 공용 데이터 형식이다.</typeparam>
/// <typeparam name="TCommand">View에서 전달하는 Command 종류를 정의한 Enum 형식이다.</typeparam>
public abstract class BaseViewModel<TShared, TCommand> : ObservableValidator, INavigationNotice
    where TShared : class
    where TCommand : struct, Enum
{
    /// <summary>
    /// 공용 데이터 모델과 RelayCommand를 연결하고 실행 환경에 맞는 속성 매핑을 구성한다.
    /// </summary>
    /// <param name="shared">여러 ViewModel이 함께 사용하는 공용 데이터 모델이다.</param>
    /// <summary>Use this constructor when child View navigation is not required.</summary>
    protected BaseViewModel(TShared shared)
    {
        Shared = shared ?? throw new ArgumentNullException(nameof(shared));
        Command = new RelayCommand<TCommand>(Execute);

        // 공통 초기값과 이벤트를 먼저 구성한 뒤 디자인/런타임용 속성 매핑을 분리한다.
        Initialise();

        if (IsDesignMode)
        {
            DesignTimePropertiesMapping();
        }
        else
        {
            PropertiesMapping();
        }
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    protected BaseViewModel(TShared shared, INavigationService navigation)
        : this(shared)
    {
        AttachNavigation(navigation);
    }

    private void AttachNavigation(INavigationService navigation)
    {
        Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        Navigation.PropertyChanged += OnNavigationPropertyChanged;
    }

    private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    /// <summary>View에서 바인딩해 사용자 동작을 전달하는 공통 Command다.</summary>
    public ICommand Command { get; }

    /// <summary>
    /// Navigation 서비스의 현재 ViewModel을 반환한다.
    /// 자기 자신이면 null, 대상이 없으면 디자인용 ViewModel을 반환한다.
    /// </summary>
    public object? CurrentViewModel
    {
        get
        {
            var current = Navigation?.CurrentViewModel;
            return ReferenceEquals(current, this) ? null : current ?? DesignTimeViewModel;
        }
    }

    /// <summary>현재 코드가 WPF 디자인 도구 안에서 실행 중인지 나타낸다.</summary>
    protected static bool IsDesignMode =>
        DesignerProperties.GetIsInDesignMode(new DependencyObject());

    /// <summary>여러 ViewModel이 함께 사용하는 공용 데이터 모델이다.</summary>
    public TShared Shared { get; }

    /// <summary>
    /// 기존 BaseUI 파생 ViewModel과의 호환성을 위한 속성명/갱신 함수 매핑 필드다.
    /// Key는 공용 모델의 PropertyName, Value는 ViewModel 속성을 갱신하는 Action이다.
    /// </summary>
    protected Dictionary<string, Action> _sharedModelPropertyMap = new();

    /// <summary>정리된 이름으로 공용 모델 속성 매핑 Dictionary를 가져온다.</summary>
    protected Dictionary<string, Action> SharedModelPropertyMap => _sharedModelPropertyMap;

    /// <summary>WPF 디자인뷰에서 CurrentViewModel 대신 표시할 선택적 ViewModel이다.</summary>
    protected virtual object? DesignTimeViewModel { get; set; }

    /// <summary>화면 전환이 필요한 파생 ViewModel에서 연결할 Navigation 서비스다.</summary>
    protected INavigationService? Navigation { get; set; }

    /// <summary>
    /// 파생 ViewModel의 초기 속성, 이벤트, 기본 상태를 구성한다.
    /// </summary>
    protected virtual void Initialise() { }

    /// <summary>
    /// View에서 전달한 Enum Command를 파생 ViewModel의 기능으로 분기한다.
    /// </summary>
    /// <param name="command">사용자가 실행한 Command 종류다.</param>
    protected abstract void Execute(TCommand command);

    /// <summary>
    /// 공용 모델의 PropertyChanged 이벤트와 ViewModel 갱신 Action을 연결한다.
    /// 파생 클래스는 이 메서드에서 _sharedModelPropertyMap을 구성하고 이벤트를 등록한다.
    /// </summary>
    protected virtual void PropertiesMapping() { }

    /// <summary>
    /// WPF 디자인뷰에서 사용할 샘플 속성이나 디자인 전용 매핑을 구성한다.
    /// </summary>
    protected virtual void DesignTimePropertiesMapping()
    {
    }

    /// <summary>
    /// 공용 모델의 PropertyChanged 이벤트를 받아 등록된 ViewModel 갱신 Action을 실행한다.
    /// 기존 BaseUI 파생 클래스의 이벤트 연결 코드를 유지하기 위한 이름이다.
    /// </summary>
    /// <param name="sender">속성 변경을 발생시킨 공용 모델이다.</param>
    /// <param name="e">변경된 속성 이름을 포함한 이벤트 데이터다.</param>
    protected void PropertiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        // PropertyName과 연결된 Action만 실행해 불필요한 전체 속성 갱신을 피한다.
        if (e.PropertyName is not null &&
            _sharedModelPropertyMap.TryGetValue(e.PropertyName, out var update))
        {
            update();
        }
    }

    /// <summary>
    /// 의미가 명확한 이름으로 공용 모델 속성 변경을 처리한다.
    /// 내부적으로 기존 호환 메서드인 PropertiesChanged를 호출한다.
    /// </summary>
    /// <param name="sender">속성 변경을 발생시킨 공용 모델이다.</param>
    /// <param name="e">변경된 속성 이름을 포함한 이벤트 데이터다.</param>
    protected void OnSharedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertiesChanged(sender, e);
    }

    /// <summary>
    /// 현재 화면으로 전환된 직후 실행할 동작을 재정의한다.
    /// </summary>
    /// <param name="parameter">이전 화면이나 호출자가 전달한 선택적 데이터다.</param>
    public virtual void OnNavigatedTo(object? parameter = null)
    {
    }

    /// <summary>
    /// 현재 화면에서 이탈하기 직전에 실행할 동작을 재정의한다.
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
    }
}

/// <summary>
/// 화면 전용 Model과 공용 데이터 모델을 함께 사용하며 Command, 속성 매핑, Navigation 수명주기를 제공하는 ViewModel 기반 클래스다.
/// </summary>
/// <typeparam name="TModel">현재 화면의 데이터 처리와 업무 로직을 담당하는 Model 형식이다.</typeparam>
/// <typeparam name="TShared">여러 Model과 ViewModel이 함께 사용하는 공용 데이터 형식이다.</typeparam>
/// <typeparam name="TCommand">View에서 전달하는 Command 종류를 정의한 Enum 형식이다.</typeparam>
public abstract class BaseViewModel<TModel, TShared, TCommand> : ObservableValidator, INavigationNotice
    where TModel : ObservableObject, IModelWithSharedModel<TShared>
    where TShared : class
    where TCommand : struct, Enum
{
    /// <summary>
    /// 화면 전용 Model에 공용 데이터를 연결하고, Command와 실행 환경별 속성 매핑을 구성한다.
    /// </summary>
    /// <param name="model">현재 화면의 데이터 처리와 업무 로직을 담당하는 Model이다.</param>
    /// <param name="shared">여러 Model과 ViewModel이 함께 사용하는 공용 데이터 모델이다.</param>
    /// <summary>Use this constructor when child View navigation is not required.</summary>
    protected BaseViewModel(TModel model, TShared shared)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        Shared = shared ?? throw new ArgumentNullException(nameof(shared));

        // Model이 ViewModel과 동일한 공용 데이터 인스턴스를 사용하도록 먼저 연결한다.
        Model.AttachShared(Shared);
        Command = new RelayCommand<TCommand>(Execute);

        // 디자인뷰에서는 실제 장비/파일 처리 초기화를 건너뛰고 샘플 데이터만 구성한다.
        if (IsDesignMode)
        {
            DesignTimePropertiesMapping();
        }
        else
        {
            Initialise();
            PropertiesMapping();
        }

        // Model 속성 변경을 ViewModel이 필요한 방식으로 변환할 수 있도록 공통 이벤트 지점을 연결한다.
        Model.PropertyChanged += OnModelPropertyChanged;
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    protected BaseViewModel(TModel model, TShared shared, INavigationService navigation)
        : this(model, shared)
    {
        AttachNavigation(navigation);
    }

    private void AttachNavigation(INavigationService navigation)
    {
        Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        Navigation.PropertyChanged += OnNavigationPropertyChanged;
    }

    private void OnNavigationPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(INavigationService.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }

    /// <summary>하위 View 전환이 필요한 ViewModel용 생성자다. Navigation 변경 시 CurrentViewModel 바인딩도 갱신한다.</summary>
    /// <summary>View에서 바인딩해 사용자 동작을 전달하는 공통 Command다.</summary>
    public ICommand Command { get; }

    /// <summary>
    /// Navigation 서비스의 현재 ViewModel을 반환한다.
    /// 자기 자신이면 null, 대상이 없으면 디자인용 ViewModel을 반환한다.
    /// </summary>
    public object? CurrentViewModel
    {
        get
        {
            var current = Navigation?.CurrentViewModel;
            return ReferenceEquals(current, this) ? null : current ?? DesignTimeViewModel;
        }
    }

    /// <summary>현재 코드가 WPF 디자인 도구 안에서 실행 중인지 나타낸다.</summary>
    protected static bool IsDesignMode =>
        DesignerProperties.GetIsInDesignMode(new DependencyObject());

    /// <summary>현재 화면의 데이터 처리와 업무 로직을 담당하는 Model이다.</summary>
    protected TModel Model { get; }

    /// <summary>여러 Model과 ViewModel이 함께 사용하는 공용 데이터 모델이다.</summary>
    public TShared Shared { get; }

    /// <summary>
    /// 기존 BaseUI 파생 ViewModel과의 호환성을 위한 속성명/갱신 함수 매핑 필드다.
    /// Key는 공용 모델의 PropertyName, Value는 ViewModel 속성을 갱신하는 Action이다.
    /// </summary>
    protected Dictionary<string, Action> _sharedModelPropertyMap = new();

    /// <summary>정리된 이름으로 공용 모델 속성 매핑 Dictionary를 가져온다.</summary>
    protected Dictionary<string, Action> SharedModelPropertyMap => _sharedModelPropertyMap;

    /// <summary>WPF 디자인뷰에서 CurrentViewModel 대신 표시할 선택적 ViewModel이다.</summary>
    protected virtual object? DesignTimeViewModel { get; set; }

    /// <summary>화면 전환이 필요한 파생 ViewModel에서 연결할 Navigation 서비스다.</summary>
    protected INavigationService? Navigation { get; set; }

    /// <summary>
    /// 파생 ViewModel의 초기 속성, 이벤트, Model 기본 상태를 구성한다.
    /// 디자인 모드에서는 호출되지 않는다.
    /// </summary>
    protected virtual void Initialise() { }

    /// <summary>
    /// View에서 전달한 Enum Command를 파생 ViewModel의 기능으로 분기한다.
    /// </summary>
    /// <param name="command">사용자가 실행한 Command 종류다.</param>
    protected abstract void Execute(TCommand command);

    /// <summary>
    /// 공용 모델의 PropertyChanged 이벤트와 ViewModel 갱신 Action을 연결한다.
    /// 파생 클래스는 이 메서드에서 _sharedModelPropertyMap을 구성하고 이벤트를 등록한다.
    /// </summary>
    protected virtual void PropertiesMapping() { }

    /// <summary>
    /// WPF 디자인뷰에서 사용할 샘플 속성이나 디자인 전용 매핑을 구성한다.
    /// </summary>
    protected virtual void DesignTimePropertiesMapping()
    {
    }

    /// <summary>
    /// 공용 모델의 PropertyChanged 이벤트를 받아 등록된 ViewModel 갱신 Action을 실행한다.
    /// 기존 BaseUI 파생 클래스의 이벤트 연결 코드를 유지하기 위한 이름이다.
    /// </summary>
    /// <param name="sender">속성 변경을 발생시킨 공용 모델이다.</param>
    /// <param name="e">변경된 속성 이름을 포함한 이벤트 데이터다.</param>
    protected void PropertiesChanged(object? sender, PropertyChangedEventArgs e)
    {
        // PropertyName과 연결된 Action만 실행해 불필요한 전체 속성 갱신을 피한다.
        if (e.PropertyName is not null &&
            _sharedModelPropertyMap.TryGetValue(e.PropertyName, out var update))
        {
            update();
        }
    }

    /// <summary>
    /// 의미가 명확한 이름으로 공용 모델 속성 변경을 처리한다.
    /// 내부적으로 기존 호환 메서드인 PropertiesChanged를 호출한다.
    /// </summary>
    /// <param name="sender">속성 변경을 발생시킨 공용 모델이다.</param>
    /// <param name="e">변경된 속성 이름을 포함한 이벤트 데이터다.</param>
    protected void OnSharedPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertiesChanged(sender, e);
    }

    /// <summary>
    /// 화면 전용 Model의 속성이 변경됐을 때 ViewModel 속성을 갱신하도록 재정의하는 지점이다.
    /// </summary>
    /// <param name="sender">속성 변경을 발생시킨 화면 전용 Model이다.</param>
    /// <param name="e">변경된 속성 이름을 포함한 이벤트 데이터다.</param>
    protected virtual void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
    }

    /// <summary>
    /// 현재 화면으로 전환된 직후 실행할 동작을 재정의한다.
    /// </summary>
    /// <param name="parameter">이전 화면이나 호출자가 전달한 선택적 데이터다.</param>
    public virtual void OnNavigatedTo(object? parameter = null)
    {
    }

    /// <summary>
    /// 현재 화면에서 이탈하기 직전에 실행할 동작을 재정의한다.
    /// </summary>
    public virtual void OnNavigatedFrom()
    {
    }
}
