using System;
using System.ComponentModel;
using System.Globalization;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 현재 UI 언어에 맞는 속성 설명을 Visual Studio 속성 창에 제공합니다.
    /// EN: Provides a property description matching the current UI culture in the Visual Studio Properties window.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        private readonly string _koreanDescription;
        private readonly string _englishDescription;

        /// <summary>
        /// KO: 한국어 및 영어 설명을 사용하여 특성을 생성합니다.
        /// EN: Creates the attribute with Korean and English descriptions.
        /// </summary>
        public LocalizedDescriptionAttribute(string koreanDescription, string englishDescription)
        {
            _koreanDescription = koreanDescription;
            _englishDescription = englishDescription;
        }

        /// <summary>
        /// KO: 현재 UI 문화권이 한국어이면 한국어 설명을, 아니면 영어 설명을 반환합니다.
        /// EN: Returns the Korean description for a Korean UI culture; otherwise returns English.
        /// </summary>
        public override string Description =>
            string.Equals(
                CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                "ko",
                StringComparison.OrdinalIgnoreCase)
                ? _koreanDescription
                : _englishDescription;
    }
}
