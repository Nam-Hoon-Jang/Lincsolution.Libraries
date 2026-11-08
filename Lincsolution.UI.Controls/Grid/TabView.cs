using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lincsolution.UI.Controls;

public class TabView : Control
{
    static TabView() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(typeof(TabView)));

    public static readonly DependencyProperty TitleProperty = Register(nameof(Title), "");
    public static readonly DependencyProperty TitleFontSizeProperty = Register(nameof(TitleFontSize), 30d);
    public static readonly DependencyProperty ButtonSpacingProperty = Register(nameof(ButtonSpacing), 10d);
    public static readonly DependencyProperty Button1ContentProperty = Register(nameof(Button1Content), "");
    public static readonly DependencyProperty Button2ContentProperty = Register(nameof(Button2Content), "");
    public static readonly DependencyProperty Button3ContentProperty = Register(nameof(Button3Content), "");
    public static readonly DependencyProperty UseButton1Property = Register(nameof(UseButton1), true);
    public static readonly DependencyProperty UseButton2Property = Register(nameof(UseButton2), true);
    public static readonly DependencyProperty UseButton3Property = Register(nameof(UseButton3), true);
    public static readonly DependencyProperty IsButton1EnabledProperty = Register(nameof(IsButton1Enabled), true);
    public static readonly DependencyProperty IsButton2EnabledProperty = Register(nameof(IsButton2Enabled), true);
    public static readonly DependencyProperty IsButton3EnabledProperty = Register(nameof(IsButton3Enabled), true);
    public static readonly DependencyProperty Button1CommandProperty = Register<ICommand?>(nameof(Button1Command), null);
    public static readonly DependencyProperty Button2CommandProperty = Register<ICommand?>(nameof(Button2Command), null);
    public static readonly DependencyProperty Button3CommandProperty = Register<ICommand?>(nameof(Button3Command), null);
    public static readonly DependencyProperty Button1CommandParameterProperty = Register<object?>(nameof(Button1CommandParameter), null);
    public static readonly DependencyProperty Button2CommandParameterProperty = Register<object?>(nameof(Button2CommandParameter), null);
    public static readonly DependencyProperty Button3CommandParameterProperty = Register<object?>(nameof(Button3CommandParameter), null);
    public static readonly DependencyProperty TitleWidthProperty = Register(nameof(TitleWidth), new GridLength(160));
    public static readonly DependencyProperty ButtonWidthProperty = Register(nameof(ButtonWidth), new GridLength(120));
    public static readonly DependencyProperty HighlightBorderThicknessProperty = Register(nameof(HighlightBorderThickness), new Thickness(0, 0, 0, 3));
    public static readonly DependencyProperty TitleBorderThicknessProperty = Register(nameof(TitleBorderThickness), new Thickness(0, 0, 3, 0));

    public object? Title { get => GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public double TitleFontSize { get => (double)GetValue(TitleFontSizeProperty); set => SetValue(TitleFontSizeProperty, value); }
    public double ButtonSpacing { get => (double)GetValue(ButtonSpacingProperty); set => SetValue(ButtonSpacingProperty, value); }
    public object? Button1Content { get => GetValue(Button1ContentProperty); set => SetValue(Button1ContentProperty, value); }
    public object? Button2Content { get => GetValue(Button2ContentProperty); set => SetValue(Button2ContentProperty, value); }
    public object? Button3Content { get => GetValue(Button3ContentProperty); set => SetValue(Button3ContentProperty, value); }
    public bool UseButton1 { get => (bool)GetValue(UseButton1Property); set => SetValue(UseButton1Property, value); }
    public bool UseButton2 { get => (bool)GetValue(UseButton2Property); set => SetValue(UseButton2Property, value); }
    public bool UseButton3 { get => (bool)GetValue(UseButton3Property); set => SetValue(UseButton3Property, value); }
    public bool IsButton1Enabled { get => (bool)GetValue(IsButton1EnabledProperty); set => SetValue(IsButton1EnabledProperty, value); }
    public bool IsButton2Enabled { get => (bool)GetValue(IsButton2EnabledProperty); set => SetValue(IsButton2EnabledProperty, value); }
    public bool IsButton3Enabled { get => (bool)GetValue(IsButton3EnabledProperty); set => SetValue(IsButton3EnabledProperty, value); }
    public ICommand? Button1Command { get => (ICommand?)GetValue(Button1CommandProperty); set => SetValue(Button1CommandProperty, value); }
    public ICommand? Button2Command { get => (ICommand?)GetValue(Button2CommandProperty); set => SetValue(Button2CommandProperty, value); }
    public ICommand? Button3Command { get => (ICommand?)GetValue(Button3CommandProperty); set => SetValue(Button3CommandProperty, value); }
    public object? Button1CommandParameter { get => GetValue(Button1CommandParameterProperty); set => SetValue(Button1CommandParameterProperty, value); }
    public object? Button2CommandParameter { get => GetValue(Button2CommandParameterProperty); set => SetValue(Button2CommandParameterProperty, value); }
    public object? Button3CommandParameter { get => GetValue(Button3CommandParameterProperty); set => SetValue(Button3CommandParameterProperty, value); }
    public GridLength TitleWidth { get => (GridLength)GetValue(TitleWidthProperty); set => SetValue(TitleWidthProperty, value); }
    public GridLength ButtonWidth { get => (GridLength)GetValue(ButtonWidthProperty); set => SetValue(ButtonWidthProperty, value); }
    public Thickness HighlightBorderThickness { get => (Thickness)GetValue(HighlightBorderThicknessProperty); set => SetValue(HighlightBorderThicknessProperty, value); }
    public Thickness TitleBorderThickness { get => (Thickness)GetValue(TitleBorderThicknessProperty); set => SetValue(TitleBorderThicknessProperty, value); }

    private static DependencyProperty Register<T>(string name, T value) => DependencyProperty.Register(name, typeof(T), typeof(TabView), new FrameworkPropertyMetadata(value));
}
