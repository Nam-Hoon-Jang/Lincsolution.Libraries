using Lincsolution.UI.Base.Common;
using Lincsolution.UI.Controls.Panel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    public partial class TextBoxButtonPanel : LabelPanelBase
    {
        static TextBoxButtonPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxButtonPanel), new FrameworkPropertyMetadata(typeof(TextBoxButtonPanel)));
        }
        
        private interface IDependencyProperties
        {
            [DependencyProperty(DefaultValue = 150d, ChangedCallback = nameof(OnFrontTextWidthChanged))]
            double FrontTextWidth { get; }

            [DependencyProperty(DefaultValue = 100d, ChangedCallback = nameof(OnMidTextWidthChanged))]
            double MidTextWidth { get; }

            [DependencyProperty(DefaultValue = 100d, ChangedCallback = nameof(OnEndButtonWidthChanged))]
            double EndButtonWidth { get; }

            [DependencyProperty(DefaultValue = 20d)]
            double FrontTextFontSize { get; }

            [DependencyProperty(DefaultValue = 20d)]
            double MidTextFontSize { get; }

            [DependencyProperty(DefaultValue = 20d)]
            double ButtonContentFontSize { get; }

            [DependencyProperty]
            FontWeight FrontTextFontWeight { get; }

            [DependencyProperty]
            FontWeight MidTextFontWeight { get; }

            [DependencyProperty]
            FontWeight ButtonContentFontWeight { get; }

            [DependencyProperty]
            TextAlignment FrontTextAlignment { get; }

            [DependencyProperty]
            TextAlignment MidTextAlignment { get; }

            [DependencyProperty]
            HorizontalAlignment ButtonContentHorizontalAlignment { get; }

            [DependencyProperty]
            VerticalAlignment ButtonContentVerticalAlignment { get; }

            [DependencyProperty]
            Brush WatermarkForeground { get; }

            [DependencyProperty]
            string FrontText { get; }

            [DependencyProperty]
            string MidText { get; }

            [DependencyProperty]
            string ButtonContentText { get; }

            [DependencyProperty]
            string Watermark { get; }

            [DependencyProperty]
            CornerRadius MidTextCornerRadius { get; }

            [DependencyProperty]
            CornerRadius ButtonCornerRadius { get; }
        }

        private static readonly FontWeight FrontTextFontWeightDefault = FontWeights.Bold;
        private static readonly FontWeight MidTextFontWeightDefault = FontWeights.Regular;
        private static readonly FontWeight ButtonContentFontWeightDefault = FontWeights.SemiBold;
        private static readonly TextAlignment FrontTextAlignmentDefault = TextAlignment.Center;
        private static readonly TextAlignment MidTextAlignmentDefault = TextAlignment.Center;
        private static readonly HorizontalAlignment ButtonContentHorizontalAlignmentDefault = HorizontalAlignment.Center;
        private static readonly VerticalAlignment ButtonContentVerticalAlignmentDefault = VerticalAlignment.Center;
        private static readonly CornerRadius MidTextCornerRadiusDefault = new CornerRadius(5);
        private static readonly CornerRadius ButtonCornerRadiusDefault = new CornerRadius(5);

        private static void OnFrontTextWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBoxButtonPanel panel && e.NewValue is double width)
            {
                panel.SetCurrentValue(FrontContentWidthProperty, new GridLength(width));
            }
        }

        private static void OnMidTextWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBoxButtonPanel panel && e.NewValue is double width)
            {
                panel.SetCurrentValue(MidContentWidthProperty, new GridLength(width));
            }
        }

        private static void OnEndButtonWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is TextBoxButtonPanel panel && e.NewValue is double width)
            {
                panel.SetCurrentValue(EndContentWidthProperty, new GridLength(width));
            }
        }
    }
}
