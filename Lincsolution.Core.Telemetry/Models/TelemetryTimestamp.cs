using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// Telemetry의 데이터와 이벤트 발생 시간 정보.
    /// 실제 시각과 TelemetrySession 경과 시간의 묶음.
    /// 수집된 데이터와 이벤트를 같은 시간축에서 비교하기 위한 기준이 되는 정보.
    /// </summary>
    public sealed class TelemetryTimestamp
    {
        /// <summary>데이터/이벤트 발생 시각</summary>
        public DateTimeOffset Date { get; }

        /// <summary>Session 시작 후 경과 시간</summary>
        public TimeSpan Elapsed  { get; }

        /// <summary>
        /// TelemetryTimestamp 생성자
        /// </summary>
        /// <param name="date">데이터/이벤트 발생 시각</param>
        /// <param name="elapsed">Session 시작 후 경과 시간</param>
        public TelemetryTimestamp(DateTimeOffset date, TimeSpan elapsed)
        {
            if(elapsed < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(elapsed), "Elapsed time cannot be negative.");
            }

            Date = date.ToUniversalTime();
            Elapsed = elapsed;
        }
    }
}
