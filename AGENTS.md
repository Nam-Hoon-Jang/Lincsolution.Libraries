# Lincsolution 공통 라이브러리 개발 규정

## 1. 목적

기존 WPF 프로젝트의 공통 기능을 재사용 가능한 회사 라이브러리로 분리한다.
UI 기능은 `Lincsolution.UI.*`, UI와 무관한 기능은 `Lincsolution.Core.*` 아래에 둔다.

새 코드는 이름이 아니라 실제 책임과 의존성을 기준으로 배치한다.

## 2. 프로젝트 구조

```text
Lincsolution
├─ Core
│  ├─ Base
│  ├─ Devices
│  ├─ Communication
│  └─ Manufacturing
└─ UI
   ├─ Base
   ├─ Resources
   ├─ Basic
   └─ Controls
```

프로젝트와 기본 namespace는 다음 이름을 사용한다.

```text
Lincsolution.Core.Base
Lincsolution.Core.Devices
Lincsolution.Core.Communication
Lincsolution.Core.Manufacturing

Lincsolution.UI.Base
Lincsolution.UI.Resources
Lincsolution.UI.Basic
Lincsolution.UI.Controls
```

테스트 실행 프로젝트는 `LibraryTester`를 사용한다.

솔루션은 다음과 같이 분리한다.

```text
../BaseUI/BaseUI.sln          // 기존 BaseUI 실행 프로젝트 전용
Lincsolution.Libraries.sln    // 공통 라이브러리와 LibraryTester 전용
```

라이브러리 프로젝트를 `BaseUI.sln`에 등록하지 않는다.

## 3. Core 프로젝트 책임

### Lincsolution.Core.Base

특정 UI, 장비, 통신 프로토콜, 제품에 종속되지 않는 공통 기반 코드만 둔다.

```text
Exceptions
Helpers
Extensions
Validation
Results
Serialization
IO
Logging
```

허용 예:

- 공통 Exception과 오류 코드
- 파일 존재 여부, 경로, 디렉터리 관련 공통 함수
- 문자열, 컬렉션, Task 확장 메서드
- JSON/XML 직렬화 기반
- 공통 Result 형식
- 특정 구현에 종속되지 않는 로깅 인터페이스와 로그 모델

금지 예:

- WPF `System.Windows.*` 형식
- 특정 장비 SDK
- OPC, REST, TCP 구현
- 특정 제품의 상태나 설정

`Helper`는 작고 상태가 없는 공통 함수에만 사용한다. 여러 책임을 가진 `CommonHelper`, `BaseFunction` 같은 클래스는 만들지 않는다.

### Lincsolution.Core.Devices

물리 장비 제어와 장비 SDK 연동 코드를 둔다.

```text
Camera
Laser
PLC
MCP
Motion
Control
```

예:

```text
CrevisCamera
JptLaserController
McpController
PidController
```

장비 상태 모델, 장비별 Exception, 장비 SDK Adapter도 해당 장비 영역에 둔다.

### Lincsolution.Core.Communication

외부 시스템과 데이터를 송수신하는 프로토콜 및 Client/Server 구현을 둔다.

```text
Opc
Rest
Tcp
Serial
Messaging
```

예:

```text
OpcClient
OpcServer
RestClient
TcpClient
SerialTransport
```

물리 장비의 동작 규칙은 `Devices`, 전송 방식과 프로토콜은 `Communication`에 둔다.

### Lincsolution.Core.Manufacturing

제조 데이터와 산업용 파일 포맷 처리를 둔다.

```text
ThreeMF
BuildData
Mesh
Slice
```

외부 라이브러리에서 자동 생성된 Wrapper는 직접 수정하지 않고 별도 폴더와 namespace로 격리한다.

## 4. UI 프로젝트 책임

### Lincsolution.UI.Base

WPF/MVVM 화면 구현에서만 사용하는 기반 코드를 둔다.

```text
Models
ViewModels
Navigation
Commands
Interfaces
```

예:

```text
BaseModel
BaseViewModel
ViewModelLoader
NavigationService
INavigationNotice
IModelWithSharedModel
```

WPF 또는 UI 수명주기와 관계없는 코드는 `Core.Base`로 이동한다.

### Lincsolution.UI.Resources

공통 WPF 리소스를 둔다.

