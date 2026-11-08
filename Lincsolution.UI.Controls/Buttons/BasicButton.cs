using Lincsolution.UI.Base.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    public partial class BasicButton : Button
    {
        /// <summary>
        /// KO: BasicButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicButton control.
        /// </summary>
        static BasicButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicButton), new FrameworkPropertyMetadata(typeof(BasicButton)));
        }

        private interface IDependencyProperties
        {
            [DependencyProperty]
            CornerRadius CornerRadius { get; }

            [DependencyProperty]
            HorizontalAlignment ContentHorizontalAlignment { get; }

            [DependencyProperty]
            VerticalAlignment ContentVerticalAlignment { get; }

            [DependencyProperty]
            Thickness ContentMargin { get; }

            [DependencyProperty]
            Brush HighLightBackgroundBrush { get; }

            [DependencyProperty]
            Brush HighLightForegroundBrush { get; }

            [DependencyProperty]
            Brush HighLightBorderBrush { get; }

            [DependencyProperty]
            Thickness HighLightBorderThickness { get; }

            [DependencyProperty]
            Brush DisabledBackgroundBrush { get; }

            [DependencyProperty]
            Brush DisabledForegroundBrush { get; }

            [DependencyProperty]
            Brush DisabledBorderBrush { get; }

            [DependencyProperty]
            Thickness DisabledBorderThickness { get; }
        }
    }
}
