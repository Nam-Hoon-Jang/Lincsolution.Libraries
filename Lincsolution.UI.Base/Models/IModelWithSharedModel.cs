namespace Lincsolution.UI.Base.Models;

/// <summary>
/// Model이 애플리케이션 공용 데이터 모델을 주입받을 수 있도록 정의하는 계약이다.
/// </summary>
/// <typeparam name="TShared">여러 Model과 ViewModel이 함께 사용하는 공용 데이터 형식이다.</typeparam>
public interface IModelWithSharedModel<in TShared>
{
    /// <summary>
    /// Model에 공용 데이터 모델을 연결한다.
    /// </summary>
    /// <param name="shared">연결할 공용 데이터 모델 인스턴스다.</param>
    void AttachShared(TShared shared);
}