```text
Themes
Language
Converters
Font
Colors
Brushes
Geometry
DrawingImage
```

규칙:

- Color → Brush → Style 순서로 병합한다.
- 테마와 언어 ResourceDictionary는 분리한다.
- 런타임 변경 대상은 `DynamicResource`를 사용한다.
- 여러 UI 프로젝트에서 공통 사용하는 WPF Converter만 둔다.
- CustomControl 기본 Style은 두지 않는다.

### Lincsolution.UI.Basic

#### UI.Basic / UI.Mini Shell 설계 규약

`UI.Basic`과 향후 `UI.Mini`는 내부 Control의 모든 속성을 대신 전달하는 거대한 View가 아니며,
빈 Grid만 제공하는 단순 레이아웃도 아니다. 바로 실행 가능한 기본 구성을 제공하되,
소비자가 주요 영역과 표현을 교체할 수 있는 Shell로 설계한다.

기본 책임:

```text
MainShell 또는 MainView
├─ HeaderContent
├─ NavigationContent 또는 NavigationItemsSource
├─ MainContent
├─ RightContent
└─ FooterContent
```

Shell이 직접 소유할 수 있는 의미 기반 속성:

```text
IsPaneOpen
IsPaneCollapsed
ExpandedNavigationWidth
CollapsedNavigationWidth
HeaderHeight
FooterHeight
RightPaneWidth
ContentPadding
NavigationItemsSource
SelectedNavigationItem
```

규칙:

- Header, Navigation, Main, Right, Footer 영역은 `ContentPresenter`, `ItemsControl`, `DataTemplate` 등으로 교체 가능하게 만든다.
- 레이아웃의 `Auto`, `*`, 고정 크기를 지원해야 하는 값은 `GridLength`를 우선 사용한다.
- 애니메이션이 필요한 Navigation 확장/축소 폭은 `double` DependencyProperty를 사용할 수 있다.
- Shell 상태, 영역 크기, 표시 여부, 선택 상태처럼 Shell 자체가 의미를 소유하는 값만 DependencyProperty로 공개한다.
- 내부 `UI.Controls`의 IconSize, Stroke, Margin 같은 표현 속성을 MainView DependencyProperty로 전부 전달하지 않는다.
- 개별 Control 표현은 원래 Control의 DependencyProperty, `Style`, `DataTemplate`, `ControlTemplate`, `UI.Resources`로 조절한다.
- 초보 사용자가 즉시 실행할 수 있도록 기본 Control 조합과 기본 Template을 제공한다.
- 숙련 사용자가 기본 요소를 교체할 수 있도록 Content/Items 슬롯과 Style/Template 확장 지점을 제공한다.
- 구체 제품의 DashboardView, DashboardViewModel, 장비 상태, 업무 Command는 `UI.Basic`에 넣지 않는다.
- ViewModel과 View 연결은 실행 애플리케이션의 `DataTemplate`에서 구성한다.

Navigation/MVVM 연결:

```text
실행 애플리케이션 또는 LibraryTester
├─ DI 등록
├─ ViewModelLoader TestViewModel 매핑
├─ NavigationService 생성
└─ ViewModel → View DataTemplate 등록

UI.Basic / UI.Mini
├─ INavigationService 또는 CurrentViewModel Binding 사용
├─ 현재 화면 표시
├─ Pane 접기/펴기
└─ 화면 영역과 크기 관리
```

- `UI.Basic`은 `ViewModelLoader`나 `NavigationService`를 직접 생성하지 않는다.
- 실행 애플리케이션이 `UI.Base`의 서비스를 구성하고 Shell에 주입하거나 Binding한다.
- Shell은 구체 ViewModel 형식을 알지 않고 `CurrentViewModel`을 표시한다.
- Service Locator 방식으로 전역 DI 컨테이너에 직접 접근하지 않는다.

Basic/Mini 관계:

- 화면 구조와 공개 API가 같고 크기만 다르면 같은 Shell에 `Basic`/`Mini` Style 또는 ControlTemplate을 제공한다.
- 화면 구조, 상호작용, Navigation 방식이 실질적으로 다를 때만 `Lincsolution.UI.Mini` 프로젝트를 분리한다.
- 별도 프로젝트로 분리해도 가능한 한 같은 Shell 속성명과 Navigation 계약을 유지한다.

