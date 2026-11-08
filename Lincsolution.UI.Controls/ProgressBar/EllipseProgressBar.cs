using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// Ellipse를 사용해 원형 진행률을 표시하는 ProgressBar 컨트롤입니다.
    /// </summary>
    /// <summary>
    /// KO: 원형으로 진행률과 텍스트를 표시하는 진행률 표시줄입니다.
    /// EN: A circular progress bar that displays progress and text.
    /// </summary>
    [LocalizedDescription("원형으로 진행률과 텍스트를 표시하는 진행률 표시줄입니다.", "A circular progress bar that displays progress and text.")]
    public class EllipseProgressBar : ProgressBar
    {
        private const string IndicatorPartName = "PART_ProgressEllipse";
        private Ellipse? _indicator;

        /// <summary>
        /// KO: EllipseProgressBar 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the EllipseProgressBar control.
        /// </summary>
        static EllipseProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EllipseProgressBar), new FrameworkPropertyMetadata(typeof(EllipseProgressBar)));
        }

        /// <summary>
        /// KO: ProgressThickness 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the ProgressThickness dependency property. Gets or sets the thickness.
        /// </summary>
        public static readonly DependencyProperty ProgressThicknessProperty = DependencyProperty.Register(
            nameof(ProgressThickness),
            typeof(double),
            typeof(EllipseProgressBar),
            new FrameworkPropertyMetadata(25.0, FrameworkPropertyMetadataOptions.AffectsRender, OnProgressVisualPropertyChanged));

        /// <summary>
        /// KO: IsTextBindedByValue 종속성 속성 식별자입니다. 진행률 값으로 텍스트를 자동 생성할지 여부를 가져오거나 설정합니다.
        /// EN: Identifies the IsTextBindedByValue dependency property. Gets or sets whether the text is generated automatically from the progress value.
        /// </summary>
        public static readonly DependencyProperty IsTextBindedByValueProperty = DependencyProperty.Register(
            nameof(IsTextBindedByValue),
            typeof(bool),
            typeof(EllipseProgressBar),
            new PropertyMetadata(false, OnIsTextBindedByValueChanged));

        /// <summary>
        /// KO: TextBindedValue 종속성 속성 식별자입니다. 원형 진행률 표시줄에 표시할 사용자 지정 텍스트를 가져오거나 설정합니다.
        /// EN: Identifies the TextBindedValue dependency property. Gets or sets the custom text displayed by the circular progress bar.
        /// </summary>
        public static readonly DependencyProperty TextBindedValueProperty = DependencyProperty.Register(
            nameof(TextBindedValue),
            typeof(string),
            typeof(EllipseProgressBar),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// KO: TextForeground 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextForeground dependency property. Gets or sets the brush.
        /// </summary>
        public static readonly DependencyProperty TextForegroundProperty = DependencyProperty.Register(
            nameof(TextForeground),
            typeof(Brush),
            typeof(EllipseProgressBar),
            new PropertyMetadata(Brushes.Black));

        /// <summary>
        /// 원형 ProgressBar의 Stroke 두께입니다.
        /// </summary>
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the thickness.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the thickness.")]
        public double ProgressThickness
        {
            get => (double)GetValue(ProgressThicknessProperty);
            set => SetValue(ProgressThicknessProperty, value);
        }

        /// <summary>
        /// true이면 Value 값을 텍스트로 자동 표시합니다.
        /// </summary>
        /// <summary>
        /// KO: 진행률 값으로 텍스트를 자동 생성할지 여부를 가져오거나 설정합니다.
        /// EN: Gets or sets whether the text is generated automatically from the progress value.
        /// </summary>
        [LocalizedDescription("진행률 값으로 텍스트를 자동 생성할지 여부를 가져오거나 설정합니다.", "Gets or sets whether the text is generated automatically from the progress value.")]
        public bool IsTextBindedByValue
        {
            get => (bool)GetValue(IsTextBindedByValueProperty);
            set => SetValue(IsTextBindedByValueProperty, value);
        }

        /// <summary>
        /// 중앙에 표시할 텍스트입니다.
        /// </summary>
        /// <summary>
        /// KO: 원형 진행률 표시줄에 표시할 사용자 지정 텍스트를 가져오거나 설정합니다.
        /// EN: Gets or sets the custom text displayed by the circular progress bar.
        /// </summary>
        [LocalizedDescription("원형 진행률 표시줄에 표시할 사용자 지정 텍스트를 가져오거나 설정합니다.", "Gets or sets the custom text displayed by the circular progress bar.")]
        public string TextBindedValue
        {
            get => (string)GetValue(TextBindedValueProperty);
            set => SetValue(TextBindedValueProperty, value);
        }

        /// <summary>
        /// 중앙 텍스트 Foreground입니다.
        /// </summary>
        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the brush.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the brush.")]
        public Brush TextForeground
        {
            get => (Brush)GetValue(TextForegroundProperty);
            set => SetValue(TextForegroundProperty, value);
        }

        /// <summary>
        /// KO: 컨트롤 템플릿을 적용하고 필요한 템플릿 요소와 표시 상태를 초기화합니다.
        /// EN: Applies the control template and initializes the required template parts and visual state.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_indicator is not null)
            {
                _indicator.SizeChanged -= OnIndicatorSizeChanged;
            }

            base.OnApplyTemplate();

            _indicator = GetTemplateChild(IndicatorPartName) as Ellipse;

            if (_indicator is not null)
            {
                _indicator.SizeChanged += OnIndicatorSizeChanged;
            }

            UpdateIndicatorDashArray();
            UpdateTextBindedValue(Value);
        }

        /// <summary>
        /// KO: 진행률 값 변경을 관련 텍스트와 시각 요소에 반영합니다.
        /// EN: Applies a progress value change to the related text and visual elements.
        /// </summary>
        /// <param name="oldValue">KO: 변경 전 값입니다. EN: The value before the change.</param>
        /// <param name="newValue">KO: 변경 후 값입니다. EN: The value after the change.</param>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            UpdateIndicatorDashArray();
            UpdateTextBindedValue(newValue);
        }

        /// <summary>
        /// KO: 최솟값 변경 후 원형 진행률 표시와 텍스트를 갱신합니다.
        /// EN: Refreshes the circular progress display and text after the minimum changes.
        /// </summary>
        /// <param name="oldMinimum">KO: 변경 전 최솟값입니다. EN: The minimum value before the change.</param>
        /// <param name="newMinimum">KO: 변경 후 최솟값입니다. EN: The minimum value after the change.</param>
        protected override void OnMinimumChanged(double oldMinimum, double newMinimum)
        {
            base.OnMinimumChanged(oldMinimum, newMinimum);
            UpdateIndicatorDashArray();
            UpdateTextBindedValue(Value);
        }

        /// <summary>
        /// KO: 최댓값 변경 후 원형 진행률 표시와 텍스트를 갱신합니다.
        /// EN: Refreshes the circular progress display and text after the maximum changes.
        /// </summary>
        /// <param name="oldMaximum">KO: 변경 전 최댓값입니다. EN: The maximum value before the change.</param>
        /// <param name="newMaximum">KO: 변경 후 최댓값입니다. EN: The maximum value after the change.</param>
        protected override void OnMaximumChanged(double oldMaximum, double newMaximum)
        {
            base.OnMaximumChanged(oldMaximum, newMaximum);
            UpdateIndicatorDashArray();
            UpdateTextBindedValue(Value);
        }

        /// <summary>
        /// KO: 원형 진행률의 시각 속성 변경 시 표시를 갱신합니다.
        /// EN: Refreshes the circular progress display when a visual property changes.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnProgressVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EllipseProgressBar progressBar)
            {
                progressBar.UpdateIndicatorDashArray();
            }
        }

        /// <summary>
        /// KO: 값 기반 텍스트 표시 설정이 변경되면 표시 텍스트를 갱신합니다.
        /// EN: Refreshes the displayed text when value-based text binding changes.
        /// </summary>
        /// <param name="d">KO: 속성 값이 변경된 종속성 개체입니다. EN: The dependency object whose property value changed.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private static void OnIsTextBindedByValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EllipseProgressBar progressBar && progressBar.IsTextBindedByValue)
            {
                progressBar.UpdateTextBindedValue(progressBar.Value);
            }
        }

        /// <summary>
        /// KO: 원형 표시 요소의 크기 변경 시 대시 배열을 다시 계산합니다.
        /// EN: Recalculates the dash array when the circular indicator size changes.
        /// </summary>
        /// <param name="sender">KO: 이벤트를 발생시킨 개체입니다. EN: The object that raised the event.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private void OnIndicatorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateIndicatorDashArray();
        }

        /// <summary>
        /// KO: 현재 크기와 진행률에 맞게 원형 표시 요소의 대시 배열을 갱신합니다.
        /// EN: Updates the circular indicator dash array for the current size and progress.
        /// </summary>
        private void UpdateIndicatorDashArray()
        {
            if (_indicator is null)
            {
                return;
            }

            var thickness = Math.Max(ProgressThickness, 0.1);
            var width = Math.Max(_indicator.ActualWidth - thickness, 0);
            var height = Math.Max(_indicator.ActualHeight - thickness, 0);

            if (width <= 0 || height <= 0)
            {
                _indicator.StrokeDashArray = new DoubleCollection { 0, 1 };
                return;
            }

            var range = Maximum - Minimum;
            var normalizedValue = range <= 0 ? 0 : (Value - Minimum) / range;
            normalizedValue = Math.Clamp(normalizedValue, 0, 1);

            var total = Math.PI * Math.Sqrt((width * width + height * height) / 2) / thickness;
            var value = normalizedValue * total;
            var quarter = total * 0.25;

            _indicator.StrokeDashArray = new DoubleCollection
            {
                Math.Max(0, value - quarter),
                Math.Min(total - quarter, total - value),
                value,
                total
            };
        }

        /// <summary>
        /// KO: 현재 진행률 값으로 표시 텍스트를 갱신합니다.
        /// EN: Updates the displayed text from the current progress value.
        /// </summary>
        /// <param name="newValue">KO: 변경 후 값입니다. EN: The value after the change.</param>
        private void UpdateTextBindedValue(double newValue)
        {
            if (!IsTextBindedByValue)
            {
                return;
            }

            SetCurrentValue(TextBindedValueProperty, $"{newValue.ToString("0.#", CultureInfo.CurrentCulture)}%");
        }
    }
}
