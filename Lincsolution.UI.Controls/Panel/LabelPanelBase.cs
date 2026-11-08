using Lincsolution.UI.Base.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls.Panel
{
    public partial class LabelPanelBase : Control
    {
        static LabelPanelBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelPanelBase), new FrameworkPropertyMetadata(typeof(LabelPanelBase)));
        }

        private interface IDependencyProperties
        {
            [DependencyProperty]
            CornerRadius CornerRadius { get; }

            [DependencyProperty]
            GridLength FrontSpace { get; }

            [DependencyProperty]
            GridLength MidSpace { get; }

            [DependencyProperty]
            GridLength EndSpace { get; }

            [DependencyProperty]
            GridLength FrontContentWidth { get; }

            [DependencyProperty]
            GridLength MidContentWidth { get; }

            [DependencyProperty]
            GridLength EndContentWidth { get; }

            [DependencyProperty(DefaultValue = 65d)]
            double FrontContentHeight { get; }

            [DependencyProperty(DefaultValue = 65d)]
            double MidContentHeight { get; }

            [DependencyProperty(DefaultValue = 65d)]
            double EndContentHeight { get; }

            [DependencyProperty]
            HorizontalAlignment FrontContentHorizontalAlignment { get; }

            [DependencyProperty]
            HorizontalAlignment MidContentHorizontalAlignment { get; }

            [DependencyProperty]
            HorizontalAlignment EndContentHorizontalAlignment { get; }

            [DependencyProperty]
            VerticalAlignment FrontContentVerticalAlignment { get; }

            [DependencyProperty]
            VerticalAlignment MidContentVerticalAlignment { get; }

            [DependencyProperty]
            VerticalAlignment EndContentVerticalAlignment { get; }
        }

        private static readonly CornerRadius CornerRadiusDefault = new CornerRadius(0);
        private static readonly GridLength FrontSpaceDefault = new GridLength(10);
        private static readonly GridLength MidSpaceDefault = new GridLength(10);
        private static readonly GridLength EndSpaceDefault = new GridLength(10);
        private static readonly GridLength FrontContentWidthDefault = new GridLength(150);
        private static readonly GridLength MidContentWidthDefault = new GridLength(100);
        private static readonly GridLength EndContentWidthDefault = new GridLength(100);
        private static readonly HorizontalAlignment FrontContentHorizontalAlignmentDefault = HorizontalAlignment.Stretch;
        private static readonly HorizontalAlignment MidContentHorizontalAlignmentDefault = HorizontalAlignment.Stretch;
        private static readonly HorizontalAlignment EndContentHorizontalAlignmentDefault = HorizontalAlignment.Stretch;
        private static readonly VerticalAlignment FrontContentVerticalAlignmentDefault = VerticalAlignment.Center;
        private static readonly VerticalAlignment MidContentVerticalAlignmentDefault = VerticalAlignment.Center;
        private static readonly VerticalAlignment EndContentVerticalAlignmentDefault = VerticalAlignment.Center;


    }
}
