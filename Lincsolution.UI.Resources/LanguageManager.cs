using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Lincsolution.UI.Resources
{
    /// <summary>
    /// KO: 애플리케이션의 언어 리소스와 UI 문화를 전환합니다.
    /// EN: Switches the application's language resources and UI culture.
    /// </summary>
    public static class LanguageManager
    {
        private const string ResourcePrefix = "/Lincsolution.UI.Resources;component/Language/String.";
        private static readonly HashSet<string> SupportedCultureNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "en-US",
            "ko-KR"
        };
        private static readonly Dictionary<string, IReadOnlyDictionary<string, Uri>> RegisteredSources =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<ResourceDictionary> CurrentDictionaries = new();
        private static readonly object SyncRoot = new();

        /// <summary>
        /// KO: 현재 적용된 UI 문화권입니다.
        /// EN: Gets the currently applied UI culture.
        /// </summary>
        public static CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// KO: 지원하는 문화권 이름 목록입니다.
        /// EN: Gets the supported culture names.
        /// </summary>
        public static IReadOnlyCollection<string> SupportedCultures => SupportedCultureNames;

        /// <summary>
        /// KO: 언어가 변경된 후 발생합니다.
        /// EN: Occurs after the language has changed.
        /// </summary>
        public static event EventHandler? LanguageChanged;

        /// <summary>
        /// KO: 프로젝트별 언어 리소스 사전 경로를 이름으로 등록합니다. 같은 이름은 새 값으로 교체됩니다.
        /// EN: Registers project-specific language dictionary paths by name. The same name is replaced.
        /// </summary>
        /// <param name="name">KO: 언어 소스를 구분하는 고유 이름입니다. EN: A unique name identifying the language source.</param>
        /// <param name="dictionaries">KO: 문화권 이름과 리소스 사전 URI의 대응 정보입니다. EN: Culture names mapped to resource dictionary URIs.</param>
        public static void RegisterSource(string name, IReadOnlyDictionary<string, Uri> dictionaries)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(dictionaries);

            var source = new Dictionary<string, Uri>(StringComparer.OrdinalIgnoreCase);

            foreach(var cultureName in SupportedCultureNames)
            {
                if(!dictionaries.TryGetValue(cultureName, out var uri) || uri is null)
                {
                    throw new ArgumentException(
                        $"Language source '{name}' does not contain culture '{cultureName}'.",
                        nameof(dictionaries));
                }

                source[cultureName] = uri;
            }

            lock(SyncRoot)
            {
                RegisteredSources[name] = source;
            }
        }

        /// <summary>
        /// KO: 지정한 문화권의 언어 리소스를 적용합니다.
        /// EN: Applies the language resources for the specified culture.
        /// </summary>
        public static void ApplyLanguage(string cultureName)
        {
            if(!SupportedCultureNames.Contains(cultureName))
            {
                throw new ArgumentOutOfRangeException(nameof(cultureName), cultureName, "The culture is not supported.");
            }

            var application = Application.Current
                ?? throw new InvalidOperationException("A WPF Application must exist before applying a language.");

            if(!application.Dispatcher.CheckAccess())
            {
                application.Dispatcher.Invoke(() => ApplyLanguage(cultureName));
                return;
            }

            var culture = CultureInfo.GetCultureInfo(cultureName);
            List<IReadOnlyDictionary<string, Uri>> sources;

            lock(SyncRoot)
            {
                sources = new List<IReadOnlyDictionary<string, Uri>>(RegisteredSources.Values);
            }

            var nextDictionaries = new List<ResourceDictionary>
            {
                CreateDictionary(new Uri($"{ResourcePrefix}{culture.Name}.xaml", UriKind.Relative))
            };

            foreach(var source in sources)
            {
                nextDictionaries.Add(CreateDictionary(source[culture.Name]));
            }

            foreach(var dictionary in CurrentDictionaries)
            {
                application.Resources.MergedDictionaries.Remove(dictionary);
            }

            CurrentDictionaries.Clear();

            foreach(var dictionary in nextDictionaries)
            {
                application.Resources.MergedDictionaries.Add(dictionary);
                CurrentDictionaries.Add(dictionary);
            }

            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CurrentCulture = culture;
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        private static ResourceDictionary CreateDictionary(Uri source) => new()
        {
            Source = source
        };
    }
}
