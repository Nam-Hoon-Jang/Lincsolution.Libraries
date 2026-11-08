using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 출력값을 읽기 전용으로 표시하고 기준선을 함께 보여주는 슬라이더입니다.
    /// EN: A read-only slider that displays an output value with reference ticks.
    /// </summary>
    public class BasicSlider : Slider
    {
        static BasicSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicSlider), new FrameworkPropertyMetadata(typeof(BasicSlider)));
        }

        public BasicSlider()
        {
            IsHitTestVisible = false;
            Focusable = false;
        }

        /// <summary>
        /// KO: TrackHeight 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TrackHeight dependency property. Gets or sets the height.
        /// </summary>
        public static readonly DependencyProperty TrackHeightProperty =
            DependencyProperty.Register(nameof(TrackHeight), typeof(double), typeof(BasicSlider), new FrameworkPropertyMetadata(8d));
        /// <summary>
        /// KO: TickBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TickBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TickBrushProperty =
            DependencyProperty.Register(nameof(TickBrush), typeof(Brush), typeof(BasicSlider));
        /// <summary>
        /// KO: IndicatorBrush 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IndicatorBrush dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IndicatorBrushProperty =
            DependencyProperty.Register(nameof(IndicatorBrush), typeof(Brush), typeof(BasicSlider));
        /// <summary>
        /// KO: ValueTextFormat 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ValueTextFormat dependency property. Gets or sets the ValueTextFormat value.
        /// </summary>
        public static readonly DependencyProperty ValueTextFormatProperty =
            DependencyProperty.Register(nameof(ValueTextFormat), typeof(string), typeof(BasicSlider), new FrameworkPropertyMetadata("0.##"));
        /// <summary>
        /// KO: IsValueVisible 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IsValueVisible dependency property. Gets or sets the IsValueVisible value.
        /// </summary>
        public static readonly DependencyProperty IsValueVisibleProperty =
            DependencyProperty.Register(nameof(IsValueVisible), typeof(bool), typeof(BasicSlider), new FrameworkPropertyMetadata(true));

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the height.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the height.")]
        public double TrackHeight { get => (double)GetValue(TrackHeightProperty); set => SetValue(TrackHeightProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? TickBrush { get => (Brush?)GetValue(TickBrushProperty); set => SetValue(TickBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? IndicatorBrush { get => (Brush?)GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the ValueTextFormat value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the ValueTextFormat value.")]
        public string ValueTextFormat { get => (string)GetValue(ValueTextFormatProperty); set => SetValue(ValueTextFormatProperty, value); }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the IsValueVisible value.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the IsValueVisible value.")]
        public bool IsValueVisible { get => (bool)GetValue(IsValueVisibleProperty); set => SetValue(IsValueVisibleProperty, value); }
    }
}
