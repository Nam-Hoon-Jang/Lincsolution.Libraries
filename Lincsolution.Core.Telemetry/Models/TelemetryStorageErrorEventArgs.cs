using System;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>백그라운드 저장 실패 알림. 오류가 발생한 Session과 원래 예외를 제공.</summary>
    public sealed class TelemetryStorageErrorEventArgs : EventArgs
    {
        /// <summary>오류 발생 Session ID. 새 Session의 상태와 혼동하지 않도록 비교.</summary>
        public Guid SessionId { get; }
        /// <summary>SQLite 쓰기 또는 저장값 변환 중 발생한 원래 예외.</summary>
        public Exception Exception { get; }

        /// <summary>Recorder 내부에서 오류 알림 객체 생성.</summary>
        internal TelemetryStorageErrorEventArgs(Guid sessionId, Exception exception)
        {
            SessionId = sessionId;
            Exception = exception;
        }
    }
}
