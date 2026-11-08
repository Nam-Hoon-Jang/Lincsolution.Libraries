using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 알람 발생 여부와 아이콘 상태를 표시하는 토글 버튼입니다.
    /// EN: A toggle button that displays the alarm and icon state.
    /// </summary>
    [LocalizedDescription("알람 발생 여부와 아이콘 상태를 표시하는 토글 버튼입니다.", "A toggle button that displays the alarm and icon state.")]
    public class AlarmButton : ToggleButton
    {
        /// <summary>
        /// KO: AlarmButton 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the AlarmButton control.
        /// </summary>
        static AlarmButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AlarmButton), new FrameworkPropertyMetadata(typeof(AlarmButton)));
        }
        /// <summary>
        /// KO: IsAlarmOn 종속성 속성 식별자입니다. 알람 활성 상태를 가져오거나 설정합니다.
        /// EN: Identifies the IsAlarmOn dependency property. Gets or sets whether the alarm is active.
        /// </summary>
        public static readonly DependencyProperty IsAlarmOnProperty = DependencyProperty.Register(nameof(IsAlarmOn), typeof(bool), typeof(AlarmButton));
        /// <summary>
        /// KO: IconSize 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconSize dependency property. Gets or sets the size.
        /// </summary>
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(AlarmButton));
        /// <summary>
        /// KO: IconFill 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconFill dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconFillProperty = DependencyProperty.Register(nameof(IconFill), typeof(Brush), typeof(AlarmButton));
        /// <summary>
        /// KO: IconStroke 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStroke dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty IconStrokeProperty = DependencyProperty.Register(nameof(IconStroke), typeof(Brush), typeof(AlarmButton));
        /// <summary>
        /// KO: IconStrokeThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the IconStrokeThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty IconStrokeThicknessProperty = DependencyProperty.Register(nameof(IconStrokeThickness), typeof(double), typeof(AlarmButton));
        /// <summary>
        /// KO: 알람 활성 상태를 가져오거나 설정합니다.
        /// EN: Gets or sets whether the alarm is active.
        /// </summary>
        [LocalizedDescription("알람 활성 상태를 가져오거나 설정합니다.", "Gets or sets whether the alarm is active.")]
        public bool? IsAlarmOn
        {
            get => (bool?)GetValue(IsAlarmOnProperty);
            set => SetValue(IsAlarmOnProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the size.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the size.")]
        public double? IconSize
        {
            get => (double?)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? IconFill
        {
            get => (Brush?)GetValue(IconFillProperty);
            set => SetValue(IconFillProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush? IconStroke
        {
            get => (Brush?)GetValue(IconStrokeProperty);
            set => SetValue(IconStrokeProperty, value);
        }
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double IconStrokeThickness
        {
            get => (double)GetValue(IconStrokeThicknessProperty);
            set => SetValue(IconStrokeThicknessProperty, value);
        }
    }
}
