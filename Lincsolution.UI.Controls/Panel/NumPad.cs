using System;
using System.Windows;
using System.Windows.Controls;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 대상 TextBox에 숫자를 입력하는 숫자 패드입니다.
    /// EN: A numeric keypad that enters values into a target TextBox.
    /// </summary>
    [LocalizedDescription("대상 TextBox에 숫자를 입력하는 숫자 패드입니다.", "A numeric keypad that enters values into a target TextBox.")]
    public class NumPad : Control
    {
        /// <summary>
        /// KO: NumPad 컨트롤의 기본 스타일 메타데이터를 등록합니다.
        /// EN: Registers the default style metadata for the NumPad control.
        /// </summary>
        static NumPad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumPad),
                new FrameworkPropertyMetadata(typeof(NumPad)));
        }

        /// <summary>
        /// KO: NumPad 인스턴스를 초기화하고 숫자 버튼 클릭 처리기를 등록합니다.
        /// EN: Initializes a NumPad instance and registers its numeric button click handler.
        /// </summary>
        public NumPad()
        {
            AddHandler(Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
        }

        /// <summary>
        /// KO: TargetTextBox 종속성 속성 식별자입니다. 숫자 입력을 전달할 TextBox를 가져오거나 설정합니다.
        /// EN: Identifies the TargetTextBox dependency property. Gets or sets the TextBox that receives numeric input.
        /// </summary>
        public static readonly DependencyProperty TargetTextBoxProperty =
            DependencyProperty.Register(
                nameof(TargetTextBox),
                typeof(TextBox),
                typeof(NumPad));

        /// <summary>
        /// KO: ApplyContent 종속성 속성 식별자입니다. 적용 버튼에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Identifies the ApplyContent dependency property. Gets or sets the content displayed by the apply button.
        /// </summary>
        public static readonly DependencyProperty ApplyContentProperty =
            DependencyProperty.Register(
                nameof(ApplyContent),
                typeof(object),
                typeof(NumPad),
                new FrameworkPropertyMetadata("Apply"));

        /// <summary>
        /// KO: 숫자 입력을 전달할 TextBox를 가져오거나 설정합니다.
        /// EN: Gets or sets the TextBox that receives numeric input.
        /// </summary>
        [LocalizedDescription("숫자 입력을 전달할 TextBox를 가져오거나 설정합니다.", "Gets or sets the TextBox that receives numeric input.")]
        public TextBox? TargetTextBox
        {
            get => (TextBox?)GetValue(TargetTextBoxProperty);
            set => SetValue(TargetTextBoxProperty, value);
        }

        /// <summary>
        /// KO: 적용 버튼에 표시할 내용을 가져오거나 설정합니다.
        /// EN: Gets or sets the content displayed by the apply button.
        /// </summary>
        [LocalizedDescription("적용 버튼에 표시할 내용을 가져오거나 설정합니다.", "Gets or sets the content displayed by the apply button.")]
        public object? ApplyContent
        {
            get => GetValue(ApplyContentProperty);
            set => SetValue(ApplyContentProperty, value);
        }

        public event EventHandler<string>? InputRequested;

        /// <summary>
        /// KO: 컨트롤 내부 버튼 클릭을 처리합니다.
        /// EN: Handles a button click raised within the control.
        /// </summary>
        /// <param name="sender">KO: 이벤트를 발생시킨 개체입니다. EN: The object that raised the event.</param>
        /// <param name="e">KO: 이벤트 데이터입니다. EN: The event data.</param>
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is not Button { Tag: string input })
                return;

            string value = input switch
            {
                "BackSpace" => "BackSpace",
                "Dot" => "Dot",
                "ESC" => "ESC",
                "Clear" => "Clear",
                "Apply" => "Apply",
                _ => input
            };

            UpdateTargetTextBox(value);
            InputRequested?.Invoke(this, value);
        }

        /// <summary>
        /// KO: 숫자 패드 입력을 대상 TextBox의 현재 선택 영역에 반영합니다.
        /// EN: Applies numeric keypad input to the current selection in the target TextBox.
        /// </summary>
        /// <param name="input">KO: 처리할 숫자 패드 입력입니다. EN: The numeric keypad input to process.</param>
        private void UpdateTargetTextBox(string input)
        {
            if (TargetTextBox is not { IsReadOnly: false } textBox)
                return;

            switch (input)
            {
                case "BackSpace":
                    if (textBox.SelectionLength > 0)
                    {
                        ReplaceSelection(textBox, string.Empty);
                    }
                    else if (textBox.CaretIndex > 0)
                    {
                        textBox.Select(textBox.CaretIndex - 1, 1);
                        ReplaceSelection(textBox, string.Empty);
                    }
                    break;
                case "ESC":
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
                    SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    return;
                case "Clear":
                    textBox.Clear();
                    break;
                case "Dot":
                    ApplyDotOrMinus(textBox);
                    break;
                case "Apply":
                    textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                    SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                    return;
                default:
                    ReplaceSelection(textBox, input);
                    break;
            }

            textBox.Focus();
        }

        /// <summary>
        /// KO: 대상 TextBox에 소수점 또는 부호 변경을 적용합니다.
        /// EN: Applies a decimal point or sign change to the target TextBox.
        /// </summary>
        /// <param name="textBox">KO: 입력을 변경할 TextBox입니다. EN: The TextBox whose input is modified.</param>
        private static void ApplyDotOrMinus(TextBox textBox)
        {
            string text = textBox.Text;

            if (string.IsNullOrEmpty(text) || text == "0")
            {
                textBox.Text = "-";
                textBox.CaretIndex = 1;
                return;
            }

            if (text == "-")
            {
                textBox.Clear();
                return;
            }

            if (!text.Contains('.'))
            {
                ReplaceSelection(textBox, ".");
                return;
            }

            int caret = textBox.CaretIndex;
            if (text.EndsWith('.'))
            {
                text = text[..^1];
                caret = Math.Min(caret, text.Length);
            }

            if (text.StartsWith('-'))
            {
                textBox.Text = text[1..];
                textBox.CaretIndex = Math.Max(0, caret - 1);
            }
            else
            {
                textBox.Text = $"-{text}";
                textBox.CaretIndex = caret + 1;
            }
        }

        /// <summary>
        /// KO: TextBox의 선택된 문자열을 지정한 값으로 교체하고 캐럿을 이동합니다.
        /// EN: Replaces the selected text in a TextBox and moves the caret.
        /// </summary>
        /// <param name="textBox">KO: 입력을 변경할 TextBox입니다. EN: The TextBox whose input is modified.</param>
        /// <param name="value">KO: 삽입하거나 기본값으로 사용할 값입니다. EN: The value to insert or use as the default.</param>
        private static void ReplaceSelection(TextBox textBox, string value)
        {
            int start = textBox.SelectionStart;
            textBox.SelectedText = value;
            textBox.CaretIndex = start + value.Length;
        }
    }
}
