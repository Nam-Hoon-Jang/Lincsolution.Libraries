namespace Lincsolution.Core.Base.Alarms
{
    /// <summary>
    /// UI 표현 방식과 무관하게 Alarm의 중요도를 구분한다.
    /// </summary>
    public enum AlarmSeverity
    {
        /// <summary>사용자에게 상태나 참고 정보를 전달한다.</summary>
        Information,

        /// <summary>즉시 중단할 오류는 아니지만 사용자 확인이나 조치가 필요하다.</summary>
        Warning,

        /// <summary>기능 수행이 실패했거나 정상 동작을 계속할 수 없는 오류다.</summary>
        Error,

        /// <summary>시스템 안전이나 데이터 무결성에 영향을 줄 수 있는 치명적 오류다.</summary>
        Critical
    }
}