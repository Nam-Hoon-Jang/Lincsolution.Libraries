using System;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>
    /// 애플리케이션이 Recorder에 전달하는 센서 하나의 수집 결과.
    /// Channel의 자료형은 등록 정보를 사용하므로 호출자가 다시 지정하지 않음.
    /// 값과 상태의 일치 검사는 Recorder가 TelemetryData를 생성할 때 수행.
    /// </summary>
    public sealed class TelemetryReading
    {
        /// <summary>StartSession에서 등록한 Channel ID.</summary>
        public int ChannelId { get; }
        /// <summary>정수 채널 값. 실수/Boolean 채널 또는 누락/오류 상태면 null.</summary>
        public long? IntValue { get; }
        /// <summary>실수 채널 값. 반올림은 기존 저장소에서 Channel 정밀도에 따라 수행.</summary>
        public double? RealValue { get; }
        /// <summary>Boolean 채널 값. 해당 자료형이 아니면 null.</summary>
        public bool? BoolValue { get; }
        /// <summary>Good/Stale은 값 하나 필요. Missing/Error는 모든 값이 null이어야 함.</summary>
        public TelemetryDataStatus Status { get; }
        /// <summary>장치가 제공한 원본 측정 시각. 없으면 null. Frame 공통 시각과 구분.</summary>
        public DateTimeOffset? SourceDate { get; }

        /// <summary>수집 결과를 변경 불가능한 객체로 보관. 값은 이름 있는 인수로 지정 권장.</summary>
        /// <param name="channelId">등록된 Channel의 ID.</param>
        /// <param name="intValue">정수 값.</param>
        /// <param name="realValue">실수 값.</param>
        /// <param name="boolValue">Boolean 값.</param>
        /// <param name="status">수집 상태. 기본값은 Good.</param>
        /// <param name="sourceDate">장치의 원본 측정 시각.</param>
        public TelemetryReading(int channelId, long? intValue = null,
            double? realValue = null, bool? boolValue = null,
            TelemetryDataStatus status = TelemetryDataStatus.Good,
            DateTimeOffset? sourceDate = null)
        {
            ChannelId = channelId;
            IntValue = intValue;
            RealValue = realValue;
            BoolValue = boolValue;
            Status = status;
            SourceDate = sourceDate?.ToUniversalTime();
        }
    }
}
