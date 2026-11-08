using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Data.Sqlite;
using Lincsolution.Core.Telemetry.Models;

namespace Lincsolution.Core.Telemetry
{
    /// <summary>
    /// SchemaVersion 1 Telemetry DB 읽기 전용 API. 호출마다 독립 연결을 열고 닫으므로 병렬 조회 가능.
    /// 동기 I/O API이며 UI에서는 Task.Run과 CancellationToken을 사용. DB 파일을 생성하거나 수정하지 않음.
    /// 페이지 조회는 원본 값, Plot 조회는 메모리/표시량이 제한된 요약 데이터를 반환.
    /// </summary>
    public sealed class TelemetryQueryReader
    {
        private readonly string path;
        public TelemetryQueryReader(string databasePath)
        {
            if(string.IsNullOrWhiteSpace(databasePath)) throw new ArgumentException("Database path is required.", nameof(databasePath));
            path = Path.GetFullPath(databasePath);
        }

        /// <summary>세션 식별·종료 정보, 실제 저장 건수와 조회 가능한 마지막 경과 시간.</summary>
        public TelemetrySessionInfo ReadSession(CancellationToken cancellationToken = default(CancellationToken))
        {
            TelemetrySessionInfo result = null;
            Read(@"SELECT SessionId,BuildFileName,StartUtcTicks,EndUtcTicks,EndElapsedTicks,EndReason,
                (SELECT COUNT(*) FROM Frames),(SELECT COUNT(*) FROM Samples),(SELECT COUNT(*) FROM Events),
                MAX(COALESCE(EndElapsedTicks,0),COALESCE((SELECT MAX(ElapsedTicks) FROM Frames),0),COALESCE((SELECT MAX(ElapsedTicks) FROM Events),0))
                FROM Session WHERE Id=1;", null, r =>
            {
                var session = new TelemetrySession(Guid.Parse(r.GetString(0)), r.GetString(1), Utc(r.GetInt64(2)));
                var end = r.IsDBNull(3) || r.IsDBNull(4) ? null : new TelemetryTimestamp(Utc(r.GetInt64(3)), TimeSpan.FromTicks(r.GetInt64(4)));
                result = new TelemetrySessionInfo(session, end, r.IsDBNull(5) ? null : r.GetString(5), r.GetInt64(9), r.GetInt64(6), r.GetInt64(7), r.GetInt64(8));
            }, cancellationToken);
            if(result is null) throw new InvalidDataException("Session is missing.");
            return result;
        }

        /// <summary>등록 채널 정의를 ID 순으로 반환. 잘못된 DB의 무제한 메모리 사용을 막기 위해 최대 10,000개.</summary>
        public IReadOnlyList<TelemetryChannel> ReadChannels(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new List<TelemetryChannel>();
            Read("SELECT ChannelId,Name,Unit,ValueType,DecimalPlaces FROM Channels ORDER BY ChannelId LIMIT 10001;", null, r =>
            {
                if(result.Count == 10000) throw new InvalidDataException("Too many channels.");
                result.Add(new TelemetryChannel(r.GetInt32(0),r.GetString(1),r.GetString(2),(TelemetryValueType)r.GetInt32(3),r.GetInt32(4)));
            }, cancellationToken);
            return result.AsReadOnly();
        }

        private const string SampleSql = @"SELECT s.FrameNumber,s.ChannelId,f.UtcTicks,f.ElapsedTicks,s.Value,s.Status,s.SourceUtcTicks,c.ValueType,c.DecimalPlaces
            FROM Samples s JOIN Frames f ON f.FrameNumber=s.FrameNumber JOIN Channels c ON c.ChannelId=s.ChannelId
            WHERE f.ElapsedTicks >= $from AND f.ElapsedTicks <= $to AND ($channel IS NULL OR s.ChannelId=$channel)
            ORDER BY f.ElapsedTicks,s.FrameNumber,s.ChannelId";

