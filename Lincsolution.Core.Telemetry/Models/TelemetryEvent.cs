using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 불특정 이벤트 발생 시 기록하는 Telemetry 이벤트 정보.
    /// 일정 주기로 수집되는 Channel, Data와 달리, 불특정 시점에 발생하는 이벤트를 기록하기 위한 정보.
    /// </summary>
    public sealed class TelemetryEvent
    {
        /// <summary>TelemetryEvent 발생 시각</summary>
        public TelemetryTimestamp Timestamp { get; }

        /// <summary>이벤트 종류 구분 코드</summary>
        public string EventCode { get; }

        /// <summary>이벤트 발생 대상. 없으면 빈 문자열</summary>
        public string Source { get; }

        /// <summary>같은 작업의 시작과 종료를 연결하는 실행의 ID. 없으면 빈 문자열</summary>
        public string OperationId { get; }

        /// <summary>이벤트에 대한 메세지. 없으면 빈 문자열</summary>
        public string Message { get; }

        /// <summary>이벤트 시간과 식별 정보 초기화.</summary>
        /// <param name="timestamp">이벤트 발생 시간.</param>
        /// <param name="eventCode">이벤트 종류를 구분하는 코드.</param>
        /// <param name="source">이벤트가 발생한 장치 또는 기능.</param>
        /// <param name="operationId">같은 작업 실행을 연결하는 ID.</param>
        /// <param name="message">이벤트에 대한 추가 설명.</param>
        /// <exception cref="ArgumentNullException">
        /// 시간 정보가 null인 경우.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 이벤트 코드가 비어 있는 경우.
        /// </exception>
        public TelemetryEvent(TelemetryTimestamp timestamp, string eventCode, 
                              string source = "", string operationId = "", string message = "")
        {
            if(timestamp is null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            if(string.IsNullOrWhiteSpace(eventCode))
            {
                throw new ArgumentException("Event code cannot be empty.", nameof(eventCode));
            }

            Timestamp = timestamp;
            EventCode = eventCode;
            Source = source ?? string.Empty;
            OperationId = operationId ?? string.Empty;
            Message = message ?? string.Empty;
        }
    }
}
