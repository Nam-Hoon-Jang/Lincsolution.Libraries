using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: 숫자, 일반 문자 또는 이메일 문자만 입력할 수 있는 BasicTextBox입니다.
    /// EN: A BasicTextBox that accepts numeric, text, or email characters.
    /// </summary>
    public class ValidationTextBox : BasicTextBox
    {
        private static readonly Regex NumericPattern = new(@"^-?\d+(?:\.\d*)?$|^-?$", RegexOptions.Compiled);
        private static readonly Regex TextPattern = new(@"^\p{L}*$", RegexOptions.Compiled);
        private static readonly Regex EmailPattern = new(@"^[\p{L}\p{N}@.]*$", RegexOptions.Compiled);

        static ValidationTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationTextBox), new FrameworkPropertyMetadata(typeof(BasicTextBox)));
        }

        public ValidationTextBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        public static readonly DependencyProperty ValidationModeProperty =
            DependencyProperty.Register(nameof(ValidationMode), typeof(InputValidationMode), typeof(ValidationTextBox),
                new FrameworkPropertyMetadata(InputValidationMode.NumOnly));

        [LocalizedDescription("허용할 입력 형식을 가져오거나 설정합니다.", "Gets or sets the accepted input format.")]
        public InputValidationMode ValidationMode
        {
            get => (InputValidationMode)GetValue(ValidationModeProperty);
            set => SetValue(ValidationModeProperty, value);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsValid(CreateProspectiveText(e.Text));
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText) ||
                e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string pastedText ||
                !IsValid(CreateProspectiveText(pastedText)))
            {
                e.CancelCommand();
            }
        }

        private string CreateProspectiveText(string input)
        {
            string current = Text ?? string.Empty;
            return current.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, input);
        }

        private bool IsValid(string value) => ValidationMode switch
        {
            InputValidationMode.NumOnly => NumericPattern.IsMatch(value),
            InputValidationMode.TextOnly => TextPattern.IsMatch(value),
            InputValidationMode.EmailOnly => EmailPattern.IsMatch(value) && value.Count(c => c == '@') <= 1,
            _ => false
        };
    }
}