        /// <summary>시간 범위(양 끝 포함)의 원본 센서값 페이지. 채널 null이면 전체, limit은 1~10,000.</summary>
        public IReadOnlyList<TelemetrySample> ReadSamples(TimeSpan from, TimeSpan to, int? channelId = null,
            int offset = 0, int limit = 200, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRange(from,to); ValidatePage(offset,limit);
            var result = new List<TelemetrySample>();
            Read(SampleSql + " LIMIT $limit OFFSET $offset;", c =>
            { Range(c,from,to,channelId); c.Parameters.AddWithValue("$limit",limit); c.Parameters.AddWithValue("$offset",offset); },
                r => result.Add(Sample(r)), cancellationToken);
            return result.AsReadOnly();
        }

        /// <summary>같은 경과 시간축의 이벤트 원본 페이지. 작업 시작/종료 비교에 사용.</summary>
        public IReadOnlyList<TelemetryEvent> ReadEvents(TimeSpan from, TimeSpan to, int offset = 0, int limit = 200,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRange(from,to); ValidatePage(offset,limit);
            var result = new List<TelemetryEvent>();
            Read(@"SELECT UtcTicks,ElapsedTicks,EventCode,Source,OperationId,Message FROM Events
                WHERE ElapsedTicks >= $from AND ElapsedTicks <= $to ORDER BY ElapsedTicks,EventId LIMIT $limit OFFSET $offset;",
                c => { c.Parameters.AddWithValue("$from",from.Ticks); c.Parameters.AddWithValue("$to",to.Ticks); c.Parameters.AddWithValue("$limit",limit); c.Parameters.AddWithValue("$offset",offset); },
                r => result.Add(new TelemetryEvent(new TelemetryTimestamp(Utc(r.GetInt64(0)),TimeSpan.FromTicks(r.GetInt64(1))),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5))),cancellationToken);
            return result.AsReadOnly();
        }

        /// <summary>
        /// 선택 채널의 표시용 데이터 조회. 최대 100채널, 채널당 12~6,000점으로 반환량 제한.
        /// 전체 원본은 스트리밍으로 읽으며 시간 구간마다 첫/마지막/최소/최대/첫 누락/마지막 누락만 보관.
        /// 원본이 한도 이하면 그대로 반환. 초과하면 피크와 누락 표시를 남긴 요약 반환.
        /// 요약은 모든 전환을 보존하지 않으므로 상세 분석은 시간 범위를 좁히거나 ReadSamples 사용.
        /// </summary>
        public IReadOnlyList<TelemetryPlotSeries> ReadPlot(IEnumerable<int> channelIds, TimeSpan from, TimeSpan to,
            int maxPointsPerChannel = 1200, CancellationToken cancellationToken = default(CancellationToken))
        {
            ValidateRange(from,to);
            if(channelIds is null) throw new ArgumentNullException(nameof(channelIds));
            if(maxPointsPerChannel < 12 || maxPointsPerChannel > 6000) throw new ArgumentOutOfRangeException(nameof(maxPointsPerChannel));
            var ids = new HashSet<int>();
            foreach(int id in channelIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ids.Add(id); if(ids.Count > 100) throw new ArgumentException("Select at most 100 channels.",nameof(channelIds));
            }
            var channels = ReadChannels(cancellationToken).ToDictionary(c => c.ChannelId);
            foreach(int id in ids) if(!channels.ContainsKey(id)) throw new ArgumentException("Unknown channel: " + id,nameof(channelIds));
            var result = new List<TelemetryPlotSeries>();
            foreach(int id in ids.OrderBy(x => x))
            {
                var raw = new List<TelemetrySample>(maxPointsPerChannel);
                int bucketCount = maxPointsPerChannel / 6;
                var buckets = new Bucket[bucketCount];
                long count = 0;
                Read(SampleSql.Replace("($channel IS NULL OR s.ChannelId=$channel)", "s.ChannelId=$channel") + ";", c => Range(c,from,to,id), r =>
                {
                    var point = Sample(r); count++;
                    if(raw != null)
                    {
                        if(raw.Count < maxPointsPerChannel) raw.Add(point);
                        else raw = null;
                    }
                    double span = (double)to.Ticks - from.Ticks;
                    int index = span <= 0 ? 0 : Math.Min(bucketCount-1,(int)(((double)point.Timestamp.Elapsed.Ticks-from.Ticks)/span*bucketCount));
                    if(buckets[index] is null) buckets[index] = new Bucket();
                    buckets[index].Add(point);
                }, cancellationToken);
                var points = raw ?? buckets.Where(b => b != null).SelectMany(b => b.Points())
                    .OrderBy(p => p.Timestamp.Elapsed).ThenBy(p => p.FrameNumber).ToList();
                result.Add(new TelemetryPlotSeries(channels[id],points.AsReadOnly(),count));
            }
            return result.AsReadOnly();
        }

