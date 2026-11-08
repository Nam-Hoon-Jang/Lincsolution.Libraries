using System;
using System.Collections.Generic;

namespace Lincsolution.Core.Telemetry.Models
{
    /// <summary>DB의 세션 요약. 종료 정보가 null이면 수집 중 또는 비정상 종료된 기록.</summary>
    public sealed class TelemetrySessionInfo
    {
        public TelemetrySession Session { get; }
        public TelemetryTimestamp End { get; }
        public string EndReason { get; }
        public TimeSpan LastElapsed { get; }
        public long FrameCount { get; }
        public long SampleCount { get; }
        public long EventCount { get; }
        internal TelemetrySessionInfo(TelemetrySession session, TelemetryTimestamp end, string reason,
            long elapsed, long frames, long samples, long events)
        { Session = session; End = end; EndReason = reason; LastElapsed = TimeSpan.FromTicks(elapsed); FrameCount = frames; SampleCount = samples; EventCount = events; }
    }

    /// <summary>
    /// 조회한 센서값 한 건. StoredValue는 손실 없는 DB 정수 원본, Value는 Plot용 실수 변환값.
    /// 큰 정수는 double의 유효 정밀도를 넘을 수 있으므로 정밀 수치 처리는 StoredValue 사용.
    /// </summary>
    public sealed class TelemetrySample
    {
        public long FrameNumber { get; }
        public int ChannelId { get; }
        public TelemetryTimestamp Timestamp { get; }
        public long? StoredValue { get; }
        public double? Value { get; }
        public TelemetryDataStatus Status { get; }
        public DateTimeOffset? SourceDate { get; }
        internal TelemetrySample(long frame, int channel, TelemetryTimestamp time, long? stored,
            double? value, TelemetryDataStatus status, DateTimeOffset? source)
        { FrameNumber = frame; ChannelId = channel; Timestamp = time; StoredValue = stored; Value = value; Status = status; SourceDate = source; }
    }

    /// <summary>한 채널의 표시용 조회 결과. 원본 개수와 축약 여부를 제공하여 원본 조회와 구분.</summary>
    public sealed class TelemetryPlotSeries
    {
        public TelemetryChannel Channel { get; }
        public IReadOnlyList<TelemetrySample> Points { get; }
        public long SourceCount { get; }
        public bool IsReduced { get { return SourceCount > Points.Count; } }
        internal TelemetryPlotSeries(TelemetryChannel channel, IReadOnlyList<TelemetrySample> points, long count)
        { Channel = channel; Points = points; SourceCount = count; }
    }
}
