using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Lincsolution.UI.Controls;

namespace Lincsolution.UI;

/// <summary>
/// 하위 화면의 공통 헤더와 콘텐츠 영역을 제공하는 기본 컨트롤입니다.
/// Provides the common header and content area for sub-views.
/// </summary>
[ContentProperty(nameof(ChildContent))]
public class BaseSubView : Control
{
    /// <summary>
    /// KO: <see cref="BaseSubView"/>의 기본 스타일 키를 등록합니다.<br/>
    /// EN: Registers the default style key for <see cref="BaseSubView"/>.
    /// </summary>
    static BaseSubView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseSubView), new FrameworkPropertyMetadata(typeof(BaseSubView)));
    }

    public static readonly DependencyProperty TitleHeightProperty = DependencyProperty.Register(nameof(TitleHeight), typeof(GridLength), typeof(BaseSubView), new PropertyMetadata(new GridLength(88.0)));
    public static readonly DependencyProperty ButtonContentHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(ButtonContentHorizontalAlignment), typeof(HorizontalAlignment), typeof(BaseSubView));
    public static readonly DependencyProperty ButtonContentVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(ButtonContentVerticalAlignment), typeof(VerticalAlignment), typeof(BaseSubView));
    public static readonly DependencyProperty ButtonContentMarginProperty = DependencyProperty.Register(nameof(ButtonContentMargin), typeof(Thickness), typeof(BaseSubView), new FrameworkPropertyMetadata(new Thickness(0)));
    /// <summary>KO: 버튼 글꼴 크기 종속성 속성입니다. EN: Identifies the button font-size dependency property.</summary>
    public static readonly DependencyProperty ButtonFontSizeProperty = DependencyProperty.Register(nameof(ButtonFontSize), typeof(double), typeof(BaseSubView), new PropertyMetadata(20d));

    /// <summary>KO: 버튼 사이 간격 종속성 속성입니다. EN: Identifies the button-spacing dependency property.</summary>
    public static readonly DependencyProperty ButtonSpacingProperty = DependencyProperty.Register(nameof(ButtonSpacing), typeof(double), typeof(BaseSubView), new PropertyMetadata(10d));

    /// <summary>KO: 첫 번째 버튼 콘텐츠 종속성 속성입니다. EN: Identifies the first button content dependency property.</summary>
    public static readonly DependencyProperty Button1ContentProperty = DependencyProperty.Register(nameof(Button1Content), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 두 번째 버튼 콘텐츠 종속성 속성입니다. EN: Identifies the second button content dependency property.</summary>
    public static readonly DependencyProperty Button2ContentProperty = DependencyProperty.Register(nameof(Button2Content), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 세 번째 버튼 콘텐츠 종속성 속성입니다. EN: Identifies the third button content dependency property.</summary>
    public static readonly DependencyProperty Button3ContentProperty = DependencyProperty.Register(nameof(Button3Content), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 첫 번째 버튼 사용 여부 종속성 속성입니다. EN: Identifies the first button usage dependency property.</summary>
    public static readonly DependencyProperty UseButton1Property = DependencyProperty.Register(nameof(UseButton1), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 두 번째 버튼 사용 여부 종속성 속성입니다. EN: Identifies the second button usage dependency property.</summary>
    public static readonly DependencyProperty UseButton2Property = DependencyProperty.Register(nameof(UseButton2), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 세 번째 버튼 사용 여부 종속성 속성입니다. EN: Identifies the third button usage dependency property.</summary>
    public static readonly DependencyProperty UseButton3Property = DependencyProperty.Register(nameof(UseButton3), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 첫 번째 버튼 활성화 상태 종속성 속성입니다. EN: Identifies the first button enabled-state dependency property.</summary>
    public static readonly DependencyProperty IsButton1EnabledProperty = DependencyProperty.Register(nameof(IsButton1Enabled), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 두 번째 버튼 활성화 상태 종속성 속성입니다. EN: Identifies the second button enabled-state dependency property.</summary>
    public static readonly DependencyProperty IsButton2EnabledProperty = DependencyProperty.Register(nameof(IsButton2Enabled), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 세 번째 버튼 활성화 상태 종속성 속성입니다. EN: Identifies the third button enabled-state dependency property.</summary>
    public static readonly DependencyProperty IsButton3EnabledProperty = DependencyProperty.Register(nameof(IsButton3Enabled), typeof(bool), typeof(BaseSubView), new PropertyMetadata(true));

    /// <summary>KO: 첫 번째 버튼 선택 상태 종속성 속성입니다. EN: Identifies the first button checked-state dependency property.</summary>
    public static readonly DependencyProperty IsButton1CheckedProperty = DependencyProperty.Register(nameof(IsButton1Checked), typeof(bool), typeof(BaseSubView), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>KO: 두 번째 버튼 선택 상태 종속성 속성입니다. EN: Identifies the second button checked-state dependency property.</summary>
    public static readonly DependencyProperty IsButton2CheckedProperty = DependencyProperty.Register(nameof(IsButton2Checked), typeof(bool), typeof(BaseSubView), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>KO: 세 번째 버튼 선택 상태 종속성 속성입니다. EN: Identifies the third button checked-state dependency property.</summary>
    public static readonly DependencyProperty IsButton3CheckedProperty = DependencyProperty.Register(nameof(IsButton3Checked), typeof(bool), typeof(BaseSubView), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>KO: 상단 버튼 공용 명령 종속성 속성입니다. EN: Identifies the shared header-button command dependency property.</summary>
    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(BaseSubView));

    /// <summary>KO: 첫 번째 버튼 명령 매개변수 종속성 속성입니다. EN: Identifies the first button command-parameter dependency property.</summary>
    public static readonly DependencyProperty Button1CommandParameterProperty = DependencyProperty.Register(nameof(Button1CommandParameter), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 두 번째 버튼 명령 매개변수 종속성 속성입니다. EN: Identifies the second button command-parameter dependency property.</summary>
    public static readonly DependencyProperty Button2CommandParameterProperty = DependencyProperty.Register(nameof(Button2CommandParameter), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 세 번째 버튼 명령 매개변수 종속성 속성입니다. EN: Identifies the third button command-parameter dependency property.</summary>
    public static readonly DependencyProperty Button3CommandParameterProperty = DependencyProperty.Register(nameof(Button3CommandParameter), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 각 버튼 너비 종속성 속성입니다. EN: Identifies the width dependency property for each button.</summary>
    public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(nameof(ButtonWidth), typeof(GridLength), typeof(BaseSubView), new PropertyMetadata(new GridLength(120)));

    /// <summary>KO: 선택 강조 테두리 두께 종속성 속성입니다. EN: Identifies the selection-highlight border thickness dependency property.</summary>
    public static readonly DependencyProperty HighlightBorderThicknessProperty = DependencyProperty.Register(nameof(HighlightBorderThickness), typeof(Thickness), typeof(BaseSubView), new PropertyMetadata(new Thickness(0, 0, 0, 3)));

    /// <summary>KO: 하위 콘텐츠 종속성 속성입니다. EN: Identifies the child-content dependency property.</summary>
    public static readonly DependencyProperty ChildContentProperty = DependencyProperty.Register(nameof(ChildContent), typeof(object), typeof(BaseSubView));

    /// <summary>KO: 상단 RadioButton 그룹 이름 종속성 속성입니다. EN: Identifies the header radio-button group-name dependency property.</summary>
    public static readonly DependencyProperty ButtonGroupNameProperty = DependencyProperty.Register(nameof(ButtonGroupName), typeof(string), typeof(BaseSubView), new PropertyMetadata("BaseSubButtons"));

    public GridLength TitleHeight { get => (GridLength)GetValue(TitleHeightProperty); set => SetValue(TitleHeightProperty, value); }
    public HorizontalAlignment ButtonContentHorizontalAlignment { get => (HorizontalAlignment)GetValue(ButtonContentHorizontalAlignmentProperty); set => SetValue(ButtonContentHorizontalAlignmentProperty, value); }
    public VerticalAlignment ButtonContentVerticalAlignment { get => (VerticalAlignment)GetValue(ButtonContentVerticalAlignmentProperty); set => SetValue(ButtonContentVerticalAlignmentProperty, value); }
    public Thickness ButtonContentMargin { get => (Thickness)GetValue(ButtonContentMarginProperty); set => SetValue(ButtonContentMarginProperty, value); }
    
    /// <summary>KO: 상단 버튼의 글꼴 크기를 가져오거나 설정합니다. EN: Gets or sets the header button font size.</summary>
    [LocalizedDescription("상단 버튼의 글꼴 크기를 가져오거나 설정합니다.", "Gets or sets the header button font size.")]
    public double ButtonFontSize { get => (double)GetValue(ButtonFontSizeProperty); set => SetValue(ButtonFontSizeProperty, value); }

    /// <summary>KO: 상단 버튼 사이의 간격을 가져오거나 설정합니다. EN: Gets or sets the spacing between header buttons.</summary>
    [LocalizedDescription("상단 버튼 사이의 간격을 가져오거나 설정합니다.", "Gets or sets the spacing between header buttons.")]
    public double ButtonSpacing { get => (double)GetValue(ButtonSpacingProperty); set => SetValue(ButtonSpacingProperty, value); }

    /// <summary>KO: 첫 번째 버튼의 콘텐츠를 가져오거나 설정합니다. EN: Gets or sets the first button content.</summary>
    [LocalizedDescription("첫 번째 버튼의 콘텐츠를 가져오거나 설정합니다.", "Gets or sets the first button content.")]
    public object? Button1Content { get => GetValue(Button1ContentProperty); set => SetValue(Button1ContentProperty, value); }

    /// <summary>KO: 두 번째 버튼의 콘텐츠를 가져오거나 설정합니다. EN: Gets or sets the second button content.</summary>
    [LocalizedDescription("두 번째 버튼의 콘텐츠를 가져오거나 설정합니다.", "Gets or sets the second button content.")]
    public object? Button2Content { get => GetValue(Button2ContentProperty); set => SetValue(Button2ContentProperty, value); }

    /// <summary>KO: 세 번째 버튼의 콘텐츠를 가져오거나 설정합니다. EN: Gets or sets the third button content.</summary>
    [LocalizedDescription("세 번째 버튼의 콘텐츠를 가져오거나 설정합니다.", "Gets or sets the third button content.")]
    public object? Button3Content { get => GetValue(Button3ContentProperty); set => SetValue(Button3ContentProperty, value); }

    /// <summary>KO: 첫 번째 버튼을 표시할지 여부를 가져오거나 설정합니다. EN: Gets or sets whether the first button is displayed.</summary>
    [LocalizedDescription("첫 번째 버튼을 표시할지 여부를 가져오거나 설정합니다.", "Gets or sets whether the first button is displayed.")]
    public bool UseButton1 { get => (bool)GetValue(UseButton1Property); set => SetValue(UseButton1Property, value); }

    /// <summary>KO: 두 번째 버튼을 표시할지 여부를 가져오거나 설정합니다. EN: Gets or sets whether the second button is displayed.</summary>
    [LocalizedDescription("두 번째 버튼을 표시할지 여부를 가져오거나 설정합니다.", "Gets or sets whether the second button is displayed.")]
    public bool UseButton2 { get => (bool)GetValue(UseButton2Property); set => SetValue(UseButton2Property, value); }

    /// <summary>KO: 세 번째 버튼을 표시할지 여부를 가져오거나 설정합니다. EN: Gets or sets whether the third button is displayed.</summary>
    [LocalizedDescription("세 번째 버튼을 표시할지 여부를 가져오거나 설정합니다.", "Gets or sets whether the third button is displayed.")]
    public bool UseButton3 { get => (bool)GetValue(UseButton3Property); set => SetValue(UseButton3Property, value); }

    /// <summary>KO: 첫 번째 버튼의 활성화 여부를 가져오거나 설정합니다. EN: Gets or sets whether the first button is enabled.</summary>
    [LocalizedDescription("첫 번째 버튼의 활성화 여부를 가져오거나 설정합니다.", "Gets or sets whether the first button is enabled.")]
    public bool IsButton1Enabled { get => (bool)GetValue(IsButton1EnabledProperty); set => SetValue(IsButton1EnabledProperty, value); }

    /// <summary>KO: 두 번째 버튼의 활성화 여부를 가져오거나 설정합니다. EN: Gets or sets whether the second button is enabled.</summary>
    [LocalizedDescription("두 번째 버튼의 활성화 여부를 가져오거나 설정합니다.", "Gets or sets whether the second button is enabled.")]
    public bool IsButton2Enabled { get => (bool)GetValue(IsButton2EnabledProperty); set => SetValue(IsButton2EnabledProperty, value); }

    /// <summary>KO: 세 번째 버튼의 활성화 여부를 가져오거나 설정합니다. EN: Gets or sets whether the third button is enabled.</summary>
    [LocalizedDescription("세 번째 버튼의 활성화 여부를 가져오거나 설정합니다.", "Gets or sets whether the third button is enabled.")]
    public bool IsButton3Enabled { get => (bool)GetValue(IsButton3EnabledProperty); set => SetValue(IsButton3EnabledProperty, value); }

    /// <summary>KO: 첫 번째 버튼의 선택 상태를 가져오거나 설정합니다. EN: Gets or sets whether the first button is selected.</summary>
    [LocalizedDescription("첫 번째 버튼의 선택 상태를 가져오거나 설정합니다.", "Gets or sets whether the first button is selected.")]
    public bool IsButton1Checked { get => (bool)GetValue(IsButton1CheckedProperty); set => SetValue(IsButton1CheckedProperty, value); }

    /// <summary>KO: 두 번째 버튼의 선택 상태를 가져오거나 설정합니다. EN: Gets or sets whether the second button is selected.</summary>
    [LocalizedDescription("두 번째 버튼의 선택 상태를 가져오거나 설정합니다.", "Gets or sets whether the second button is selected.")]
    public bool IsButton2Checked { get => (bool)GetValue(IsButton2CheckedProperty); set => SetValue(IsButton2CheckedProperty, value); }

    /// <summary>KO: 세 번째 버튼의 선택 상태를 가져오거나 설정합니다. EN: Gets or sets whether the third button is selected.</summary>
    [LocalizedDescription("세 번째 버튼의 선택 상태를 가져오거나 설정합니다.", "Gets or sets whether the third button is selected.")]
    public bool IsButton3Checked { get => (bool)GetValue(IsButton3CheckedProperty); set => SetValue(IsButton3CheckedProperty, value); }

    /// <summary>KO: 상단 버튼들이 공유하는 명령을 가져오거나 설정합니다. EN: Gets or sets the command shared by the header buttons.</summary>
    [LocalizedDescription("상단 버튼들이 공유하는 명령을 가져오거나 설정합니다.", "Gets or sets the command shared by the header buttons.")]
    public ICommand? Command { get => (ICommand?)GetValue(CommandProperty); set => SetValue(CommandProperty, value); }

    /// <summary>KO: 첫 번째 버튼의 명령 매개변수를 가져오거나 설정합니다. EN: Gets or sets the first button command parameter.</summary>
    [LocalizedDescription("첫 번째 버튼의 명령 매개변수를 가져오거나 설정합니다.", "Gets or sets the first button command parameter.")]
    public object? Button1CommandParameter { get => GetValue(Button1CommandParameterProperty); set => SetValue(Button1CommandParameterProperty, value); }

    /// <summary>KO: 두 번째 버튼의 명령 매개변수를 가져오거나 설정합니다. EN: Gets or sets the second button command parameter.</summary>
    [LocalizedDescription("두 번째 버튼의 명령 매개변수를 가져오거나 설정합니다.", "Gets or sets the second button command parameter.")]
    public object? Button2CommandParameter { get => GetValue(Button2CommandParameterProperty); set => SetValue(Button2CommandParameterProperty, value); }

    /// <summary>KO: 세 번째 버튼의 명령 매개변수를 가져오거나 설정합니다. EN: Gets or sets the third button command parameter.</summary>
    [LocalizedDescription("세 번째 버튼의 명령 매개변수를 가져오거나 설정합니다.", "Gets or sets the third button command parameter.")]
    public object? Button3CommandParameter { get => GetValue(Button3CommandParameterProperty); set => SetValue(Button3CommandParameterProperty, value); }

    /// <summary>KO: 각 상단 버튼의 너비를 가져오거나 설정합니다. EN: Gets or sets the width of each header button.</summary>
    [LocalizedDescription("각 상단 버튼의 너비를 가져오거나 설정합니다.", "Gets or sets the width of each header button.")]
    public GridLength ButtonWidth { get => (GridLength)GetValue(ButtonWidthProperty); set => SetValue(ButtonWidthProperty, value); }

    /// <summary>KO: 선택된 버튼의 강조 테두리 두께를 가져오거나 설정합니다. EN: Gets or sets the highlight border thickness of the selected button.</summary>
    [LocalizedDescription("선택된 버튼의 강조 테두리 두께를 가져오거나 설정합니다.", "Gets or sets the highlight border thickness of the selected button.")]
    public Thickness HighlightBorderThickness { get => (Thickness)GetValue(HighlightBorderThicknessProperty); set => SetValue(HighlightBorderThicknessProperty, value); }

    /// <summary>KO: 헤더 아래에 표시할 하위 콘텐츠를 가져오거나 설정합니다. EN: Gets or sets the child content displayed below the header.</summary>
    [LocalizedDescription("헤더 아래에 표시할 하위 콘텐츠를 가져오거나 설정합니다.", "Gets or sets the child content displayed below the header.")]
    public object? ChildContent { get => GetValue(ChildContentProperty); set => SetValue(ChildContentProperty, value); }

    /// <summary>KO: 상단 RadioButton들이 공유할 그룹 이름을 가져오거나 설정합니다. EN: Gets or sets the group name shared by the header radio buttons.</summary>
    [LocalizedDescription("상단 RadioButton들이 공유할 그룹 이름을 가져오거나 설정합니다.", "Gets or sets the group name shared by the header radio buttons.")]
    public string ButtonGroupName { get => (string)GetValue(ButtonGroupNameProperty); set => SetValue(ButtonGroupNameProperty, value); }
}
