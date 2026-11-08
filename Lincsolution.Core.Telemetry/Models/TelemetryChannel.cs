using System;
using System.Collections.Generic;
using System.Text;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 하나의 Telemetry가 갖는 Channel 정보.
    /// 하나의 센서가 하나의 Channel에 해당한다고 볼 수 있음.
    /// </summary>
    public sealed class TelemetryChannel
    {
        /// <summary>TelemetrySession 내에서 중복되지 않는 Channel의 고유 ID</summary>
        public int ChannelId { get; }

        /// <summary>수집 데이터 명칭</summary>
        public string Name { get; }

        /// <summary>수집 데이터 단위. 없는 경우 빈 문자열.</summary>
        public string Unit { get; }

        /// <summary>수집 데이터의 데이터 자료형</summary>
        public TelemetryValueType ValueType { get; }

        /// <summary>ValueType이 TelemetryValueType.Real(실수 자료형)인 경우, 유지할 소숫점 이하 자릿수</summary>
        public int DecimalPlaces { get; }

        /// <summary>
        /// Channel 생성자
        /// </summary>
        /// <param name="channelId">Session에서 사용할 Channel의 양의 정수 ID</param>
        /// <param name="name">데이터 이름</param>
        /// <param name="unit">데이터 단위.</param>
        /// <param name="valueType">데이터 자료형</param>
        /// <param name="decimalPlaces">실수인 경우, 소숫점 자릿수</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public TelemetryChannel(int channelId, string name, string unit, TelemetryValueType valueType, int decimalPlaces)
        {
            /// ChannelID는 0보다 커야 함.
            if(channelId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(channelId), "Channel ID must be greater than zero.");
            }
            /// Name은 반드시 있어야 함.
            if(string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Channel Name cannot be empty.", nameof(name));
            }
            /// TelemetryValueType은 반드시 정의된 값 중 하나여야 함.
            if(!Enum.IsDefined(typeof(TelemetryValueType), valueType))
            {
                throw new ArgumentOutOfRangeException(nameof(valueType), "Value Type must be defined.");
            }
            /// 실수의 경우, DecimalPlaces가 0이상 15이하여야 함.
            if(decimalPlaces < 0 || decimalPlaces > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(decimalPlaces), "Decimal Places must be between 0 and 15");
            }
            /// ValueType이 실수가 아니면, DecimalPlaces 는 반드시 0이어야 함.
            if(valueType != TelemetryValueType.Real && decimalPlaces != 0)
            {
                throw new ArgumentException("Decimal places must be zero for non-Real values.", nameof(decimalPlaces));
            }

            ChannelId = channelId;
            Name = name;
            Unit = unit ?? string.Empty;
            ValueType = valueType;
            DecimalPlaces = decimalPlaces;
        }
    }
}
