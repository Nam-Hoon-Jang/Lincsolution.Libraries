using System;
using System.Windows;

namespace Lincsolution.UI.Base.Common
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class DependencyPropertyAttribute : Attribute
    {
        /// <summary>Attribute로 등록하는 DependencyProperty의 기본값.</summary>
        public object? DefaultValue { get; set; }

        /// <summary>FrameworkPropertyMetadata 옵션</summary>
        public FrameworkPropertyMetadataOptions Options { get; set; }
            = FrameworkPropertyMetadataOptions.None;

        /// <summary>PropertyChanged Callback 메소드 이름. nameof(OnXXXChanged) 사용 권장.</summary>
        public string? ChangedCallback { get; set; }
    }
}
