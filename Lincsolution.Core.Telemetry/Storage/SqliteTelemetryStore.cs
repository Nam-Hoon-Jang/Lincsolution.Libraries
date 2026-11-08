using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Lincsolution.Core.Telemetry.Models;
using System.Data;

namespace Lincsolution.Core.Telemetry.Storage
{
    /// <summary>
    /// Telemetry 데이터의 SQLite 처리.
    /// DB 준비, Session/Channel/Frame/Samples/Event 저장, Session 종료 처리.
    /// DB 내부 Table들을 용도별로 나눈 구조.
    /// Session/Channels/Frame/Samples/Events 
    /// </summary>
    internal sealed class SqliteTelemetryStore : IDisposable
    {
        private readonly SqliteConnection connection;
        private bool disposed;

        /// <summary>
        /// 생성자. 
        /// CreateNew 로만 호출 가능.
        /// 사용할 DB 연결을 받아 SqliteTelemetryStore 객체를 생성.
        /// </summary>
        /// <param name="connection">사용할 SQLite DB 연결</param>
        private SqliteTelemetryStore(SqliteConnection connection)
        {
            this.connection = connection;
        }

        private const string createTableContext =@"
        CREATE TABLE IF NOT EXISTS Session (
            Id              INTEGER PRIMARY KEY CHECK (Id = 1),
            SessionId       TEXT NOT NULL UNIQUE,
            BuildFileName   TEXT NOT NULL,
            StartUtcTicks   INTEGER NOT NULL,
            EndUtcTicks     INTEGER,
            EndElapsedTicks INTEGER,
            EndReason       TEXT,
            SchemaVersion   INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Channels (
            ChannelId       INTEGER PRIMARY KEY CHECK (ChannelId > 0),
            Name            TEXT NOT NULL,
            Unit            TEXT NOT NULL,
            ValueType       INTEGER NOT NULL,
            DecimalPlaces   INTEGER NOT NULL
                            CHECK (DecimalPlaces BETWEEN 0 AND 15)
        );

        CREATE TABLE IF NOT EXISTS Frames (
            FrameNumber     INTEGER PRIMARY KEY CHECK (FrameNumber >= 0),
            UtcTicks        INTEGER NOT NULL,
            ElapsedTicks    INTEGER NOT NULL CHECK (ElapsedTicks >= 0)
        );

        CREATE TABLE IF NOT EXISTS Samples (
            FrameNumber     INTEGER NOT NULL,
            ChannelId       INTEGER NOT NULL,
            Value           INTEGER,
            Status          INTEGER NOT NULL,
            SourceUtcTicks  INTEGER,

            PRIMARY KEY (ChannelId, FrameNumber),

            FOREIGN KEY (FrameNumber) REFERENCES Frames(FrameNumber),
            FOREIGN KEY (ChannelId) REFERENCES Channels(ChannelId)
        ) WITHOUT ROWID;

        CREATE TABLE IF NOT EXISTS Events (
            EventId         INTEGER PRIMARY KEY,
            UtcTicks        INTEGER NOT NULL,
            ElapsedTicks    INTEGER NOT NULL CHECK (ElapsedTicks >= 0),
            EventCode       TEXT NOT NULL,
            Source          TEXT NOT NULL,
            OperationId     TEXT NOT NULL,
            Message         TEXT NOT NULL
        );

        CREATE INDEX IF NOT EXISTS IX_Frames_ElapsedTicks
        ON Frames(ElapsedTicks);

        CREATE INDEX IF NOT EXISTS IX_Events_ElapsedTicks
        ON Events(ElapsedTicks);
        ";

        /// <summary>
        /// 새 DB 파일을 생성하고, Session과 Channel 정보를 저장한 SqliteTelemetryStore 객체를 반환.
        /// DB경로, session, channels 유효성 검사를 수행하고, 새 DB파일 생성.
        /// 이후 SQLite 연결을 열고, Table 생성 후 Session과 Channel 정보를 저장.
        /// 그 다음, 사용 가능한 Stroe를 반환.
        /// 기존 DB 파일이 있는 경우, 덮어쓰지 않고 실패.
        /// </summary>
        /// <param name="databasePath">DB 경로</param>
        /// <param name="session">기록할 Session 정보</param>
        /// <param name="channels">이번 Session에 등록한 Channel 목록</param>
        /// <returns>생성된 SqliteTelemetryStore 객체</returns>
        /// <exception cref="ArgumentException">databasePath가 비어있거나 공백인 경우</exception>
        /// <exception cref="ArgumentNullException">session 또는 channels가 null인 경우</exception>
        internal static SqliteTelemetryStore CreateNew(string databasePath, TelemetrySession session, IEnumerable<TelemetryChannel> channels)
        {
            /// DB 경로 유효성 검사
            if(string.IsNullOrWhiteSpace(databasePath))
            {
                throw new ArgumentException("Database Path cannot be empty.", nameof(databasePath));
            }
            /// 세션 & 채널 null 검사
            if(session is null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if(channels is null)
            {
                throw new ArgumentNullException(nameof(channels));
            }
            /// DB 파일 생성 전에 입력 목록을 확정하고, 검사.
            var channelList = new List<TelemetryChannel>();
            var channelIds = new HashSet<int>();

            foreach(TelemetryChannel channel in channels)
            {
                /// channels 안의 channel 에 null 있는지 검사.
                if(channel is null)
                {
                    throw new ArgumentException("Channels must not contain null items.", nameof(channels));
                }
                /// channels 안의 channel.ChannelId 중복 검사.
                if(!channelIds.Add(channel.ChannelId))
                {
                    throw new ArgumentException($"Duplicate Channel ID : {channel.ChannelId}", nameof(channels));
                }
                /// 문제 없으면 리스트에 추가.
                channelList.Add(channel);
            }

            if(channelList.Count == 0)
            {
                throw new ArgumentException("At least one Channel is required.", nameof(channels));
            }

            string fullPath = Path.GetFullPath(databasePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            /// CreateNew 함수에서는 기존 파일 덮어쓰지 않고, 새 빈 파일 생성.
            /// 이후 SQLite에서 파일 초기화.
            using(var fs = new FileStream(
                fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {

            }

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = fullPath,
                Mode = SqliteOpenMode.ReadWrite,
                Pooling = false
            };

            var newConnection = new SqliteConnection(builder.ToString());

            try
            {
                newConnection.Open();
                CreateTables(newConnection);

                var store = new SqliteTelemetryStore(newConnection);
                store.InsertSessionAndChannels(session, channelList);
                return store;
            }
            catch
            {
                /// 초기화 실패 시, 열린 connection 해제하고, 예외 전달.
                newConnection.Dispose();
                throw;
            }
        }

        /// <summary>
        /// foreign key 검사 기능을 활성화하고, Session/Channels/Frames/Samples/Events Table 생성.
        /// 시간 검색용 Index도 생성. -> 원하는 시간 범위 찾는 걸 쉽게 해주는 용도의 인덱스.
        /// </summary>
        /// <param name="connection">SQLite DB 연결</param>
        /// <exception cref="ArgumentNullException">Connection null 예외</exception>
        internal static void CreateTables(SqliteConnection connection)
        {
            if(connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            /// foreign_keys : 외래 키 검사 적용 옵션.
            /// 현재 SQLite 연결에서 외래 키(Foreign Key) 검사 활성화.
            ///
            /// 외래 키는 한 테이블의 값이 다른 테이블에 실제로 존재하는 값을
            /// 참조하도록 제한하는 DB의 관계 검사 기능.
            ///
            /// 이 DB에서 Samples는 다음 두 정보를 참조.
            /// 1. Samples.ChannelId → Channels.ChannelId
            ///    센서 값을 저장하려면 해당 Channel이 Channels에 먼저 등록되어 있어야 함.
            /// 2. Samples.FrameNumber → Frames.FrameNumber
            ///    센서 값을 저장하려면 해당 Frame이 Frames에 먼저 저장되어 있어야 함.
            ///
            /// 검사를 활성화하면 존재하지 않는 Channel이나 Frame을 참조하는
            /// Samples 데이터를 INSERT 또는 UPDATE할 때 SQLite에서 오류 발생.
            /// 현재 외래 키 설정에서는 Samples가 참조 중인 Channel이나 Frame을
            /// 삭제하거나 참조 키를 변경하는 작업도 제한.
            ///
            /// C#의 ValidateFrame()은 메모리의 등록 정보를 검사.
            /// 외래 키 검사는 실제 DB에 저장된 정보를 기준으로 관계를 검사하므로,
            /// C# 검사를 거치지 않고 SQL을 직접 실행하는 경우에도 적용.
            ///
            /// 이 설정은 DB 파일 전체의 영구 설정이 아니라 현재 연결에 적용.
            /// 따라서 데이터를 변경하는 새 연결을 열 때마다 활성화 필요.
            /// 활성 트랜잭션 안에서는 변경이 적용되지 않으므로 트랜잭션 시작 전에 실행.
            using(var command = connection.CreateCommand())
            {
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();
            }

            /// Transaction : 여러 변경사항을 하나의 저장 단위로 묶어주는 기능.
            /// CommandText : SQLite Command가 실행할 SQL 명령 문자열.
            /// ExecuteNonQuery : SQL을 실행하지만, 조회 결과의 행 목록은 읽지 않는 실행.
            using(var transaction = connection.BeginTransaction())
            {
                using(var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = createTableContext;
                    command.ExecuteNonQuery();
                }

                /// 이번 Transaction에서 실행한 변경 사항을 최종적으로 확정.
                transaction.Commit();
            }
        }

        /// <summary>
        /// Session & Channel 정보를 하나의 Transaction으로 저장하는 함수.
        /// Channel 등록 중에 실패하는 경우, Session만 생성되고 저장되는 상황 방지용.
        /// </summary>
        /// <param name="session">저장할 Session</param>
        /// <param name="channels">저장할 Channel 리스트</param>
        private void InsertSessionAndChannels(TelemetrySession session, IReadOnlyList<TelemetryChannel> channels)
        {
            using(var transaction = connection.BeginTransaction())
            {
                using(var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO Session (
                            Id,
                            SessionId,
                            BuildFileName,
                            StartUtcTicks,
                            SchemaVersion
                        )
                        VALUES (
                            1,
                            $sessionId,
                            $buildFileName,
                            $startUtcTicks,
                            $schemaVersion
                        );";

                    command.Parameters.AddWithValue("$sessionId", session.SessionId.ToString("D"));
                    command.Parameters.AddWithValue("$buildFileName", session.BuildFileName);
                    command.Parameters.AddWithValue("$startUtcTicks", session.StartDate.UtcTicks);
                    command.Parameters.AddWithValue("$schemaVersion", 1);
                    command.ExecuteNonQuery();
                }

                using(var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO Channels (
                            ChannelId,
                            Name,
                            Unit,
                            ValueType,
                            DecimalPlaces
                        )
                        VALUES (
                            $channelId,
                            $name,
                            $unit,
                            $valueType,
                            $decimalPlaces
                        );";

                    var idParameter = command.Parameters.Add("$channelId", SqliteType.Integer);
                    var nameParameter = command.Parameters.Add("$name", SqliteType.Text);
                    var unitParameter = command.Parameters.Add("$unit", SqliteType.Text);
                    var typeParameter = command.Parameters.Add("$valueType", SqliteType.Integer);
                    var placesParameter = command.Parameters.Add("$decimalPlaces", SqliteType.Integer);

                    foreach(TelemetryChannel channel in channels)
                    {
                        idParameter.Value = channel.ChannelId;
                        nameParameter.Value = channel.Name;
                        unitParameter.Value = channel.Unit;
                        typeParameter.Value = (int)channel.ValueType;
                        placesParameter.Value = channel.DecimalPlaces;

                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// DB 연결 해제.
        /// </summary>
        public void Dispose()
        {
            if(disposed)
                return;

            connection.Dispose();
            disposed = true;
        }

        /// <summary>
        /// 수집된 데이터 Set의 시간/순번을 frame에 넣고, 각 Channel별 데이터를 Samples에 저장.
        /// 전부 성공해야 하나의 DataSet을 저장.
        /// </summary>
        /// <param name="frame">저장할 Frame</param>
        /// <param name="channels">저장할 Channel Dictionary</param>
        /// <exception cref="ObjectDisposedException">SqliteTelemetryStore가 이미 해제된 경우 예외.</exception>
        /// <exception cref="ArgumentNullException">frame, channels가 null 인 경우 예외.</exception>
        /// <exception cref="ArgumentException">frame의 데이터 수와 channels의 수가 일치하지 않는 경우 예외.</exception>
        internal void WriteFrame(TelemetryFrame frame, IReadOnlyDictionary<int, TelemetryChannel> channels)
        {
            if(disposed)
            {
                throw new ObjectDisposedException(nameof(SqliteTelemetryStore));
            }

            if(frame is null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            if(channels is null)
            {
                throw new ArgumentNullException(nameof(channels));
            }

            if(frame.Data.Count != channels.Count)
            {
                throw new ArgumentException("Frame must contain one data item for every registered Channel.", nameof(frame));
            }

            using(var transaction = connection.BeginTransaction())
            {
                using(var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO Frames (
                            FrameNumber,
                            UtcTicks,
                            ElapsedTicks
                        )
                        VALUES (
                            $frameNumber,
                            $utcTicks,
                            $elapsedTicks
                        );";

                    command.Parameters.AddWithValue("$frameNumber", frame.FrameNumber);
                    command.Parameters.AddWithValue("$utcTicks", frame.Timestamp.Date.UtcDateTime.Ticks);
                    command.Parameters.AddWithValue("$elapsedTicks", frame.Timestamp.Elapsed.Ticks);
                    command.ExecuteNonQuery();
                }

                using(var command = connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = @"
                        INSERT INTO Samples (
                            FrameNumber,
                            ChannelId,
                            Value,
                            Status,
                            SourceUtcTicks
                        )
                        VALUES (
                            $frameNumber,
                            $channelId,
                            $value,
                            $status,
                            $sourceUtcTicks
                        );";

                    var framePar = command.Parameters.Add("$frameNumber", SqliteType.Integer);
                    var channelPar = command.Parameters.Add("$channelId", SqliteType.Integer);
                    var valuePar = command.Parameters.Add("$value", SqliteType.Integer);
                    var statusPar = command.Parameters.Add("$status", SqliteType.Integer);
                    var sourcePar = command.Parameters.Add("$sourceUtcTicks", SqliteType.Integer);

                    framePar.Value = frame.FrameNumber;

                    foreach(TelemetryData data in frame.Data)
                    {
                        TelemetryChannel channel;

                        if(!channels.TryGetValue(data.ChannelId, out channel))
                        {
                            throw new ArgumentException($"Unregistered Channel ID : {data.ChannelId}", nameof(frame));
                        }

                        channelPar.Value = data.ChannelId;

                        valuePar.Value = ConvertStorageValue(channel, data);
                        statusPar.Value = (int)data.Status;

                        sourcePar.Value = data.SourceDate.HasValue
                        ? (object)data.SourceDate.Value.UtcDateTime.Ticks : DBNull.Value;

                        command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// 장비 사용 중 발생한 이벤트 정보를 Events Table에 저장.
        /// EventId는 DB가 부여함.
        /// </summary>
        /// <param name="tEvent">저장할 TelemetryEvent 객체</param>
        /// <exception cref="ObjectDisposedException">SqliteTelemetryStore가 이미 해제된 경우 예외.</exception>
        /// <exception cref="ArgumentNullException">tEvent가 null인 경우 예외.</exception>
        internal void WriteEvent(TelemetryEvent tEvent)
        {
            if(disposed)
            {
                throw new ObjectDisposedException(nameof(SqliteTelemetryStore));
            }

            if(tEvent is null)
            {
                throw new ArgumentNullException(nameof(tEvent));
            }

            using(var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    INSERT INTO Events (
                        UtcTicks,
                        ElapsedTicks,
                        EventCode,
                        Source,
                        OperationId,
                        Message
                    )
                    VALUES (
                        $utcTicks,
                        $elapsedTicks,
                        $eventCode,
                        $source,
                        $operationId,
                        $message
                    );";

                command.Parameters.AddWithValue("$utcTicks", tEvent.Timestamp.Date.UtcDateTime.Ticks);
                command.Parameters.AddWithValue("$elapsedTicks", tEvent.Timestamp.Elapsed.Ticks);
                command.Parameters.AddWithValue("$eventCode", tEvent.EventCode);
                command.Parameters.AddWithValue("$source", tEvent.Source);
                command.Parameters.AddWithValue("$operationId", tEvent.OperationId);
                command.Parameters.AddWithValue("$message", tEvent.Message);

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 기존 Session 행에 종료 시각, 경과 시간, 종료 사유를 추가.
        /// </summary>
        /// <param name="sessionId">종료할 Session ID</param>
        /// <param name="timestamp">종료 시각과 경과 시간 정보</param>
        /// <param name="reason">종료 사유</param>
        /// <exception cref="ObjectDisposedException">SqliteTelemetryStore가 이미 해제된 경우 예외.</exception>
        /// <exception cref="ArgumentException">sessionId가 비어 있는 경우 예외.</exception>
        /// <exception cref="ArgumentNullException">timestamp가 null인 경우 예외.</exception>
        /// <exception cref="ArgumentOutOfRangeException">reason이 유효하지 않은 경우 예외.</exception>
        /// <exception cref="InvalidOperationException">Session이 존재하지 않거나 이미 종료된 경우 예외.</exception>
        internal void CompleteSession(Guid sessionId, TelemetryTimestamp timestamp, TelemetryEndReason reason)
        {
            if(disposed)
            {
                throw new ObjectDisposedException(nameof(SqliteTelemetryStore));
            }

            if(sessionId == Guid.Empty)
            {
                throw new ArgumentException("Session ID cannot be empty", nameof(sessionId));
            }

            if(timestamp is null)
            {
                throw new ArgumentNullException(nameof(timestamp));
            }

            if(!Enum.IsDefined(typeof(TelemetryEndReason), reason))
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }

            using(var command = connection.CreateCommand())
            {
                command.CommandText = @"
                    UPDATE Session
                    SET EndUtcTicks     = $endUtcTicks,
                        EndElapsedTicks = $endElapsedTicks,
                        EndReason       = $endReason
                    WHERE Id = 1
                      AND SessionId = $sessionId
                      AND EndUtcTicks IS NULL;";

                command.Parameters.AddWithValue("$endUtcTicks", timestamp.Date.UtcDateTime.Ticks);
                command.Parameters.AddWithValue("$endElapsedTicks", timestamp.Elapsed.Ticks);
                command.Parameters.AddWithValue("$endReason", reason.ToString());
                command.Parameters.AddWithValue("$sessionId", sessionId.ToString("D"));

                int affectedRows = command.ExecuteNonQuery();

                if(affectedRows != 1)
                {
                    throw new InvalidOperationException("The Session was not found or has already ended");
                }
            }
        }


        
        /// <summary>
        /// 측정된 데이터값을 DB에 저장 가능한 형태로 변환하는 기능.
        /// C# 데이터형식 -> SQLite 데이터형식 변환.
        /// </summary>
        /// <param name="channel">변환 전 Channel 데이터</param>
        /// <param name="data">변환 전 Data</param>
        /// <returns>SQLite에 저장 가능한 형태의 값</returns>
        /// <exception cref="ArgumentNullException">channel 또는 data가 null인 경우</exception>
        /// <exception cref="ArgumentException">channel과 data의 ID 또는 타입이 일치하지 않는 경우</exception>
        /// <exception cref="ArgumentOutOfRangeException">지원되지 않는 Channel 값 타입인 경우</exception>
        private static object ConvertStorageValue(TelemetryChannel channel, TelemetryData data)
        {
            if(channel is null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            if(data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if(channel.ChannelId != data.ChannelId)
            {
                throw new ArgumentException("Channel ID does not match.", nameof(data));
            }

            if(channel.ValueType != data.ValueType)
            {
                throw new ArgumentException("Data type does not match.", nameof(data));
            }

            if(data.Status == TelemetryDataStatus.Missing || data.Status == TelemetryDataStatus.Error)
            {
                return DBNull.Value;
            }

            switch(channel.ValueType)
            {
                case TelemetryValueType.Int:
                    return data.IntValue.Value;
                case TelemetryValueType.Boolean:
                    return data.BoolValue.Value ? 1L : 0L;
                case TelemetryValueType.Real:
                    {
                        double scale = Math.Pow(10.0, channel.DecimalPlaces);
                        double scaledValue = data.RealValue.Value * scale;

                        double roundedValue = Math.Round(scaledValue, MidpointRounding.AwayFromZero);

                        return checked((long)roundedValue);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(channel), "Unsupported Channel value type.");

            }
        }
    }
}