LibraryTester 역할:

- 모든 기본 Shell과 CustomControl의 실행 가능한 예제를 제공한다.
- 기본 상태, Pane 확장/축소, Content 교체, Style 교체, ViewModel Navigation을 확인한다.
- Word 문서는 영역 구조와 안정적인 공개 속성을 설명하고, 실제 조립 예제는 LibraryTester에 둔다.

공통 화면 골격과 재사용 가능한 WPF View/UserControl을 둔다.

```text
Views
Layouts
Themes
Converters
```

예:

```text
MainView
ShellView
BaseLayoutView
HeaderContentLayoutView
```

제품 업무 로직과 장비 제어 코드는 넣지 않는다.

### Lincsolution.UI.Controls

재사용 가능한 WPF CustomControl과 ControlTemplate을 둔다.

```text
Buttons
Inputs
Panels
Icons
Styles
Themes/Generic.xaml
Converters
```

규칙:

- CustomControl은 `DefaultStyleKeyProperty`를 등록한다.
- 기본 Style은 `Themes/Generic.xaml`에서 로드한다.
- 외부에서 조절할 값은 DependencyProperty로 제공한다.
- Style 전용 XAML은 `Styles`에 둔다.
- 특정 Control에서만 사용하는 Converter만 이 프로젝트에 둔다.

## 5. 참조 방향

허용되는 기본 참조:

```text
Core.Devices ─────────→ Core.Base
Core.Communication ───→ Core.Base
Core.Manufacturing ───→ Core.Base

UI.Base ──────────────→ Core.Base
UI.Resources ────────→ Core.Base  // 필요한 경우만
UI.Controls ─────────→ UI.Resources
UI.Basic ────────────→ UI.Base
UI.Basic ────────────→ UI.Resources
UI.Basic ────────────→ UI.Controls
UI.Mini ─────────────→ UI.Base
UI.Mini ─────────────→ UI.Resources
UI.Mini ─────────────→ UI.Controls

LibraryTester
├─ Core.*
└─ UI.*
```

금지:

```text
Core.* ──→ UI.*
UI.Resources ──→ UI.Basic
UI.Resources ──→ UI.Controls
UI.Controls ──→ UI.Basic
UI.Controls ──→ UI.Mini
```

순환 참조는 허용하지 않는다.

## 6. 분류 판단 순서

새 클래스는 다음 순서로 배치한다.

1. WPF, View, Binding, ResourceDictionary에 종속되는가?
   - 예: `UI.*`
2. 물리 장비나 장비 SDK를 제어하는가?
   - 예: `Core.Devices`
3. 외부 시스템과 프로토콜로 데이터를 송수신하는가?
   - 예: `Core.Communication`
4. 제조 데이터나 산업 파일 포맷을 처리하는가?
   - 예: `Core.Manufacturing`
5. 위 기능과 무관한 공통 기반인가?
   - 예: `Core.Base`
6. 특정 제품에서만 사용하는가?
   - 예: 공통 라이브러리가 아니라 해당 실행 프로젝트

## 7. 명명 규칙

역할이 드러나는 이름을 사용한다.

```text
Control / Controller  장비 또는 동작 제어
Client / Server       통신 끝점
Adapter               외부 SDK 추상화
Provider              값이나 설정 제공
Reader / Writer       데이터 읽기/쓰기
Serializer            직렬화
Validator             검증
Factory               객체 생성
Repository / Store    데이터 저장과 조회
State / Context       상태 보관
Helper                작고 상태 없는 보조 함수
Extensions            확장 메서드
Base*                 실제 상속 기반 클래스
```

지양:

```text
BaseFunction
CommonManager
UtilityManager
GlobalHelper
Etc
Misc
```

## 8. 마이그레이션 규칙

- 한 번에 전체 기능을 이동하지 않는다.
- 기능 단위로 이동하고 `LibraryTester`에서 검증한다.
- 기존 동작을 먼저 보존하고 이름과 구조 개선은 별도 단계로 진행한다.
- 외부 SDK, 네이티브 DLL, 설정 파일 의존성을 함께 기록한다.
- 이동 후 기존 프로젝트에 남은 중복 코드는 검증 완료 뒤 제거한다.
- 각 라이브러리는 단독 빌드 가능해야 한다.
- UI 라이브러리는 Visual Studio 디자인뷰와 런타임을 모두 검증한다.

