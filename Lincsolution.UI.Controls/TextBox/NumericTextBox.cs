using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace Lincsolution.UI.Controls
{
    /// <summary>
    /// KO: decimal 값을 입력하고 바인딩하는 숫자 전용 텍스트 박스입니다.
    /// EN: A numeric text box that edits and binds a decimal value.
    /// </summary>
    [LocalizedDescription("decimal 값을 입력하고 바인딩하는 숫자 전용 텍스트 박스입니다.", "A numeric text box that edits and binds a decimal value.")]
    public class NumericTextBox : BasicTextBox
    {
        private bool _isUpdatingText;

        static NumericTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(typeof(BasicTextBox)));
        }

        public NumericTextBox()
        {
            PreviewTextInput += OnPreviewTextInput;
            PreviewKeyDown += OnPreviewKeyDown;
            LostKeyboardFocus += OnLostKeyboardFocus;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        /// <summary>
        /// KO: Value 종속성 속성 식별자입니다.
        /// EN: Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(decimal),
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(
                decimal.Zero,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                CoerceValue));

        /// <summary>
        /// KO: Minimum 종속성 속성 식별자입니다.
        /// EN: Identifies the Minimum dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            nameof(Minimum),
            typeof(decimal),
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(decimal.MinValue, OnRangeChanged));

        /// <summary>
        /// KO: Maximum 종속성 속성 식별자입니다.
        /// EN: Identifies the Maximum dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            nameof(Maximum),
            typeof(decimal),
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(decimal.MaxValue, OnRangeChanged));

        /// <summary>
        /// KO: DecimalPlaces 종속성 속성 식별자입니다.
        /// EN: Identifies the DecimalPlaces dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(NumericTextBox),
            new FrameworkPropertyMetadata(2, OnDecimalPlacesChanged),
            value => (int)value is >= 0 and <= 28);

        /// <summary>
        /// KO: 입력된 decimal 값을 가져오거나 설정합니다.
        /// EN: Gets or sets the entered decimal value.
        /// </summary>
        [LocalizedDescription("입력된 decimal 값을 가져오거나 설정합니다.", "Gets or sets the entered decimal value.")]
        public decimal Value
        {
            get => (decimal)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// KO: 허용할 최솟값을 가져오거나 설정합니다.
        /// EN: Gets or sets the minimum allowed value.
        /// </summary>
        [LocalizedDescription("허용할 최솟값을 가져오거나 설정합니다.", "Gets or sets the minimum allowed value.")]
        public decimal Minimum
        {
            get => (decimal)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>
        /// KO: 허용할 최댓값을 가져오거나 설정합니다.
        /// EN: Gets or sets the maximum allowed value.
        /// </summary>
        [LocalizedDescription("허용할 최댓값을 가져오거나 설정합니다.", "Gets or sets the maximum allowed value.")]
        public decimal Maximum
        {
            get => (decimal)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// KO: 값을 확정할 때 적용할 소수 자릿수를 가져오거나 설정합니다.
        /// EN: Gets or sets the decimal places applied when committing the value.
        /// </summary>
        [LocalizedDescription("값을 확정할 때 적용할 소수 자릿수를 가져오거나 설정합니다.", "Gets or sets the decimal places applied when committing the value.")]
        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateText(Value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericTextBox)d).UpdateText((decimal)e.NewValue);
        }

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var textBox = (NumericTextBox)d;
            var value = (decimal)baseValue;

            return textBox.Minimum <= textBox.Maximum
                ? Math.Clamp(value, textBox.Minimum, textBox.Maximum)
                : value;
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ValueProperty);
        }

        private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (NumericTextBox)d;
            textBox.SetCurrentValue(ValueProperty, decimal.Round(textBox.Value, (int)e.NewValue));
            textBox.UpdateText(textBox.Value);
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsPotentialDecimal(CreateProspectiveText(e.Text));
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if(!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText) ||
               e.SourceDataObject.GetData(DataFormats.UnicodeText) is not string pastedText ||
               !IsPotentialDecimal(CreateProspectiveText(pastedText)))
            {
                e.CancelCommand();
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                CommitText();
                e.Handled = true;
            }
        }

        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            CommitText();
        }

        private string CreateProspectiveText(string input)
        {
            var current = Text ?? string.Empty;
            return current.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, input);
        }

        private bool IsPotentialDecimal(string text)
        {
            if(string.IsNullOrEmpty(text) || text == CultureInfo.CurrentCulture.NumberFormat.NegativeSign)
            {
                return true;
            }

            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if(text == separator || text == CultureInfo.CurrentCulture.NumberFormat.NegativeSign + separator)
            {
                return true;
            }

            return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.CurrentCulture, out _);
        }

        private void CommitText()
        {
            if(decimal.TryParse(Text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                   CultureInfo.CurrentCulture, out var parsedValue))
            {
                SetCurrentValue(ValueProperty, decimal.Round(parsedValue, DecimalPlaces));
            }

            UpdateText(Value);
        }

        private void UpdateText(decimal value)
        {
            if(_isUpdatingText)
            {
                return;
            }

            _isUpdatingText = true;
            Text = decimal.Round(value, DecimalPlaces).ToString(CultureInfo.CurrentCulture);
            _isUpdatingText = false;
        }
    }
}
