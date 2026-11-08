using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Lincsolution.Core.Telemetry.Models;
using Lincsolution.Core.Telemetry.Storage;

namespace Lincsolution.Core.Telemetry
{
    /// <summary>
    /// Telemetry 데이터 수집 Session과 Record Process 관리
    /// 수집한 데이터를 검사하여 대기열에 접수하고, 백그라운드에서 SQLite에 자동 저장.
    /// </summary>
    public sealed class TelemetryRecorder : IDisposable
    {
        // 잠금 순서는 writeRoot → syncRoot. 접수 함수에서는 syncRoot만 사용.
        // SQLite 작업 중 syncRoot를 해제하므로 디스크가 느려도 새 측정값을 접수할 수 있음.
        private readonly object writeRoot = new object();
        private readonly int autoSaveIntervalMilliseconds;
        private Timer saveTimer;
        private Exception lastStorageError;
        private TelemetryTimestamp endingTimestamp;
        private TelemetryEndReason endingReason;
        private bool disposed;

        /// <summary>
        /// 자동 저장 오류 알림. 백그라운드 스레드에서 호출되므로 UI 변경은 Dispatcher로 전달.
        /// 오류 후 자동 재시도와 신규 접수는 중단. 원인 해결 후 Flush 또는 EndSession으로 재시도.
        /// 이벤트의 SessionId를 확인하여 이전 Session에서 발생한 지연 알림을 구분.
        /// </summary>
        public event EventHandler<TelemetryStorageErrorEventArgs> StorageFailed;

        /// <summary>최근 저장 오류. 정상 저장 재시도 또는 새 Session 시작 시 null.</summary>
        public Exception LastStorageError { get { lock(syncRoot) return lastStorageError; } }

        /// <summary>저장 중인 항목을 포함하여 아직 저장 완료되지 않은 Frame/Event 개수.</summary>
        public int PendingRecordCount { get { lock(syncRoot) return pendingRecords.Count; } }

        /// <summary>기본 100ms 간격으로 자동 저장하는 Recorder 생성.</summary>
        public TelemetryRecorder() : this(100) { }

        /// <summary>자동 저장 간격 설정. 센서 수집 Timer는 애플리케이션에서 별도로 실행.</summary>
        /// <param name="autoSaveIntervalMilliseconds">자동 저장 간격(ms). 기본 100ms, 양수만 허용.</param>
        public TelemetryRecorder(int autoSaveIntervalMilliseconds)
        {
            if(autoSaveIntervalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(autoSaveIntervalMilliseconds));
            this.autoSaveIntervalMilliseconds = autoSaveIntervalMilliseconds;
        }

        /// <summary>현재 Session의 SQLite 저장소.</summary>
        private SqliteTelemetryStore store;

        /// <summary>Session 상태와 시간 정보 중복 접근 방지용 lock object</summary>
        private readonly object syncRoot = new object();

        /// <summary>Session 시작 이후 경과 시간 측정용</summary>
        private readonly Stopwatch sessionStopwatch = new Stopwatch();

        /// <summary>현재의 데이터 수집 Session. 수집 중이 아니면 null</summary>
        private TelemetrySession currentSession;

        /// <summary>currentSession에 등록한 Channel 목록. Channel ID로 검색.</summary>
        private Dictionary<int, TelemetryChannel> registeredChannels = new Dictionary<int, TelemetryChannel>();

        /// <summary>저장 대기 중인 Frame과 Event 목록</summary>
        private readonly Queue<object> pendingRecords = new Queue<object>();

        /// <summary>저장 큐의 최대 항목 수</summary>
        private const int MaxPendingRecords = 4096;

        /// <summary>마지막으로 접수한 Frame 번호</summary>
        private long lastFrameNumber = -1;

        /// <summary>마지막으로 접수한 Frame의 경과 시간</summary>
        private TimeSpan lastFrameElapsed = TimeSpan.Zero;

        /// <summary>신규 데이터를 접수할 수 있는지 확인. 종료 중이거나 저장 오류가 있으면 false.</summary>
        public bool IsRecording
        {
            get
            {
                lock(syncRoot)
                {
                    return currentSession != null && endingTimestamp is null && lastStorageError is null;
                }
            }
        }