## 9. 변경 검증

공통 라이브러리 변경 시 최소 검증:

```text
1. 대상 라이브러리 단독 빌드
2. LibraryTester x64 Debug 빌드
3. WPF 디자인뷰 로드
4. LibraryTester 런타임 실행
5. 관련 기능의 정상/오류 경로 확인
```

현재 프로젝트 기준 TargetFramework는 `.NET 8`을 사용하고, WPF 프로젝트는 `net8.0-windows`, `UseWPF=true`, 플랫폼은 `x64`로 통일한다.
## 10. 추가 설계 규약 - 2026-07-07

### UI.Basic Shell 슬롯 / ItemsControl 규약

- `UI.Basic`의 `MainView`는 외부 프로젝트가 내부에 컨트롤을 주입할 수 있도록 의미 있는 슬롯을 공개한다.
- 반복 배치 영역은 `Content`를 여러 번 받는 방식이 아니라 `LeftMenuItems`, `HeaderItems`, `ToolItems` 같은 명시적 컬렉션 프로퍼티를 사용한다.
- `UserControl.Content`와 충돌할 수 있으므로 `<Content>...</Content>`를 반복해서 쓰는 API는 만들지 않는다.
- 일정한 순서로 버튼/컨트롤을 나열하는 영역은 `ItemsControl`을 사용한다.
- 위치 고정이 필요한 영역은 `ItemsControl`보다 `ContentPresenter` 또는 명시적 slot property를 사용한다.
- 외부 사용 예시는 다음 형태를 우선한다.

```xaml
<basic:MainView
    LeftMenuItemWidth="{Binding UiSetting.MenuButtonWidth}"
    LeftMenuItemHeight="{Binding UiSetting.MenuButtonHeight}">

    <basic:MainView.LeftMenuItems>
        <ctrls:AlarmButton/>
        <ctrls:AlarmButton/>
        <ctrls:AlarmButton/>
    </basic:MainView.LeftMenuItems>

</basic:MainView>
```

- 아이템 크기 일괄 제어는 개별 컨트롤을 직접 찾아 수정하는 방식보다 `ItemsControl.ItemContainerStyle` 또는 item template binding을 우선 검토한다.
- 단순 UIElement 컬렉션을 받는 경우에는 `ContentPresenter` container의 `Width`, `Height`를 Shell DP에 Binding해서 적용한다.

### UI 설정 Binding 규약

- 버튼 크기, 아이템 크기, 패널 폭 같은 사용자 설정값은 실행 프로젝트가 소유한다.
- `UI.Basic`은 설정 파일을 직접 읽거나 저장하지 않는다.
- 실행 프로젝트는 JSON 등 설정 파일을 읽고 ViewModel 또는 설정 서비스에 값을 올린다.
- `UI.Basic.MainView`는 `DependencyProperty`만 공개하고, 실행 프로젝트가 그 DP에 Binding한다.
- 유저가 설정 페이지에서 값을 변경하면 다음 흐름으로 실시간 반영한다.

```text
설정 ViewModel Property 변경
→ PropertyChanged 발생
→ MainView DependencyProperty 갱신
→ ItemsControl container 크기 갱신
→ 화면 즉시 반영
```

### UI.Controls ProgressBar 규약

- `NormalProgressBar`처럼 선형 ProgressBar를 직접 그릴 때는 전용 `ControlTemplate`에서 `PART_Track`, `PART_Indicator`를 사용해도 된다.
- `EllipseProgressBar`처럼 `ProgressBar`를 상속하지만 원형 Ellipse의 크기를 직접 유지해야 하는 컨트롤은 `PART_Track`, `PART_Indicator` 예약 이름을 사용하지 않는다.
- WPF `ProgressBar` 기본 로직은 `PART_Indicator` 크기를 `Value` 비율로 조정할 수 있으므로, 원형 Ellipse가 타원으로 왜곡될 수 있다.
- 원형 ProgressBar는 고정 크기 Ellipse 2개를 겹치고, 진행률은 `StrokeDashArray`로만 표현한다.
- 배경 Ellipse와 진행 Ellipse는 같은 `Width`, `Height`, `StrokeThickness`를 사용한다.
- 진행 Ellipse 이름은 예: `PART_ProgressEllipse`처럼 컨트롤 내부 전용 이름을 사용한다.

