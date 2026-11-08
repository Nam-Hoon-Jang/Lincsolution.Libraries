using System;

namespace Lincsolution.Core.Base.Logging
{
    /// <summary>
    /// KO: 애플리케이션 공통 로그 기록 기능을 정의합니다.
    /// EN: Defines common application logging operations.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// KO: 디버그 수준 로그를 기록합니다.
        /// EN: Writes a debug-level log entry.
        /// </summary>
        void Debug(string message);

        /// <summary>
        /// KO: 정보 수준 로그를 기록합니다.
        /// EN: Writes an information-level log entry.
        /// </summary>
        void Info(string message);

        /// <summary>
        /// KO: 경고 수준 로그를 기록합니다.
        /// EN: Writes a warning-level log entry.
        /// </summary>
        void Warn(string message);

        /// <summary>
        /// KO: 오류 수준 로그를 기록합니다.
        /// EN: Writes an error-level log entry.
        /// </summary>
        void Error(string message, Exception exception = null);

        /// <summary>
        /// KO: 치명적 오류 수준 로그를 기록합니다.
        /// EN: Writes a fatal-level log entry.
        /// </summary>
        void Fatal(string message, Exception exception = null);
    }
}