        /// <summary>
        /// Session 내부의 Channel 목록을 등록하면서, Session을 새로 시작하는 함수.
        /// 파일 이름, 채널 목록 검사 후, 새 Session ID를 생성하고, 현재 Session으로 설정.
        /// 채널 목록을 보관하고, 경과 시간 측정을 시작.
        /// </summary>
        /// <param name="dbPath">데이터베이스 생성 경로. 확장자명 포함</param>
        /// <param name="buildFileName">확장자를 포함한 파일 이름</param>
        /// <param name="channels">이번 Session에서 수집할 Channel 목록</param>
        /// <returns>새로 생성한 Session</returns>
        /// <exception cref="InvalidOperationException">이미 Session이 데이터 수집 중인 경우.</exception>
        /// <exception cref="ArgumentNullException">Channel 목록이 null인 경우.</exception>
        /// <exception cref="ArgumentException">파일 이름이 비어 있거나, Channel 목록이 잘못된 경우.</exception>
        public TelemetrySession StartSession(string dbPath, string buildFileName, IEnumerable<TelemetryChannel> channels)
        {
            lock(writeRoot)
            lock(syncRoot)
            {
                if(disposed) throw new ObjectDisposedException(nameof(TelemetryRecorder));
                if(string.IsNullOrWhiteSpace(dbPath))
                {
                    throw new ArgumentException("Database path cannot be empty.", nameof(dbPath));
                }
                if(string.IsNullOrWhiteSpace(buildFileName))
                {
                    throw new ArgumentException("Build File Name must have a value.", nameof(buildFileName));
                }
                if(currentSession != null)
                {
                    throw new InvalidOperationException("Telemetry Session is already running.");
                }

                if(channels is null)
                {
                    throw new ArgumentNullException(nameof(channels));
                }

                /// 등록이 실패해도 현재 상태에 영향 주지 않도록, 함수 내부에서 별도 구성.
                var newChannels = new Dictionary<int, TelemetryChannel>();

                foreach(TelemetryChannel channel in channels)
                {
                    if(channel is null)
                    {
                        throw new ArgumentException("Channels must not contain null items.", nameof(channels));
                    }
                    /// 중복된 Channel ID 검사
                    if(newChannels.ContainsKey(channel.ChannelId))
                    {
                        throw new ArgumentException($"Duplicate Channel ID : {channel.ChannelId}", nameof(channels));
                    }
                    /// Channel 추가.
                    newChannels.Add(channel.ChannelId, channel);
                }
                /// 채널 갯수는 최소 1개 이상이어야 함.
                if(newChannels.Count == 0)
                {
                    throw new ArgumentException("At least one Channel is required", nameof(channels));
                }

                var session = new TelemetrySession(Guid.NewGuid(), buildFileName, DateTimeOffset.UtcNow);

                // UTC 시작 시각과 경과 시간의 기준을 맞춤. DB 준비 시간도 포함.
                sessionStopwatch.Restart();
                SqliteTelemetryStore newStore;
                Timer newTimer = null;

                try
                {
                    /// 새 DB, Table 생성. Session과 Channel 정보 저장.
                    // 먼저 비활성 Timer를 준비. DB 준비 실패 시 Timer도 해제.
                    newTimer = new Timer(SaveAutomatically, session.SessionId,
                        Timeout.Infinite, Timeout.Infinite);
                    newStore = SqliteTelemetryStore.CreateNew(dbPath, session, newChannels.Values);
                }
                catch
                {
                    /// 새 DB 생성 실패 시, 기록 중 상태로 전환하지 않음.
                    newTimer?.Dispose();
                    sessionStopwatch.Reset();
                    throw;
                }

                store = newStore;
                registeredChannels = newChannels;
                lastFrameNumber = -1;
                lastFrameElapsed = TimeSpan.Zero;
                currentSession = session;
                lastStorageError = null;
                endingTimestamp = null;
                saveTimer = newTimer;
                saveTimer.Change(autoSaveIntervalMilliseconds, autoSaveIntervalMilliseconds);

                return session;
            }
        }

