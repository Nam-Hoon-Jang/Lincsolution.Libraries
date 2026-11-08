using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// Telemetry Session의 종료 사유 정보.
    /// </summary>
    public enum TelemetryEndReason
    {
        /// <summary>Build 정상 종료</summary>
        Completed = 0,

        /// <summary>Build 취소</summary>
        Cancelled,

        /// <summary>Build 실패</summary>
        Failed
    }
}