### EllipseProgressBar 텍스트 규약

- 중앙 텍스트는 `TextBlock`의 `Width`, `Height`를 지정하지 않고 내용 크기로 자동 측정되게 둔다.
- 중앙 정렬은 부모 `Grid` 안에서 `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`로 처리한다.
- 줄바꿈으로 인한 측정 오류를 막기 위해 `TextWrapping="NoWrap"`을 지정한다.
- `TextAlignment="Center"`는 유지한다.
- `IsTextBindedByValue=True`이면 `Value` 변경 시 텍스트는 `0.#%` 형식으로 자동 갱신한다.
- 직접 텍스트를 넣고 싶으면 `IsTextBindedByValue=False`로 두고 `TextBindedValue`를 직접 지정한다.

예:

```xaml
<ctrls:EllipseProgressBar
    Value="50"
    IsTextBindedByValue="True"/>

<ctrls:EllipseProgressBar
    IsTextBindedByValue="False"
    TextBindedValue="READY"/>
```

### ResourceDictionary / Resource 참조 규약

- 여러 ResourceDictionary가 Theme에서 형제 dictionary로 병합될 때, 다른 dictionary에 있는 Brush/Color/Image를 `StaticResource`로 즉시 참조하면 디자인뷰 또는 런타임에서 리소스 탐색 실패가 날 수 있다.
- 테마 변경 가능성이 있거나 형제 dictionary 참조라면 `DynamicResource`를 우선 사용한다.
- 같은 파일 안에서 위쪽에 이미 정의된 고정 리소스만 `StaticResource` 사용을 검토한다.
- Resource key는 폴더 경로로 namespace가 나뉘지 않는다. `Color.`, `Brush.`, `Storyboard.`, `Icon.` 같은 prefix를 유지한다.

## 11. UI.Controls EmergencyToggleButton 규약 - 2026-07-23

- `EmergencyToggleButton`은 `UI.Controls/Buttons`에 CustomControl 코드로 작성한다.
- `EmergencyToggleButton` 스타일은 `UI.Controls/Styles/EmergencyToggleButton.xaml`에 작성하고, `Themes/Generic.xaml`에서 merge한다.
- Emergency 아이콘은 `Image` 기반 리소스를 사용한다.
  - Idle 상태: `Icon.EmergencyIdle`
  - Pressed/Checked 상태: `Icon.EmergencyPressed`
- Emergency 아이콘을 `Geom.EmergencyFrame`, `Geom.EmergencyRound`, `Geom.EmergencyMidCircle` 기반 `Path`로 대체하지 않는다. 기존 BaseUI의 원하는 모양과 다르다.
- 애니메이션은 컨트롤 스타일 내부에 직접 작성하지 않고 `Lincsolution.UI.Resources/Animations/Storyboards.xaml`에 `Storyboard.*` 리소스로 분리한다.
- `EmergencyToggleButton`에서 사용하는 애니메이션 key:
  - `Storyboard.EmergencyButton.Rotate.Checked`
  - `Storyboard.EmergencyButton.Rotate.Unchecked`
  - `Storyboard.EmergencyButton.Blink.IdleImage`
  - `Storyboard.EmergencyButton.Blink.Background`
- `EmergencyToggleButton.xaml`은 `Storyboards.xaml`을 직접 merge한다. 개별 style dictionary는 `Generic.xaml`의 형제 dictionary 리소스를 `StaticResource`로 안정적으로 찾지 못할 수 있다.
- Checked 상태 배경 blink는 `Brush.MainBackground` 위에 `Brush.ErrorPressedLowOpacity` overlay를 두고 opacity를 반복 변경한다.
- ResourceDictionary 간 형제 리소스 참조에서 `StaticResource Color.*` 직접 참조는 피한다. Designer 또는 `LibraryTester` 실행 시 resource not found로 style 전체가 깨질 수 있다.
- 변경 후 최소 검증:
  - `dotnet build Lincsolution.Libraries.sln -p:Platform=x64`
  - `dotnet run --project LibraryTester\LibraryTester.csproj -p:Platform=x64`
  - Design view에서 `EmergencyToggleButton` 표시 확인
