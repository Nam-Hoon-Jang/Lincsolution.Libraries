namespace Lincsolution.UI.Base.Common
{
    /// <summary>
    /// 여러 UI 라이브러리와 실행 애플리케이션이 공통으로 사용하는 화면 상태를 정의한다.
    /// </summary>
    public enum UiState
    {
        IDLE,
        BUSY,
        READY,
        BUILD,
        PAUSE,
        DONE,
        STOP,
        WARNING,
        ERROR
    }

    /// <summary>
    /// 여러 UI 라이브러리와 실행 애플리케이션이 공통으로 사용하는 팝업창 종류를 정의한다.
    /// INFO - 정보, WARNING - 경고, ERROR - 에러
    /// INFO - 정보, WARNING - 경고, ERROR - 오류
    /// </summary>
    public enum AlarmType
    {
        INFO,
        WARNING,
        ERROR
    }
}
