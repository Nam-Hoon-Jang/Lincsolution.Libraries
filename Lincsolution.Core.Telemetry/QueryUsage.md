# Telemetry 조회 및 Plot

## Core 조회 API

`TelemetryQueryReader`는 UI 의존성이 없는 .NET Standard 2.0 읽기 전용 API다.
DB 생성/수정은 하지 않으며, 조회마다 연결을 열고 닫는다.

```csharp
var reader = new TelemetryQueryReader(databasePath);
var info = reader.ReadSession();
var channels = reader.ReadChannels();

// 원본 센서값: 시간 범위 양 끝 포함, 최대 10,000행/호출.
var samples = reader.ReadSamples(
    TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20),
    channelId: 1, offset: 0, limit: 200);

var events = reader.ReadEvents(
    TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20));

// Plot용: 선택 채널만 읽고 채널당 표시 점수를 제한한다.
var plotData = await Task.Run(() => reader.ReadPlot(
    new[] { 1, 2, 3 }, TimeSpan.Zero, info.LastElapsed,
    maxPointsPerChannel: 1200, cancellationToken: token), token);
```

`StoredValue`는 DB의 정확한 정수 원본이다. 실수 채널은 채널의 `DecimalPlaces`에 따라 복원한다.
`Value`는 Plot에 쓰기 쉬운 double 값이며 큰 정수의 마지막 자릿수까지 보장하지 않는다.
`Missing`/`Error`는 Plot 값이 null이며 선을 끊는다. `Stale`은 이전 값이므로 선에 포함된다.

## 표시량 제한

- `ReadPlot`은 최대 100개 채널, 채널당 12~6,000점을 반환한다.
- 원본이 한도 이하이면 모든 원본 점을 반환한다.
- 초과하면 시간을 일정 구간으로 나누고 각 구간의 첫/마지막/최소/최대/첫 누락/마지막 누락을 남긴다.
- 원본을 전부 리스트로 만들지 않고 순서대로 읽는다. 메모리는 선택 채널과 점수 한도에 의해 제한된다.
- 원본을 읽는 시간 자체는 데이터량에 비례한다. UI에서는 작업 스레드와 취소 토큰을 사용한다.
- 요약 데이터는 모든 상태 전환/짧은 누락 구간을 그대로 보존하지 않는다. 자세히 보려면 시간 범위를 좁힌다.
- `SourceCount`, `Points.Count`, `IsReduced`로 축약 여부를 확인한다.
- 수집 중 파일도 읽을 수 있지만 여러 API/채널 조회 전체가 단일 시점의 스냅샷은 아니다. 확정 비교에는 종료한 DB를 사용한다.
- 취소는 조회 전과 행 처리 사이에서 확인한다. SQLite 잠금 대기는 최대 2초로 설정되어 있다.

## 재사용 컨트롤

컨트롤: `Lincsolution.UI.Controls.TimeSeriesPlot`

소스: `Lincsolution.UI.Controls/Grid/TimeSeriesPlot.cs`

스타일: `Lincsolution.UI.Controls/Styles/TimeSeriesPlot.xaml` (Generic.xaml에서 병합)

```xml
<controls:TimeSeriesPlot
    Series="{Binding PlotSeries}"
    Title="센서 비교"
    RangeStart="0" RangeEnd="60"
    LineThickness="1.5"
    ShowGrid="True"
    SelectorWidth="220"
    NormalizeSeries="False"/>
```

`Series`에는 UI 계층의 `PlotSeries` 목록을 전달한다. 이 컨트롤은 DB와 Core.Telemetry를 참조하지 않는다.
`PlotSeries.Points`는 시간순 `PlotPoint` 목록이며, 갱신할 때 목록을 교체한다.
센서 색은 생성자의 Brush로 지정한다. Boolean/상태 값은 `isStep: true`로 계단형 표시한다.
체크박스는 `PlotSeries.IsVisible`과 연결된다. 데이터 재조회는 애플리케이션이 이 변경을 받아 처리한다.

커스텀 DP는 `[DependencyProperty]`로 생성하고 한국어/영어 설명을 속성 창에 전달한다.
변경 가능한 요소: 제목, 시간 범위, Y축 범위(NaN=자동), 정규화, 선 두께, 격자 표시/색,
축 색, Plot 배경, 축 여백, 선택 영역 폭/표시/항목 템플릿, 모서리, 강조 테두리, 비활성 불투명도, 빈 화면 문구.
일반 Control의 Background/Foreground/Font/Padding/Border 속성도 지원한다.

정규화는 조회 구간의 센서별 최소/최대를 0~1로 표시한다. 상수 센서는 0.5로 표시한다.
정규화하지 않으면 서로 다른 단위도 같은 Y축의 원래 숫자로 표시하므로 단위와 크기를 고려한다.
툴팁은 원본 보간값이 아니라 표시용 점 중 커서에 가장 가까운 값을 최대 8개 센서까지 보여준다.

컨트롤도 채널당 6,000점/전체 120,000점/선택 100개 제한을 검사한다.
초과 입력은 조용히 일부를 생략하지 않고 한도 초과 안내를 표시한다.

## LibraryTester

`Dashboard → Display → Plot`에서 DB를 선택한다. 최초 3개 센서가 자동 선택된다.

1. 오른쪽 센서 체크박스로 비교 대상을 선택한다.
2. 경과 초 범위 입력 후 `범위 적용`을 누른다. `전체 시간`으로 복원한다.
3. 데이터 범위/단위가 크게 다르면 `센서별 0~1 정규화`를 사용한다.
4. 하단의 원본/표시 점수를 확인한다. 축약된 영역은 범위를 좁혀 자세히 본다.
5. 조건 변경 시 이전 조회가 취소되며, 이전 결과는 새 선택에 반영되지 않는다.

테스트 화면은 조회를 한 번에 하나만 실행하며, 체크 변경을 180ms 모아 불필요한 연속 조회를 줄인다.
Plot 곡선은 데이터/스타일/크기가 바뀔 때만 다시 계산하고 커서 이동에는 기존 그리기를 재사용한다.