        /// <summary>
        /// 현재 시점의 공통 TelemetryTimestamp 정보 생성하는 함수
        /// </summary>
        /// <returns>Telemetry 내부적으로 공유하기 위한 시간 정보</returns>
        /// <exception cref="InvalidOperationException">수집 중인 Session이 없는 경우</exception>
        public TelemetryTimestamp CaptureTimestamp()
        {
            lock(syncRoot)
            {
                if(currentSession is null)
                {
                    throw new InvalidOperationException("There is no Telemetry Session running.");
                }

                TimeSpan elapsed = sessionStopwatch.Elapsed;
                DateTimeOffset date = DateTimeOffset.UtcNow;

                return new TelemetryTimestamp(date, elapsed);
            }
        }

        /// <summary>
        /// 접수를 중단하고 남은 데이터를 저장한 뒤 종료 정보를 기록하고 연결을 해제.
        /// 실패 시 대기 데이터와 최초 종료 시각을 유지. 같은 ID/이유로 다시 호출하여 재시도.
        /// 동기 함수이므로 UI에서는 EndSessionAsync 사용 권장.
        /// </summary>
        /// <param name="sessionId">종료할 Session ID.</param>
        /// <param name="reason">정상 종료, 취소 또는 실패.</param>
        /// <returns>최초 종료 요청 시 확보한 시간 정보.</returns>
        public TelemetryTimestamp EndSession(Guid sessionId, TelemetryEndReason reason)
        {
            // 접수 잠금을 잡은 채 저장 작업의 종료를 기다리지 않음.
            // 먼저 새 접수를 막고 종료 시각을 확정한 뒤 진행 중인 DB 쓰기를 기다림.
            lock(syncRoot)
            {
                ValidateSession(sessionId);
                if(!Enum.IsDefined(typeof(TelemetryEndReason), reason))
                    throw new ArgumentOutOfRangeException(nameof(reason));
                if(endingTimestamp != null && endingReason != reason)
                    throw new InvalidOperationException("Retry with the original end reason.");
                if(endingTimestamp is null)
                {
                    endingTimestamp = new TelemetryTimestamp(DateTimeOffset.UtcNow, sessionStopwatch.Elapsed);
                    endingReason = reason;
                    sessionStopwatch.Stop();
                    saveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }

            lock(writeRoot)
            {
                lock(syncRoot) ValidateSession(sessionId);
                try
                {
                    DrainPendingRecords();
                    store.CompleteSession(sessionId, endingTimestamp, endingReason);
                }
                catch(Exception error)
                {
                    // 종료 실패를 정상 종료로 처리하거나 데이터를 버리지 않음.
                    lock(syncRoot) lastStorageError = error;
                    throw;
                }

                var timestamp = endingTimestamp;
                try { store.Dispose(); }
                finally
                {
                    lock(syncRoot)
                    {
                        saveTimer.Dispose();
                        saveTimer = null;
                        store = null;
                        currentSession = null;
                        registeredChannels.Clear();
                        sessionStopwatch.Reset();
                        endingTimestamp = null;
                        lastStorageError = null;
                    }
                }
                return timestamp;
            }
        }

        /// <summary>
        /// UI 스레드를 막지 않고 종료 저장을 기다림. 수집 작업을 먼저 중단한 후 await 사용.
        /// 저장 실패는 반환 Task의 예외로 전달되며, 같은 ID/이유로 재시도 가능.
        /// </summary>
        public Task<TelemetryTimestamp> EndSessionAsync(Guid sessionId, TelemetryEndReason reason)
        {
            return Task.Run(() => EndSession(sessionId, reason));
        }

        /// <summary>
        /// 센서별 입력 목록으로 Data와 Frame을 생성하여 접수. Frame 번호는 마지막 접수 번호 + 1.
        /// 직접 TryRecordFrame을 호출한 경우에도 그 번호 다음부터 이어짐.
        /// 채널 누락/중복/자료형/값 상태는 기존 모델과 동일한 규칙으로 검사.
        /// </summary>
        /// <param name="sessionId">현재 Session ID.</param>
        /// <param name="readings">모든 등록 채널의 수집 결과. 누락 센서도 Missing 상태로 포함.</param>
        /// <param name="timestamp">수집 묶음의 실제 시각. null이면 접수 시각 사용.</param>
        /// <returns>대기열에 접수했으면 true. 포화이면 false이며 번호를 소비하지 않음.</returns>
        public bool TryRecordReadings(Guid sessionId, IEnumerable<TelemetryReading> readings,
            TelemetryTimestamp timestamp = null)
        {
            lock(syncRoot)
            {
                ValidateCanAccept(sessionId);
                if(readings is null) throw new ArgumentNullException(nameof(readings));
                var data = new List<TelemetryData>();
                foreach(var reading in readings)
                {
                    if(reading is null)
                        throw new ArgumentException("Readings must not contain null items.", nameof(readings));
                    TelemetryChannel channel;
                    if(!registeredChannels.TryGetValue(reading.ChannelId, out channel))
                        throw new ArgumentException("Unregistered Channel ID: " + reading.ChannelId, nameof(readings));
                    data.Add(new TelemetryData(reading.ChannelId, channel.ValueType, reading.Status,
                        reading.IntValue, reading.RealValue, reading.BoolValue, reading.SourceDate));
                }
                var frame = new TelemetryFrame(checked(lastFrameNumber + 1),
                    timestamp ?? CaptureTimestamp(), data);
                // 동일 스레드의 lock 재진입은 허용됨. 검사와 접수가 한 번에 이루어짐.
                return TryRecordFrame(sessionId, frame);
            }
        }

        /// <summary>
        /// Session & Frame 데이터 유효성 검사 후, 저장 대기열(Queue)에 추가하는 함수.
        /// 입력받은 Frame의 Session/Channel/구성/자료형/순번/경과 시간 검사 후 대기열에 추가.
        /// 대기열이 다 차면 false 반환
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="frame">기록할 Frame</param>
        /// <returns>저장 대기열 접수 성공 여부. 대기열 꽉 차면 false.</returns>
        /// <exception cref="ArgumentException">Channel 구성과 Frame 구성 간 데이터가 안 맞는 경우</exception>
        public bool TryRecordFrame(Guid sessionId, TelemetryFrame frame)
        {
            lock(syncRoot)
            {
                /// Session & Frame 유효성 검사
                ValidateCanAccept(sessionId);
                ValidateFrame(frame);
                /// frame 내부 속성 유효 검사.
                if(frame.FrameNumber <= lastFrameNumber)
                {
                    throw new ArgumentException("Frame number must increase.", nameof(frame));
                }

                if(frame.Timestamp.Elapsed < lastFrameElapsed)
                {
                    throw new ArgumentException("Frame elapsed time must not decrease.", nameof(frame));
                }
                /// 큐에 담을 항목 갯수 최대치 도달 여부 검사.
                if(pendingRecords.Count >= MaxPendingRecords)
                {
                    return false;
                }
                /// pendingRecords 큐에 현재 frame 추가
                pendingRecords.Enqueue(frame);

                lastFrameNumber = frame.FrameNumber;
                lastFrameElapsed = frame.Timestamp.Elapsed;

                return true;
            }
        }

        /// <summary>
        /// Event 검사 후, 저장 대기열(Queue)에 추가하는 함수.
        /// Session과 Event 검사 후, Frame 대기열에 추가. 대기열이 다 차면 false 반환
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <param name="tEvent">기록할 Event</param>
        /// <returns>Event 기록 성공 여부</returns>
        /// <exception cref="ArgumentNullException">Event가 null인 경우</exception>
        public bool TryRecordEvent(Guid sessionId, TelemetryEvent tEvent)
        {
            lock(syncRoot)
            {
                /// Session 유효성 검사
                ValidateCanAccept(sessionId);

                if(tEvent is null)
                {
                    throw new ArgumentNullException(nameof(tEvent));
                }
                /// 큐에 담을 항목 갯수 최대치 도달 여부 검사.
                if(pendingRecords.Count >= MaxPendingRecords)
                {
                    return false;
                }
                /// pendingRecords 큐에 현재 event 추가
                pendingRecords.Enqueue(tEvent);

                return true;
            }
        }

        /// <summary>
        /// 호출 시점의 대기 항목을 동기 저장. 자동 저장과 같은 쓰기 잠금을 사용하여 중복 저장 방지.
        /// 실패 항목은 대기열에 유지하고 예외 전달. 성공하면 저장 오류 상태를 해제하여 자동 저장 재개.
        /// 저장 중 새로 접수한 항목은 다음 저장 주기에서 처리할 수 있음.
        /// </summary>
        /// <param name="sessionId">현재 Session ID.</param>
        /// <returns>이번 호출에서 저장한 Frame/Event 수. 자동 저장이 먼저 처리했다면 0.</returns>
        public int Flush(Guid sessionId)
        {
            lock(writeRoot)
            {
                lock(syncRoot) ValidateSession(sessionId);
                try
                {
                    int count = DrainPendingRecords();
                    lock(syncRoot) lastStorageError = null;
                    return count;
                }
                catch(Exception error)
                {
                    lock(syncRoot) lastStorageError = error;
                    throw;
                }
            }
        }

        /// <summary>
        /// writeRoot 내부에서만 호출. 대기열 잠금은 확인/제거 순간에만 사용하고 SQLite I/O 중에는 해제.
        /// 시작 시 항목 수만 처리하므로 계속 접수되어도 한 번의 저장 작업이 무한히 길어지지 않음.
        /// </summary>
        private int DrainPendingRecords()
        {
            int count;
            lock(syncRoot) count = pendingRecords.Count;
            for(int i = 0; i < count; i++)
            {
                object record;
                lock(syncRoot) record = pendingRecords.Peek();
                if(record is TelemetryFrame frame)
                    store.WriteFrame(frame, registeredChannels);
                else if(record is TelemetryEvent tEvent)
                    store.WriteEvent(tEvent);
                else
                    throw new InvalidOperationException("Unsupported record type in the pending queue.");
                // DB 저장 성공 전에는 제거하지 않음. 다른 소비자는 writeRoot에서 기다림.
                lock(syncRoot) pendingRecords.Dequeue();
            }
            return count;
        }

        /// <summary>
        /// ThreadPool Timer 콜백. 이전 저장이 진행 중이면 이번 호출을 건너뛰어 콜백 누적 방지.
        /// 이전 Session의 예약 콜백은 ID 검사로 무시. 오류 후 무한 자동 재시도하지 않음.
        /// </summary>
        private void SaveAutomatically(object state)
        {
            Guid sessionId = (Guid)state;
            Exception failure = null;
            if(!Monitor.TryEnter(writeRoot)) return;
            try
            {
                lock(syncRoot)
                {
                    if(currentSession is null || currentSession.SessionId != sessionId ||
                        endingTimestamp != null || lastStorageError != null || disposed) return;
                }
                try { DrainPendingRecords(); }
                catch(Exception error)
                {
                    lock(syncRoot) lastStorageError = error;
                    failure = error;
                }
            }
            finally { Monitor.Exit(writeRoot); }

            // 외부 이벤트 코드는 잠금 밖에서 실행. 핸들러가 Flush/EndSession을 호출해도 교착 없음.
            if(failure != null)
            {
                var handler = StorageFailed;
                if(handler != null)
                {
                    var args = new TelemetryStorageErrorEventArgs(sessionId, failure);
                    foreach(EventHandler<TelemetryStorageErrorEventArgs> subscriber in handler.GetInvocationList())
                    {
                        try { subscriber(this, args); }
                        catch(Exception error)
                        {
                            // 사용자 알림 핸들러의 예외가 Timer 스레드 밖으로 유출되지 않도록 격리.
                            // 원래 저장 오류는 LastStorageError에서 계속 확인 가능.
                            Trace.TraceError("Telemetry StorageFailed handler: {0}", error);
                        }
                    }
                }
            }
        }

        /// <summary>접수 전 상태 검사. 오류 후에는 원인 해결 및 명시적 Flush 재시도가 필요.</summary>
        private void ValidateCanAccept(Guid sessionId)
        {
            ValidateSession(sessionId);
            if(endingTimestamp != null)
                throw new InvalidOperationException("Telemetry Session is ending.");
            if(lastStorageError != null)
                throw new InvalidOperationException("Telemetry storage failed. Retry Flush after resolving the error.", lastStorageError);
        }

        /// <summary>
        /// 사용 완료 시 Timer와 DB 연결 해제. 진행 중 Session은 Cancelled로 저장 후 종료.
        /// 저장 실패 시 예외를 전달하고 데이터를 보존하므로 원인을 해결한 뒤 재시도해야 함.
        /// </summary>
        public void Dispose()
        {
            lock(writeRoot)
            {
                Guid? id;
                TelemetryEndReason reason;
                lock(syncRoot)
                {
                    if(disposed) return;
                    id = currentSession?.SessionId;
                    reason = endingTimestamp is null ? TelemetryEndReason.Cancelled : endingReason;
                }
                if(id.HasValue) EndSession(id.Value, reason);
                lock(syncRoot) disposed = true;
            }
        }

        /// <summary>
        /// 현재 Session 유효성 검사하는 함수.
        /// 현재 Session null 검사 및 Session ID 일치 여부 검사.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <exception cref="InvalidOperationException"></exception>
        private void ValidateSession(Guid sessionId)
        {
            if(disposed) throw new ObjectDisposedException(nameof(TelemetryRecorder));
            if(currentSession is null)
            {
                throw new InvalidOperationException("There is no Telemetry Session running.");
            }

            if(currentSession.SessionId != sessionId)
            {
                throw new InvalidOperationException("The sessionId doesn't match Current Session ID.");
            }
        }

        /// <summary>
        /// 등록한 Channel 정보와 Frame 일치 여부 검사.
        /// Frame 안에 등록된 Channel 마다 데이터가 있는지 검사.
        /// 미등록 Channel 또는 TelemetryValueType에 없는 자료형이 있는지 검사.
        /// 
        /// ※사용 시, 반드시 lock(syncRoot) 내부에서 호출※
        /// 여러 곳에서 ValidateFrame 호출 시, lock(syncRoot) 내부에서 호출해야,
        /// 중복 호출로 발생하는 동기화 문제 예방 가능.
        /// </summary>
        /// <param name="frame">검사할 Frame</param>
        /// <exception cref="InvalidOperationException">수집 중인 Session이 없는 경우</exception>
        /// <exception cref="ArgumentNullException">Frame이 null인 경우</exception>
        /// <exception cref="ArgumentException">Channel 갯수, 등록 여부, 자료형이 일치하지 않는 경우</exception>
        private void ValidateFrame(TelemetryFrame frame)
        {
            if(currentSession is null)
            {
                throw new InvalidOperationException("There is no Telemetry Session running.");
            }

            if(frame is null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            /// 등록된 채널 갯수와 프레임 데이터 갯수가 다른 경우
            if(frame.Data.Count != registeredChannels.Count)
            {
                throw new ArgumentException("Frame must contain one data item for every registered channel.", nameof(frame));
            }

            /// registeredChannels 사전 안에 frame에 등록된 data의 ChannelId 정보 있는지 검사.
            /// 저장된 data의 값 형식과 channel 에 저장된 값 형식 비교.
            foreach(TelemetryData data in frame.Data)
            {
                TelemetryChannel channel;

                if(!registeredChannels.TryGetValue(data.ChannelId, out channel))
                {
                    throw new ArgumentException($"Unregistered Channel ID : {data.ChannelId}", nameof(frame));
                }

                if(data.ValueType != channel.ValueType)
                {
                    throw new ArgumentException($"Data type doesn't match Channel ID : {data.ChannelId}", nameof(frame));
                }
            }

        }
    }
}
