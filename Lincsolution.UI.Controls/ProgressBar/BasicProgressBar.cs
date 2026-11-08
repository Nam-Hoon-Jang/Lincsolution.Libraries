using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 진행률과 사용자 지정 텍스트를 함께 표시하는 진행률 표시줄입니다.
    /// EN: A progress bar that displays progress and customizable text.
    /// </summary>
    [LocalizedDescription("진행률과 사용자 지정 텍스트를 함께 표시하는 진행률 표시줄입니다.", "A progress bar that displays progress and customizable text.")]
    public class BasicProgressBar : ProgressBar
    {
        /// <summary>
        /// KO: BasicProgressBar 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicProgressBar control.
        /// </summary>
        static BasicProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BasicProgressBar), new FrameworkPropertyMetadata(typeof(BasicProgressBar)));
        }

        /// <summary>
        /// KO: TextBlockVisibility 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockVisibility dependency property. Gets or sets the visibility.
        /// </summary>
        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 진행률 표시줄 모서리 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the progress bar corner radius.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(BasicProgressBar),
                new FrameworkPropertyMetadata(new CornerRadius(0)));
        public static readonly DependencyProperty TextBlockVisibilityProperty =
            DependencyProperty.Register(nameof(TextBlockVisibility),
            typeof(Visibility),
            typeof(BasicProgressBar),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// KO: TextBlockMargin 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockMargin dependency property. Gets or sets the margin.
        /// </summary>
        public static readonly DependencyProperty TextBlockMarginProperty =
            DependencyProperty.Register(nameof(TextBlockMargin),
            typeof(Thickness),
            typeof(BasicProgressBar), 
            new FrameworkPropertyMetadata(new Thickness(0,0,0,0)));

        /// <summary>
        /// KO: TextBlockHorizontalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockHorizontalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty TextBlockHorizontalAlignmentProperty =
            DependencyProperty.Register(nameof(TextBlockHorizontalAlignment),
            typeof(HorizontalAlignment),
            typeof(BasicProgressBar),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left));

        /// <summary>
        /// KO: TextBlockVerticalAlignment 종속성 속성 식별자입니다.  가져오거나 설정합니다.
        /// EN: Identifies the TextBlockVerticalAlignment dependency property. Gets or sets the alignment.
        /// </summary>
        public static readonly DependencyProperty TextBlockVerticalAlignmentProperty =
            DependencyProperty.Register(nameof(TextBlockVerticalAlignment),
            typeof(VerticalAlignment),
            typeof(BasicProgressBar),
            new FrameworkPropertyMetadata(VerticalAlignment.Center));

        /// <summary>
        /// KO: TextValue 종속성 속성 식별자입니다. 진행률 표시줄에 표시할 텍스트를 가져오거나 설정합니다.
        /// EN: Identifies the TextValue dependency property. Gets or sets the text displayed on the progress bar.
        /// </summary>
        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register(nameof(TextValue),
            typeof(string),
            typeof(BasicProgressBar),
            new PropertyMetadata(string.Empty));

        /// <summary>
        /// KO: 진행률 값 변경을 관련 텍스트와 시각 요소에 반영합니다.
        /// EN: Applies a progress value change to the related text and visual elements.
        /// </summary>
        /// <param name="oldValue">KO: 변경 전 값입니다. EN: The value before the change.</param>
        /// <param name="newValue">KO: 변경 후 값입니다. EN: The value after the change.</param>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            SetCurrentValue(TextValueProperty, $"{newValue.ToString("0.#", CultureInfo.CurrentCulture)}%");
        }

        /// <summary>
        /// KO: 진행률 표시줄 모서리 반경을 가져오거나 설정합니다.
        /// EN: Gets or sets the progress bar corner radius.
        /// </summary>
        [LocalizedDescription("진행률 표시줄 모서리 반경을 가져오거나 설정합니다.", "Gets or sets the progress bar corner radius.")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// KO: 텍스트 표시 상태를 가져오거나 설정합니다.
        /// EN: Gets or sets the text visibility.
        /// </summary>
        [LocalizedDescription("텍스트 표시 상태를 가져오거나 설정합니다.", "Gets or sets the text visibility.")]
        public Visibility TextBlockVisibility
        {
            get => (Visibility)GetValue(TextBlockVisibilityProperty);
            set => SetValue(TextBlockVisibilityProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the margin.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the margin.")]
        public Thickness TextBlockMargin
        {
            get => (Thickness)GetValue(TextBlockMarginProperty);
            set => SetValue(TextBlockMarginProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public HorizontalAlignment TextBlockHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(TextBlockHorizontalAlignmentProperty);
            set => SetValue(TextBlockHorizontalAlignmentProperty, value);
        }

        /// <summary>
        /// KO:  가져오거나 설정합니다.
        /// EN: Gets or sets the alignment.
        /// </summary>
        [LocalizedDescription(" 가져오거나 설정합니다.", "Gets or sets the alignment.")]
        public VerticalAlignment TextBlockVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(TextBlockVerticalAlignmentProperty);
            set => SetValue(TextBlockVerticalAlignmentProperty, value);
        }
        

        /// <summary>
        /// KO: 진행률 표시줄에 표시할 텍스트를 가져오거나 설정합니다.
        /// EN: Gets or sets the text displayed on the progress bar.
        /// </summary>
        [LocalizedDescription("진행률 표시줄에 표시할 텍스트를 가져오거나 설정합니다.", "Gets or sets the text displayed on the progress bar.")]
        public string TextValue
        {
            get => (string)GetValue(TextValueProperty);
            set => SetValue(TextValueProperty, value);
        }
    }
}
