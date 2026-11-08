using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 수집된 TelemetryData의 상태 정보.
    /// </summary>
    public enum TelemetryDataStatus
    {
        /// <summary>정상</summary>
        Good = 0,

        /// <summary>누락됨</summary>
        Missing,

        /// <summary>통신/측정 오류로 사용 불가.</summary>
        Error,

        /// <summary>새로 수집하지 못해, 이전 값 사용</summary>
        Stale
    }
}