        /// <summary>고정 크기 구간 누적기. 값이 없는 구간을 선으로 이어 그리지 않도록 null 점도 보존.</summary>
        private sealed class Bucket
        {
            private TelemetrySample first,last,min,max,firstGap,lastGap;
            internal void Add(TelemetrySample point)
            {
                if(first is null) first=point; last=point;
                if(!point.Value.HasValue) { if(firstGap is null) firstGap=point; lastGap=point; }
                else
                {
                    if(min is null || point.Value.Value < min.Value.Value) min=point;
                    if(max is null || point.Value.Value > max.Value.Value) max=point;
                }
            }
            internal IEnumerable<TelemetrySample> Points() { return new[]{first,last,min,max,firstGap,lastGap}.Where(p=>p!=null).Distinct(); }
        }

        private void Read(string sql, Action<SqliteCommand> parameters, Action<SqliteDataReader> consume, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            using(var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=path,Mode=SqliteOpenMode.ReadOnly,Pooling=false,DefaultTimeout=2 }.ToString()))
            {
                connection.Open();
                using(var schema=connection.CreateCommand())
                {
                    schema.CommandText="SELECT SchemaVersion FROM Session WHERE Id=1;";
                    if(Convert.ToInt32(schema.ExecuteScalar())!=1) throw new InvalidDataException("Unsupported Telemetry schema.");
                }
                using(var command=connection.CreateCommand())
                {
                    command.CommandText=sql; parameters?.Invoke(command);
                    using(token.Register(command.Cancel))
                    {
                        try
                        {
                            using(var reader=command.ExecuteReader())
                                while(reader.Read()) { token.ThrowIfCancellationRequested(); consume(reader); }
                        }
                        catch(SqliteException) when(token.IsCancellationRequested) { throw new OperationCanceledException(token); }
                    }
                }
            }
        }
        private static TelemetrySample Sample(SqliteDataReader r)
        {
            long? raw=r.IsDBNull(4)?(long?)null:r.GetInt64(4);
            var status=(TelemetryDataStatus)r.GetInt32(5);
            int type=r.GetInt32(7), places=r.GetInt32(8);
            if(!Enum.IsDefined(typeof(TelemetryDataStatus),status) || type<0 || type>2 || places<0 || places>15)
                throw new InvalidDataException("Invalid sample metadata.");
            double? value=raw.HasValue?(double?)raw.Value:null;
            if(type==(int)TelemetryValueType.Real && value.HasValue) value/=Math.Pow(10,places);
            if(status==TelemetryDataStatus.Missing || status==TelemetryDataStatus.Error) value=null;
            return new TelemetrySample(r.GetInt64(0),r.GetInt32(1),new TelemetryTimestamp(Utc(r.GetInt64(2)),TimeSpan.FromTicks(r.GetInt64(3))),raw,value,status,r.IsDBNull(6)?(DateTimeOffset?)null:Utc(r.GetInt64(6)));
        }
        private static DateTimeOffset Utc(long ticks) { return new DateTimeOffset(ticks,TimeSpan.Zero); }
        private static void Range(SqliteCommand c,TimeSpan from,TimeSpan to,int? channel)
        { c.Parameters.AddWithValue("$from",from.Ticks); c.Parameters.AddWithValue("$to",to.Ticks); c.Parameters.AddWithValue("$channel",(object)channel??DBNull.Value); }
        private static void ValidateRange(TimeSpan from,TimeSpan to)
        { if(from<TimeSpan.Zero || to<from) throw new ArgumentOutOfRangeException(nameof(from),"Use 0 <= from <= to."); }
        private static void ValidatePage(int offset,int limit)
        { if(offset<0 || limit<1 || limit>10000) throw new ArgumentOutOfRangeException(nameof(limit)); }
    }
}
