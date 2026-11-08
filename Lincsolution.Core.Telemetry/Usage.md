# Telemetry 수집 및 자동 저장

`TelemetryRecorder`는 센서를 직접 읽지 않는다. 애플리케이션에서 수집한 값을 접수하고 SQLite 파일에 저장한다.
기본 자동 저장 간격은 100ms이며, 센서 수집 주기와는 별개다.

## 기본 사용

```csharp
var recorder = new TelemetryRecorder(autoSaveIntervalMilliseconds: 100);

// 백그라운드 스레드 알림. WPF 화면 갱신은 Dispatcher.BeginInvoke 등으로 전달한다.
recorder.StorageFailed += (sender, e) =>
{
    // e.SessionId와 e.Exception을 로그/오류 상태에 반영한다.
    // 원인을 해결하지 않은 채 여기서 무한 재시도하지 않는다.
};

var session = recorder.StartSession(databasePath, "sample.job", new[]
{
    new TelemetryChannel(1, "온도", "℃", TelemetryValueType.Real, 3),
    new TelemetryChannel(2, "카운터", "", TelemetryValueType.Int, 0),
    new TelemetryChannel(3, "도어", "", TelemetryValueType.Boolean, 0)
});

// 애플리케이션의 수집 주기마다 실행한다.
// 센서값 묶음을 확보한 시점에 시간을 붙이고, 전달이 늦어져도 그 시간을 유지한다.
var timestamp = recorder.CaptureTimestamp();
var readings = new[]
{
    new TelemetryReading(1, realValue: 24.1234),
    new TelemetryReading(2, intValue: 123L),
    new TelemetryReading(3, boolValue: true)
};
bool accepted = recorder.TryRecordReadings(session.SessionId, readings, timestamp);
if(!accepted)
{
    // 대기열 포화: 이번 묶음은 접수되지 않았다.
    // 호출자가 보관하여 순서대로 재시도하거나, 누락을 명시적으로 보고한다.
}

// 별도 이벤트도 같은 대기열을 통해 자동 저장된다.
recorder.TryRecordEvent(session.SessionId,
    new TelemetryEvent(recorder.CaptureTimestamp(), "BuildCompleted"));

// 수집 Timer와 진행 중 수집 작업을 먼저 중단한 뒤 마지막 저장까지 기다린다.
await recorder.EndSessionAsync(session.SessionId, TelemetryEndReason.Completed);
recorder.Dispose();
```

위 예제의 센서값은 설명용 상수다. 실제 장치의 수집 결과로 바꾼다.
이벤트 접수의 bool 반환값도 Frame과 동일하게 확인해야 한다.

## 입력 규칙

- 등록한 모든 채널을 한 번씩 전달한다. 목록의 순서는 자유이며 채널 ID로 대응한다.
- 정수는 `intValue`, 실수는 `realValue`, Boolean은 `boolValue`에 넣는다.
- `Good`와 `Stale`에는 등록된 자료형에 맞는 값 하나가 필요하다.
- 값이 없으면 채널을 생략하지 않고 `new TelemetryReading(id, status: TelemetryDataStatus.Missing)`로 전달한다.
- `Error`와 `Missing`은 값을 전달하지 않는다. 장치 측정 시각은 선택 인수 `sourceDate`로 전달한다.
- 입력 객체는 불변이며, Recorder가 목록을 Data/Frame으로 복사한다. 접수 후 원본 목록을 바꿔도 저장 데이터는 바뀌지 않는다.
- `timestamp` 생략 시 접수 시각을 사용한다. 실제 수집 시각 비교가 필요하면 명시적으로 전달한다.
- Frame 번호는 0부터 자동 증가한다. 직접 만든 Frame과 섞어도 마지막 접수 번호 다음으로 이어진다.
- 입력 오류는 예외, 대기열 포화는 `false`다. 둘 다 번호를 소비하지 않는다.

## 저장과 오류 처리

- `true`는 메모리 대기열 접수 완료이며 디스크 저장 완료가 아니다.
- 자동 저장, 수동 `Flush`, 종료 저장은 하나의 DB 쓰기 잠금으로 직렬화한다.
- DB 쓰기 중에는 접수용 잠금을 잡지 않는다. 다만 데이터 검사 등 짧은 메모리 작업 사이에는 대기할 수 있다.
- `PendingRecordCount`는 저장 중 항목을 포함한다. 최대 4,096개 Frame/Event이며 센서값 개수가 아니다.
- 자동 저장 실패는 `StorageFailed`와 `LastStorageError`로 확인한다. 실패한 항목은 대기열에 남는다.
- 오류 후에는 자동 재시도를 멈추고 신규 접수에 예외를 던진다. `IsRecording`도 false다.
- 원인을 해결한 뒤 `Flush(sessionId)`를 호출하면 남은 항목을 재시도한다. 성공하면 오류 상태가 해제되고 자동 저장/접수가 재개된다.
- `Flush`는 동기 I/O이므로 UI 스레드에서 직접 실행하지 않는다. 재시도 오류도 호출자에게 전달된다.
- 종료 요청 후에는 접수를 중단한다. 저장 실패 시 최초 종료 시각과 이유를 유지하며, 같은 ID와 이유로 `EndSessionAsync`를 재호출할 수 있다.
- `Dispose`는 열린 세션을 Cancelled로 저장 후 종료한다. 저장 실패는 삼키지 않으며, 객체와 대기열을 보존하므로 원인 해결 후 재시도한다.
- 강제 프로세스 종료나 전원 차단 시 아직 메모리에만 있는 항목은 보존되지 않는다.

## 스레드 주의사항

`StorageFailed`는 잠금 밖에서 호출되며, 다른 스레드에서 새 Session을 시작한 후 이전 오류 알림이 도착할 수 있다.
항상 이벤트의 SessionId를 비교한다. UI 핸들러는 동기 Dispatcher.Invoke 대신 비동기 전달을 사용한다.
핸들러 예외는 다른 구독자와 자동 저장 스레드를 보호하기 위해 Trace로 기록하고 격리한다.
