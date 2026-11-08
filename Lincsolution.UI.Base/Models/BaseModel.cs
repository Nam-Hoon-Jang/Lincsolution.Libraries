using CommunityToolkit.Mvvm.ComponentModel;

namespace Lincsolution.UI.Base.Models;

/// <summary>
/// 속성 변경 알림, 공용 데이터 연결, 명시적 리소스 해제를 제공하는 Model 기반 클래스다.
/// </summary>
/// <typeparam name="TShared">여러 Model과 ViewModel이 함께 사용하는 공용 데이터 형식이다.</typeparam>
public abstract class BaseModel<TShared> : ObservableObject, IDisposable, IModelWithSharedModel<TShared>
{
    /// <summary>현재 Model에 연결된 공용 데이터 모델을 보관한다.</summary>
    private TShared? _shared;

    /// <summary>중복 Dispose 호출을 방지하기 위한 상태다.</summary>
    private bool _disposed;

    /// <summary>
    /// 기존 BaseUI 파생 Model과의 호환성을 위해 제공하는 공용 데이터 접근자다.
    /// AttachShared 호출 전에 접근하면 예외가 발생한다.
    /// </summary>
    protected TShared Sh =>
        _shared ?? throw new InvalidOperationException("Shared model is not attached.");

    /// <summary>
    /// 의미가 명확한 이름으로 공용 데이터 모델을 가져온다.
    /// 기존 코드 호환용 <see cref="Sh"/>와 동일한 인스턴스를 반환한다.
    /// </summary>
    protected TShared Shared => Sh;

    /// <summary>
    /// 공용 데이터 모델이 필요하지 않거나 나중에 연결할 Model을 초기화한다.
    /// </summary>
    protected BaseModel()
    {
    }

    /// <summary>
    /// 생성 시점에 공용 데이터 모델을 바로 연결하는 Model을 초기화한다.
    /// </summary>
    /// <param name="shared">연결할 공용 데이터 모델 인스턴스다.</param>
    protected BaseModel(TShared shared)
    {
        AttachShared(shared);
    }

    /// <summary>
    /// 공용 데이터 모델을 한 번만 연결한다.
    /// 동일 인스턴스의 재연결은 허용하고 다른 인스턴스로의 교체는 차단한다.
    /// </summary>
    /// <param name="shared">연결할 공용 데이터 모델 인스턴스다.</param>
    /// <exception cref="ArgumentNullException">공용 데이터 모델이 null일 때 발생한다.</exception>
    /// <exception cref="InvalidOperationException">이미 다른 공용 데이터 모델이 연결되어 있을 때 발생한다.</exception>
    public virtual void AttachShared(TShared shared)
    {
        ArgumentNullException.ThrowIfNull(shared);

        // 최초 연결만 내부 필드에 저장한다.
        if (_shared is null)
        {
            _shared = shared;
            return;
        }

        // 실행 중 공용 상태가 조용히 다른 인스턴스로 바뀌는 오류를 방지한다.
        if (!ReferenceEquals(_shared, shared))
        {
            throw new InvalidOperationException("A different shared model instance is already attached.");
        }
    }

    /// <summary>
    /// 파생 Model이 보유한 관리 리소스를 한 번만 해제한다.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        // 파생 클래스가 구현한 실제 정리 코드를 먼저 실행한 뒤 Dispose 완료 상태를 기록한다.
        DisposeResources();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 파생 Model이 Timer, Stream, SDK Handle 같은 관리 리소스를 해제하도록 재정의하는 지점이다.
    /// </summary>
    protected virtual void DisposeResources()
    {
    }
}
