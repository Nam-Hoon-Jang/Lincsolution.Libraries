using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// Telemetry에서 수집하는 Channel 데이터
    /// 하나의 Channel(센서)에 대응하여, 수집한 Channel의 데이터
    /// </summary>
    public sealed class TelemetryData
    {
        /// <summary>데이터를 수집한 Channel ID.</summary>
        public int ChannelId { get; }

        /// <summary>데이터 값의 자료형.</summary>
        public TelemetryValueType ValueType { get; }

        /// <summary>수집한 데이터 상태.</summary>
        public TelemetryDataStatus Status { get; }

        /// <summary>정수 값. 해당 자료형이 아니거나 값이 없으면 null.</summary>
        public long? IntValue { get; }

        /// <summary>실수 값. 해당 자료형이 아니거나 값이 없으면 null.</summary>
        public double? RealValue { get; }

        /// <summary>Boolean 값. 해당 자료형이 아니거나 값이 없으면 null.</summary>
        public bool? BoolValue { get; }

        /// <summary>장치에서 제공한 원본 측정 시각. 없으면 null.</summary>
        public DateTimeOffset? SourceDate { get; }

        /// <summary>데이터 값과 상태 초기화.</summary>
        /// <param name="channelId">등록한 Channel의 양의 정수 ID.</param>
        /// <param name="valueType">센서 값의 자료형.</param>
        /// <param name="status">센서 데이터 수집 상태.</param>
        /// <param name="intValue">정수 값.</param>
        /// <param name="realValue">실수 값.</param>
        /// <param name="boolValue">Boolean 값.</param>
        /// <param name="sourceDate">장치에서 제공한 원본 측정 시각.</param>
        public TelemetryData(int channelId, TelemetryValueType valueType, TelemetryDataStatus status, long? intValue = null, double? realValue = null, bool? boolValue = null, DateTimeOffset? sourceDate = null)
        {
            /// ChannelID는 0보다 커야 함.
            if(channelId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelId), "Channel ID must be greater than zero.");
            }
            /// TelemetryValueType은 반드시 정의되야 함.
            if(!Enum.IsDefined(typeof(TelemetryValueType), valueType))
            {
                throw new ArgumentOutOfRangeException(nameof(valueType), "Telemetry Value Type must be defined.");
            }
            /// TelemetryDataStatus는 반드시 정의되야 함.
            if(!Enum.IsDefined(typeof(TelemetryDataStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status), "Telemetry Data Status must be defined.");
            }

            int valueCount = (intValue.HasValue ? 1 : 0) +
                             (realValue.HasValue ? 1 : 0) +
                             (boolValue.HasValue ? 1 : 0);

            bool requiresValue = status == TelemetryDataStatus.Good ||
                                 status == TelemetryDataStatus.Stale;

            if(requiresValue)
            {
                bool matchesType = (valueType == TelemetryValueType.Int && intValue.HasValue) ||
                                   (valueType == TelemetryValueType.Real && realValue.HasValue) ||
                                   (valueType == TelemetryValueType.Boolean && boolValue.HasValue);

                if(valueCount != 1 || !matchesType)
                {
                    throw new ArgumentException("Value Type must be only one.", nameof(valueType));
                }
            }
            else if(valueCount != 0)
            {
                throw new ArgumentException("If the Data doesn't require a value, Data must not have a value.");
            }

            if(realValue.HasValue && (double.IsNaN(realValue.Value) || double.IsInfinity(realValue.Value)))
            {
                throw new ArgumentOutOfRangeException(nameof(realValue), "Real value must be Finite.");
            }

            ChannelId = channelId;
            ValueType = valueType;
            Status = status;
            IntValue = intValue;
            RealValue = realValue;
            BoolValue = boolValue;
            SourceDate = sourceDate.HasValue ? sourceDate.Value.ToUniversalTime() : (DateTimeOffset?)null;
        }
    }
}
