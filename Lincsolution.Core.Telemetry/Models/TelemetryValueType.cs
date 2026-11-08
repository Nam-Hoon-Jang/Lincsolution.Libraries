using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// Telemetry에서 수집하는 데이터 자료형 정보.
    /// </summary>
    public enum TelemetryValueType
    {
        /// <summary>정수</summary>
        Int = 0,

        /// <summary>실수</summary>
        Real,

        /// <summary>True/False</summary>
        Boolean
    }
}
