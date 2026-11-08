using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    public class BorderHighlightRadioButton : BasicRadioButton
    {
        static BorderHighlightRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderHighlightRadioButton), new FrameworkPropertyMetadata(typeof(BorderHighlightRadioButton)));
        }

        public static readonly DependencyProperty HighlightBorderThicknessProperty = DependencyProperty.Register(nameof(HighlightBorderThickness), typeof(Thickness), typeof(BorderHighlightRadioButton), new PropertyMetadata(new Thickness(0, 0, 0, 3)));

        public Thickness HighlightBorderThickness
        {
            get => (Thickness)GetValue(HighlightBorderThicknessProperty); set => SetValue(HighlightBorderThicknessProperty, value);
        }
    }
}
