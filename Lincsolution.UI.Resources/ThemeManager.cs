using System;
using System.Windows;

namespace Lincsolution.UI.Resources
{
    /// <summary>
    /// KO: 애플리케이션의 공용 색상 테마 리소스를 전환합니다.
    /// EN: Switches the application's shared color-theme resources.
    /// </summary>
    public static class ThemeManager
    {
        private const string ResourcePrefix = "/Lincsolution.UI.Resources;component/Themes/";
        private static ResourceDictionary? currentDictionary;

        /// <summary>
        /// KO: 현재 적용된 테마입니다.
        /// EN: Gets the currently applied theme.
        /// </summary>
        public static UiTheme CurrentTheme { get; private set; } = UiTheme.Light;

        /// <summary>
        /// KO: 테마가 변경된 후 발생합니다.
        /// EN: Occurs after the theme has changed.
        /// </summary>
        public static event EventHandler? ThemeChanged;

        /// <summary>
        /// KO: 지정한 테마를 애플리케이션 리소스에 적용합니다.
        /// EN: Applies the specified theme to the application resources.
        /// </summary>
        public static void ApplyTheme(UiTheme theme)
        {
            var application = Application.Current
                ?? throw new InvalidOperationException("A WPF Application must exist before applying a theme.");

            if(!application.Dispatcher.CheckAccess())
            {
                application.Dispatcher.Invoke(() => ApplyTheme(theme));
                return;
            }

            if(currentDictionary is not null)
            {
                application.Resources.MergedDictionaries.Remove(currentDictionary);
            }

            currentDictionary = new ResourceDictionary
            {
                Source = new Uri($"{ResourcePrefix}{theme}.xaml", UriKind.Relative)
            };

            application.Resources.MergedDictionaries.Add(currentDictionary);
            CurrentTheme = theme;
            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>
    /// KO: UI에서 지원하는 색상 테마입니다.
    /// EN: Defines the color themes supported by the UI.
    /// </summary>
    public enum UiTheme
    {
        Light,
        Dark
    }
}
