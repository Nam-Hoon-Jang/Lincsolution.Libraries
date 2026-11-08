using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    public class BasicRadioButton : RadioButton
    {
        static BasicRadioButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicRadioButton), new FrameworkPropertyMetadata(typeof(BasicRadioButton)));
        }

        public static readonly DependencyProperty CornerRadiusProperty  = DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BasicRadioButton), new FrameworkPropertyMetadata(new CornerRadius(0)));
        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(nameof(ContentWidth), typeof(double), typeof(BasicRadioButton), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty ContentHeightProperty = DependencyProperty.Register(nameof(ContentHeight), typeof(double), typeof(BasicRadioButton), new FrameworkPropertyMetadata(double.NaN));
        public static readonly DependencyProperty ContentHorizontalAlignmentProperty = DependencyProperty.Register(nameof(ContentHorizontalAlignment), typeof(HorizontalAlignment), typeof(BasicRadioButton));
        public static readonly DependencyProperty ContentVerticalAlignmentProperty = DependencyProperty.Register(nameof(ContentVerticalAlignment), typeof(VerticalAlignment), typeof(BasicRadioButton));
        public static readonly DependencyProperty ContentMarginProperty = DependencyProperty.Register(nameof(ContentMargin), typeof(Thickness), typeof(BasicRadioButton), new FrameworkPropertyMetadata(new Thickness(0)));
        public static readonly DependencyProperty HighLightBackgroundBrushProperty = DependencyProperty.Register(nameof(HighLightBackgroundBrush), typeof(Brush), typeof(BasicRadioButton));
        public static readonly DependencyProperty HighLightForegroundBrushProperty = DependencyProperty.Register(nameof(HighLightForegroundBrush), typeof(Brush), typeof(BasicRadioButton));
        public static readonly DependencyProperty HighLightBorderBrushProperty = DependencyProperty.Register(nameof(HighLightBorderBrush), typeof(Brush), typeof(BasicRadioButton));
        public static readonly DependencyProperty SelectedBackgroundBrushProperty = DependencyProperty.Register(nameof(SelectedBackgroundBrush), typeof(Brush), typeof(BasicRadioButton));
        public static readonly DependencyProperty MouseOverBorderThicknessProperty = DependencyProperty.Register(nameof(MouseOverBorderThickness), typeof(double), typeof(BasicRadioButton), new FrameworkPropertyMetadata(2.0d));
        public static readonly DependencyProperty PressedBorderThicknessProperty = DependencyProperty.Register(nameof(PressedBorderThickness), typeof(double), typeof(BasicRadioButton), new FrameworkPropertyMetadata(3.0d));

        public CornerRadius CornerRadius { get => (CornerRadius)GetValue(CornerRadiusProperty); set => SetValue(CornerRadiusProperty, value); }
        public double ContentWidth { get => (double)GetValue(ContentWidthProperty); set => SetValue(ContentWidthProperty, value); } 
        public double ContentHeight { get => (double)GetValue(ContentHeightProperty); set => SetValue(ContentHeightProperty, value);  }
        public HorizontalAlignment ContentHorizontalAlignment { get => (HorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty); set => SetValue(ContentHorizontalAlignmentProperty, value);  }
        public VerticalAlignment ContentVerticalAlignment { get => (VerticalAlignment)GetValue(ContentVerticalAlignmentProperty); set => SetValue(ContentVerticalAlignmentProperty, value);  }
        public Thickness ContentMargin { get => (Thickness)GetValue(ContentMarginProperty); set => SetValue(ContentMarginProperty, value); }
        public Brush? HighLightBackgroundBrush { get => (Brush?)GetValue(HighLightBackgroundBrushProperty); set => SetValue(HighLightBackgroundBrushProperty, value); }
        public Brush? HighLightForegroundBrush { get => (Brush?)GetValue(HighLightForegroundBrushProperty); set => SetValue(HighLightForegroundBrushProperty, value); }
        public Brush? HighLightBorderBrush { get => (Brush?)GetValue(HighLightBorderBrushProperty); set => SetValue(HighLightBorderBrushProperty, value); }
        public Brush? SelectedBackgroundBrush { get => (Brush?)GetValue(SelectedBackgroundBrushProperty); set => SetValue(SelectedBackgroundBrushProperty, value); }
        public double MouseOverBorderThickness { get => (double)GetValue(MouseOverBorderThicknessProperty); set => SetValue(MouseOverBorderThicknessProperty, value); }
        public double PressedBorderThickness { get => (double)GetValue(PressedBorderThicknessProperty); set => SetValue(PressedBorderThicknessProperty, value); }
    }
}
