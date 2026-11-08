using System.Windows;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 테두리와 모서리 반경을 지원하는 읽기 전용 텍스트 컨트롤입니다.
    /// EN: A read-only text control supporting borders and corner radius.
    /// </summary>
    [LocalizedDescription("테두리와 모서리 반경을 지원하는 읽기 전용 텍스트 컨트롤입니다.", "A read-only text control supporting borders and corner radius.")]
    public class BasicTextBlock : Control
    {
        /// <summary>
        /// KO: BasicTextBlock 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the BasicTextBlock control.
        /// </summary>
        static BasicTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(typeof(BasicTextBlock)));
        }

        /// <summary>
        /// KO: Text 종속성 속성 식별자입니다. 표시할 텍스트를 가져오거나 설정합니다.
        /// EN: Identifies the Text dependency property. Gets or sets the text to display.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(string.Empty));

        /// <summary>
        /// KO: CornerRadius 종속성 속성 식별자입니다. 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Identifies the CornerRadius dependency property. Gets or sets the corner radius of the control.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(new CornerRadius(0)));

        /// <summary>
        /// KO: TextAlignment 종속성 속성 식별자입니다. 텍스트의 가로 정렬 방식을 가져오거나 설정합니다.
        /// EN: Identifies the TextAlignment dependency property. Gets or sets the horizontal alignment of the text.
        /// </summary>
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(
                nameof(TextAlignment),
                typeof(TextAlignment),
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(TextAlignment.Left));

        /// <summary>
        /// KO: TextWrapping 종속성 속성 식별자입니다. 텍스트 줄 바꿈 방식을 가져오거나 설정합니다.
        /// EN: Identifies the TextWrapping dependency property. Gets or sets how text wrapping is performed.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register(
                nameof(TextWrapping),
                typeof(TextWrapping),
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(TextWrapping.NoWrap));

        /// <summary>
        /// KO: TextTrimming 종속성 속성 식별자입니다. 공간이 부족할 때 텍스트를 자르는 방식을 가져오거나 설정합니다.
        /// EN: Identifies the TextTrimming dependency property. Gets or sets how text is trimmed when space is insufficient.
        /// </summary>
        public static readonly DependencyProperty TextTrimmingProperty =
            DependencyProperty.Register(
                nameof(TextTrimming),
                typeof(TextTrimming),
                typeof(BasicTextBlock),
                new FrameworkPropertyMetadata(TextTrimming.None));

        /// <summary>
        /// KO: 표시할 텍스트를 가져오거나 설정합니다.
        /// EN: Gets or sets the text to display.
        /// </summary>
        [LocalizedDescription("표시할 텍스트를 가져오거나 설정합니다.", "Gets or sets the text to display.")]
        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        /// <summary>
        /// KO: 컨트롤 모서리의 반경을 가져오거나 설정합니다.
        /// EN: Gets or sets the corner radius of the control.
        /// </summary>
        [LocalizedDescription("컨트롤 모서리의 반경을 가져오거나 설정합니다.", "Gets or sets the corner radius of the control.")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// KO: 텍스트의 가로 정렬 방식을 가져오거나 설정합니다.
        /// EN: Gets or sets the horizontal alignment of the text.
        /// </summary>
        [LocalizedDescription("텍스트의 가로 정렬 방식을 가져오거나 설정합니다.", "Gets or sets the horizontal alignment of the text.")]
        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        /// <summary>
        /// KO: 텍스트 줄 바꿈 방식을 가져오거나 설정합니다.
        /// EN: Gets or sets how text wrapping is performed.
        /// </summary>
        [LocalizedDescription("텍스트 줄 바꿈 방식을 가져오거나 설정합니다.", "Gets or sets how text wrapping is performed.")]
        public TextWrapping TextWrapping
        {
            get => (TextWrapping)GetValue(TextWrappingProperty);
            set => SetValue(TextWrappingProperty, value);
        }

        /// <summary>
        /// KO: 공간이 부족할 때 텍스트를 자르는 방식을 가져오거나 설정합니다.
        /// EN: Gets or sets how text is trimmed when space is insufficient.
        /// </summary>
        [LocalizedDescription("공간이 부족할 때 텍스트를 자르는 방식을 가져오거나 설정합니다.", "Gets or sets how text is trimmed when space is insufficient.")]
        public TextTrimming TextTrimming
        {
            get => (TextTrimming)GetValue(TextTrimmingProperty);
            set => SetValue(TextTrimmingProperty, value);
        }
    }
}
