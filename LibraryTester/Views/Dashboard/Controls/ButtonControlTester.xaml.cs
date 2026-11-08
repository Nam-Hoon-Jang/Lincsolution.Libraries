using Lincsolution.UI.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace LibraryTester.Views.Dashboard.Controls
{
    public partial class ButtonControlTester : UserControl
    {
        private static readonly string[] CommonProperties =
        {
            nameof(FrameworkElement.Width),
            nameof(FrameworkElement.Height),
            nameof(UIElement.IsEnabled),
            nameof(Control.Background),
            nameof(Control.Foreground),
            nameof(Control.BorderBrush),
            nameof(Control.BorderThickness),
            nameof(Control.FontSize),
            nameof(Control.FontWeight),
            nameof(FrameworkElement.HorizontalAlignment),
            nameof(FrameworkElement.VerticalAlignment),
            nameof(FrameworkElement.Margin)
        };

        private readonly IReadOnlyList<ControlDefinition> definitions;
        private ControlDefinition? selectedDefinition;
        private IReadOnlyList<FrameworkElement> previewControls = Array.Empty<FrameworkElement>();

        private static readonly string[] GeometryIconKeys =
        {
            "Geom.Close", "Geom.ManualMode", "Geom.Preference", "Geom.LogList", "Geom.BuildParameter",
            "Geom.Page", "Geom.Pause", "Geom.Play", "Geom.Photo"
        };

        private static readonly string[] ImageIconKeys =
        {
            "Icon.EmergencyIdle", "Icon.EmergencyPressed", "Icon.Info", "Icon.Warning", "Icon.Error"
        };

        public ButtonControlTester()
            : this(TesterCategory.Buttons)
        {
        }

        internal ButtonControlTester(TesterCategory category)
        {
            InitializeComponent();

            definitions = category switch
            {
                TesterCategory.Inputs => CreateInputDefinitions(),
                TesterCategory.Display => CreateDisplayDefinitions(),
                _ => CreateButtonDefinitions()
            };
            ControlList.ItemsSource = definitions;
            ControlList.SelectedIndex = 0;
        }

        private static IReadOnlyList<ControlDefinition> CreateButtonDefinitions()
        {
            Geometry sampleIcon = Geometry.Parse("M 12,2 A 10,10 0 1 1 11.99,2 M 12,7 L 12,13 M 12,17 L 12,18");

            return new[]
            {
                DefineSingle("AlarmButton",
                    () => new AlarmButton { Width = 88, Height = 48 },
                    new[] { "IsChecked", "IsAlarmOn", "IconSize", "IconFill", "IconStroke", "IconStrokeThickness" }),

                DefineSingle("BasicButton",
                    () => new BasicButton { Content = "Basic Button" },
                    new[]
                    {
                        "Content", "CornerRadius", "ContentHorizontalAlignment", "ContentVerticalAlignment", "ContentMargin",
                        "HighLightBackgroundBrush", "HighLightForegroundBrush", "HighLightBorderBrush", "HighLightBorderThickness",
                        "DisabledBackgroundBrush", "DisabledForegroundBrush", "DisabledBorderBrush", "DisabledBorderThickness"
                    }),

                Define("BasicRadioButton",
                    () => new Control[]
                    {
                        new BasicRadioButton { Content = "Button 1", Width = 150, Height = 48, GroupName = "Tester.BasicRadioButton" },
                        new BasicRadioButton { Content = "Button 2", Width = 150, Height = 48, GroupName = "Tester.BasicRadioButton" }
                    },
                    new[]
                    {
                        "Content", "IsChecked", "CornerRadius", "ContentWidth", "ContentHeight", "ContentHorizontalAlignment",
                        "ContentVerticalAlignment", "ContentMargin", "HighLightBackgroundBrush", "HighLightForegroundBrush",
                        "HighLightBorderBrush", "SelectedBackgroundBrush", "MouseOverBorderThickness", "PressedBorderThickness"
                    }),

                DefineSingle("BasicToggleButton",
                    () => new BasicToggleButton { Width = 64, Height = 32 },
                    new[]
                    {
                        "IsChecked", "CornerRadius", "CheckedBackground", "UncheckedBackground", "ThumbBrush",
                        "CheckedThumbBrush", "UncheckedThumbBrush", "ThumbSize", "ThumbMargin"
                    }),

                Define("BorderHighlightRadioButton",
                    () => new Control[]
                    {
                        new BorderHighlightRadioButton { Content = "Button 1", Width = 160, Height = 64, GroupName = "Tester.BorderHighlightRadioButton" },
                        new BorderHighlightRadioButton { Content = "Button 2", Width = 160, Height = 64, GroupName = "Tester.BorderHighlightRadioButton" }
                    },
                    new[]
                    {
                        "Content", "IsChecked", "CornerRadius", "ContentWidth", "ContentHeight", "ContentHorizontalAlignment",
                        "ContentVerticalAlignment", "ContentMargin", "HighLightBackgroundBrush", "HighLightForegroundBrush",
                        "HighLightBorderBrush", "SelectedBackgroundBrush", "HighlightBorderThickness"
                    }),

                Define("CollapsibleRectangleIconRadioButton",
                    () => new Control[]
                    {
                        new CollapsibleRectangleIconRadioButton { Content = "Button 1", Icon = sampleIcon, GroupName = "Tester.CollapsibleRectangleIconRadioButton" },
                        new CollapsibleRectangleIconRadioButton { Content = "Button 2", Icon = sampleIcon, GroupName = "Tester.CollapsibleRectangleIconRadioButton" }
                    },
                    new[]
                    {
                        "Content", "IsChecked", "IsCollapsed", "ExpandedWidth", "CollapsedWidth", "Icon", "IconSize",
                        "IconFill", "IconStroke", "IconStrokeThickness", "IconExpandedHorizontalAlignment",
                        "IconExpandedVerticalAlignment", "IconCollapsedHorizontalAlignment", "IconCollapsedVerticalAlignment",
                        "IconExpandedMargin", "IconCollapsedMargin", "ContentWidth", "ContentHeight",
                        "ContentExpandedMargin", "ContentCollapsedMargin", "ContentExpandedHorizontalAlignment",
                        "ContentExpandedVerticalAlignment", "ContentCollapsedHorizontalAlignment", "ContentCollapsedVerticalAlignment",
                        "ContentExpandedFontSize", "ContentCollapsedFontSize"
                    }),

                DefineSingle("EmergencyToggleButton",
                    () => new EmergencyToggleButton { IdleContent = "IDLE", PressedContent = "STOP" },
                    new[]
                    {
                        "IsChecked", "IdleIcon", "PressedIcon", "IdleContent", "PressedContent", "IconWidth", "IconHeight", "IconMargin",
                        "ContentWidth", "ContentHeight", "ContentMargin"
                    }),

                DefineSingle("IconButton",
                    () => new IconButton { Content = "Icon Button", Icon = sampleIcon, Width = 150, Height = 48 },
                    new[]
                    {
                        "Content", "Icon", "IconWidth", "IconHeight", "IconFill", "IconStroke", "IconStrokeThickness",
                        "IconHorizontalAlignment", "IconVerticalAlignment", "IconMargin", "ContentMargin", "CornerRadius"
                    }),

                DefineSingle("ThemeToggleButton",
                    () => new ThemeToggleButton(),
                    new[]
                    {
                        "IsChecked", "ToggleBoxWidth", "ToggleBoxHeight", "ToggleBoxFill", "ToggleBoxStroke",
                        "ToggleBoxStrokeThickness", "IconSize", "IconLeftPos", "IconRightPos", "IconTextLeftPos",
                        "IconTextRightPos", "TextBlockWidth", "TextBlockHeight"
                    })
            };
        }

        private static IReadOnlyList<ControlDefinition> CreateInputDefinitions()
        {
            string[] items = { "Alpha", "Beta", "Gamma", "Delta" };

            return new[]
            {
                DefineSingle("BasicTextBox",
                    () => new BasicTextBox { Width = 260, Height = 42, Watermark = "Enter text" },
                    new[] { "Text", "CornerRadius", "Watermark", "WatermarkFontSize", "WatermarkFontWeight", "WatermarkFontStyle", "WatermarkForeground", "WatermarkMargin", "Padding", "TextAlignment" }),

                DefineSingle("NumericTextBox",
                    () => new NumericTextBox { Width = 260, Height = 42, Value = 25.5m, Minimum = 0m, Maximum = 100m },
                    new[] { "Value", "Minimum", "Maximum", "DecimalPlaces", "CornerRadius", "Watermark", "Padding", "TextAlignment" }),

                DefineSingle("ValidationTextBox",
                    () => new ValidationTextBox { Width = 260, Height = 42, Watermark = "Validated input" },
                    new[] { "Text", "ValidationMode", "CornerRadius", "Watermark", "WatermarkForeground", "Padding", "TextAlignment" }),

                DefineSingle("BasicComboBox",
                    () => new BasicComboBox { Width = 260, Height = 42, ItemsSource = items, SelectedIndex = 0 },
                    new[] { "SelectedIndex", "IsDropDownOpen", "CornerRadius", "DropDownButtonWidth", "IconSize", "IconStroke", "IconStrokeThickness", "IconMargin", "Padding" }),

                DefineSingle("BasicCheckBox",
                    () => new BasicCheckBox { Content = "Check option", Width = 180, Height = 42 },
                    new[] { "Content", "IsChecked", "IsThreeState", "CornerRadius", "BoxSize", "CheckedBackground", "CheckBrush", "StrokeThickness", "CheckArrowSize", "ContentSpacing" }),

                DefineSingle("BasicSlider",
                    () => new BasicSlider { Width = 320, Minimum = 0, Maximum = 100, Value = 40 },
                    new[] { "Value", "Minimum", "Maximum", "SmallChange", "LargeChange", "TickFrequency", "IsSnapToTickEnabled", "TrackHeight", "TickBrush", "IndicatorBrush", "ValueTextFormat", "IsValueVisible" }),

                Define("NumPad",
                    () =>
                    {
                        var target = new BasicTextBox { Width = 180, Height = 42, Text = "0" };
                        return new FrameworkElement[] { target, new NumPad { Width = 300, Height = 360, TargetTextBox = target } };
                    },
                    new[] { "Text", "ApplyContent", "Width", "Height", "IsEnabled" }),

                DefineSingle("ToggleButtonPanel",
                    () => new ToggleButtonPanel { Width = 520, Height = 52, SubjectText = "Enabled", UnitText = "State" },
                    new[] { "SubjectText", "UnitText", "IsChecked", "CornerRadius", "SubjectWidth", "UnitWidth", "ToggleWidth", "ToggleHeight", "ToggleCornerRadius", "CheckedBackground", "UncheckedBackground", "ThumbBrush", "ThumbSize" }),

                DefineSingle("TextBoxPanel",
                    () => new TextBoxPanel { Width = 620, Height = 52, SubjectText = "Position", UnitText = "mm", Watermark = "0.00" },
                    new[] { "SubjectText", "UnitText", "Text", "ValidationMode", "CornerRadius", "SubjectWidth", "UnitWidth", "TextBoxWidth", "TextBoxHeight", "TextBoxCornerRadius", "TextBoxBackground", "TextBoxForeground", "TextBoxBorderBrush", "TextBoxBorderThickness", "TextBoxPadding", "Watermark", "WatermarkForeground" }),

                DefineSingle("TextBoxButtonPanel",
                    () => new TextBoxButtonPanel { Width = 620, Height = 52, FrontText = "File", MidText = "sample.txt", ButtonContentText = "Browse" },
                    new[] { "FrontText", "MidText", "ButtonContentText", "FrontTextWidth", "MidTextWidth", "EndButtonWidth", "FrontTextFontSize", "MidTextFontSize", "ButtonContentFontSize", "FrontTextAlignment", "MidTextAlignment", "MidTextCornerRadius", "ButtonCornerRadius", "Watermark", "WatermarkForeground" })
            };
        }

        private static IReadOnlyList<ControlDefinition> CreateDisplayDefinitions()
        {
            string[] rows = Enumerable.Range(1, 12).Select(index => $"Sample item {index:00}").ToArray();

            return new[]
            {
                DefineSingle("BasicTextBlock",
                    () => new BasicTextBlock { Text = "Basic Text Block", Width = 300, Height = 54 },
                    new[] { "Text", "CornerRadius", "TextAlignment", "TextWrapping", "TextTrimming", "Padding" }),

                DefineSingle("BasicLabel",
                    () => new BasicLabel { Content = "Basic Label", Width = 260, Height = 54 },
                    new[] { "Content", "CornerRadius", "IconImage", "IconSize", "IconLeftPos", "TextBlockLeftPos", "TextBlockWidth", "TextBlockHeight" }),

                DefineSingle("StatusLabel",
                    () => new StatusLabel { Width = 260, Height = 54 },
                    new[] { "Status", "CornerRadius", "IconImage", "IconSize", "IdleText", "BusyText", "ReadyText", "BuildText", "PauseText", "DoneText", "StopText", "WarningText", "ErrorText" }),

                DefineSingle("AlarmLabel",
                    () => new AlarmLabel { Width = 260, Height = 54 },
                    new[] { "Status", "CornerRadius", "IconImage", "IconSize", "IdleText", "WarningText", "ErrorText" }),

                DefineSingle("BasicProgressBar",
                    () => new BasicProgressBar { Width = 360, Height = 36, Minimum = 0, Maximum = 100, Value = 65 },
                    new[] { "Value", "Minimum", "Maximum", "Orientation", "IsIndeterminate", "CornerRadius", "TextBlockVisibility", "TextBlockMargin", "TextBlockHorizontalAlignment", "TextBlockVerticalAlignment", "TextValue" }),

                DefineSingle("EllipseProgressBar",
                    () => new EllipseProgressBar { Width = 150, Height = 150, Minimum = 0, Maximum = 100, Value = 65 },
                    new[] { "Value", "Minimum", "Maximum", "IsIndeterminate", "ProgressThickness", "IsTextBindedByValue", "TextBindedValue", "TextForeground" }),

                DefineSingle("TabView",
                    () => new TabView { Width = 620, Height = 180, Title = "Tab View", UseButton1 = true, UseButton2 = true, UseButton3 = true, Button1Content = "One", Button2Content = "Two", Button3Content = "Three" },
                    new[] { "Title", "TitleFontSize", "UseButton1", "UseButton2", "UseButton3", "IsButton1Enabled", "IsButton2Enabled", "IsButton3Enabled", "Button1Content", "Button2Content", "Button3Content", "TitleWidth", "ButtonWidth", "ButtonSpacing", "HighlightBorderThickness", "TitleBorderThickness" }),

                DefineSingle("StatusGrid",
                    () => new StatusGrid { Width = 420, Height = 180 },
                    new[] { "Status", "Background", "Opacity" }),

                DefineSingle("OpacityAdjustableStatusGrid",
                    () => new OpacityAdjustableStatusGrid { Width = 420, Height = 180 },
                    new[] { "Status", "Background", "Opacity" }),

                DefineSingle("CoordinateMap",
                    () => new CoordinateMap { Width = 420, Height = 420, RealPosition = new Point(100, 80), TargetPosition = new Point(-120, -60) },
                    new[] { "RealPosition", "TargetPosition", "GridSpacing", "LogicalMinimum", "LogicalMaximum", "GridLineBrush", "GridLineThickness", "OriginBrush", "OriginDotSize", "RealDotSize", "RealDotFill", "RealDotStroke", "RealDotStrokeThickness", "TargetDotSize", "TargetDotFill", "TargetDotStroke", "TargetDotStrokeThickness", "TrailDuration", "CornerRadius" }),

                DefineSingle("BasicListView",
                    () => new BasicListView { Width = 520, Height = 360, ItemsSource = rows, Header = "Sample Items", IsCheckBoxVisible = true },
                    new[] { "Header", "EmptyContent", "SelectedIndex", "IsCheckBoxVisible", "IsSelectAllChecked", "ItemHeight", "CornerRadius", "HeaderHeight" }),

                DefineSingle("PaginationControl",
                    () => new PaginationControl { Width = 520, CurrentPage = 1, PageCount = 8 },
                    new[] { "CurrentPage", "PageCount", "ButtonSize", "ItemSpacing" }),

                DefineSingle("BasicPopup",
                    () => new BasicPopup { PopupContent = "Popup visual test", IsSingleButton = false },
                    new[] { "AlarmType", "IsSingleButton", "PopupContent", "ButtonWidth", "ButtonHeight", "TitleFontSize", "ContentFontSize", "ButtonSpacing", "AccentHeight", "PopupWidth", "PopupHeight", "OverlayBackground", "OverlayWidth", "OverlayHeight" }),

                DefineSingle("AlarmListPopup",
                    () => new AlarmListPopup { ItemsSource = rows.Take(5).ToArray(), ClearContent = "Clear All", Width = 520, Height = 420 },
                    new[] { "ClearContent", "CornerRadius", "HeaderHeight", "ItemHeight", "Width", "Height" })
            };
        }

        private static ControlDefinition Define(string name, Func<IReadOnlyList<FrameworkElement>> factory, IEnumerable<string> specificProperties) =>
            new(name, factory, CommonProperties.Concat(specificProperties).Distinct().ToArray());

        private static ControlDefinition DefineSingle(string name, Func<FrameworkElement> factory, IEnumerable<string> specificProperties) =>
            Define(name, () => new[] { factory() }, specificProperties);

        private void OnControlSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ControlList.SelectedItem is ControlDefinition definition)
            {
                ShowDefinition(definition);
            }
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            if(selectedDefinition is not null)
            {
                ShowDefinition(selectedDefinition);
            }
        }

        private void ShowDefinition(ControlDefinition definition)
        {
            selectedDefinition = definition;
            previewControls = definition.Factory();

            PreviewTitle.Text = definition.Name;
            PreviewHost.Children.Clear();

            var previewPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            for(int index = 0; index < previewControls.Count; index++)
            {
                FrameworkElement control = previewControls[index];
                control.HorizontalAlignment = HorizontalAlignment.Center;
                control.VerticalAlignment = VerticalAlignment.Center;
                previewPanel.Children.Add(control is BasicPopup or AlarmListPopup
                    ? CreatePopupLauncher(control, definition.Name)
                    : control);

                if(index < previewControls.Count - 1)
                    previewPanel.Children.Add(new Border { Width = 24 });
            }

            PreviewHost.Children.Add(previewPanel);

            foreach(FrameworkElement control in previewControls)
            {
                if(control is Control templateControl)
                    templateControl.ApplyTemplate();
            }

            TargetSelector.ItemsSource = previewControls
                .Select((control, index) => new PreviewTarget(
                    previewControls.Select(item => item.GetType()).Distinct().Count() == 1 && previewControls.Count > 1
                        ? $"Button {index + 1}"
                        : control.GetType().Name,
                    control))
                .ToArray();
            TargetSelector.Visibility = previewControls.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
            TargetSelector.SelectedIndex = -1;
            TargetSelector.SelectedIndex = 0;
        }

        private static FrameworkElement CreatePopupLauncher(FrameworkElement popup, string name)
        {
            var button = new Button
            {
                Content = $"Open {name}",
                Width = 180,
                Height = 48
            };

            button.Click += (_, _) =>
            {
                if(popup.Parent is not null)
                    return;

                var window = new Window
                {
                    Title = $"{name} Test",
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Width = 900,
                    Height = 650,
                    Content = popup
                };
                window.Closed += (_, _) => window.Content = null;
                window.ShowDialog();
            };

            return button;
        }

        private void OnTargetSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(TargetSelector.SelectedItem is PreviewTarget target && selectedDefinition is not null)
                BuildPropertyEditors(target.Control, selectedDefinition.PropertyNames);
        }

        private void BuildPropertyEditors(FrameworkElement target, IEnumerable<string> propertyNames)
        {
            PropertyPanel.Children.Clear();

            foreach(string propertyName in propertyNames)
            {
                PropertyInfo? property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);

                if(property is null || !property.CanRead || !property.CanWrite || property.GetIndexParameters().Length != 0)
                    continue;

                PropertyPanel.Children.Add(CreatePropertyRow(target, property));
            }
        }

        private static FrameworkElement CreatePropertyRow(FrameworkElement target, PropertyInfo property)
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            row.Children.Add(new TextBlock
            {
                Text = property.Name,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Foreground = Application.Current.TryFindResource("Brush.ForegroundNormal") as Brush
            });

            FrameworkElement editor = CreateEditor(target, property);
            Grid.SetColumn(editor, 1);
            row.Children.Add(editor);
            return row;
        }

        private static FrameworkElement CreateEditor(FrameworkElement target, PropertyInfo property)
        {
            Type valueType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if(typeof(Geometry).IsAssignableFrom(valueType))
                return CreateIconEditor(target, property, GeometryIconKeys, valueType);

            if(typeof(ImageSource).IsAssignableFrom(valueType))
                return CreateIconEditor(target, property, ImageIconKeys, valueType);

            if(valueType == typeof(bool))
            {
                var checkBox = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    IsThreeState = false
                };

                checkBox.SetBinding(ToggleButton.IsCheckedProperty, new Binding(property.Name)
                {
                    Source = target,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                return checkBox;
            }

            if(valueType.IsEnum)
            {
                var comboBox = new ComboBox
                {
                    ItemsSource = Enum.GetValues(valueType)
                };

                comboBox.SetBinding(Selector.SelectedItemProperty, new Binding(property.Name)
                {
                    Source = target,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

                return comboBox;
            }

            object? lastValidValue = property.GetValue(target);
            var textBox = new TextBox { Text = ConvertToText(lastValidValue, valueType) };

            void Commit()
            {
                if(TryConvert(textBox.Text, property.PropertyType, out object? converted) &&
                   TrySetValue(target, property, converted, textBox))
                {
                    lastValidValue = property.GetValue(target);
                    textBox.Text = ConvertToText(lastValidValue, valueType);
                    ClearError(textBox);
                    return;
                }

                textBox.Text = ConvertToText(lastValidValue, valueType);
                MarkError(textBox, $"Invalid {property.Name} value");
                textBox.SelectAll();
            }

            textBox.LostKeyboardFocus += (_, _) => Commit();
            textBox.KeyDown += (_, e) =>
            {
                if(e.Key == Key.Enter)
                {
                    Commit();
                    e.Handled = true;
                }
                else if(e.Key == Key.Escape)
                {
                    textBox.Text = ConvertToText(lastValidValue, valueType);
                    ClearError(textBox);
                    e.Handled = true;
                }
            };

            DependencyPropertyDescriptor? descriptor = DependencyPropertyDescriptor.FromName(
                property.Name,
                property.DeclaringType ?? target.GetType(),
                target.GetType());
            descriptor?.AddValueChanged(target, (_, _) =>
            {
                if(!textBox.IsKeyboardFocusWithin)
                {
                    lastValidValue = property.GetValue(target);
                    textBox.Text = ConvertToText(lastValidValue, valueType);
                    ClearError(textBox);
                }
            });

            return textBox;
        }

        private static FrameworkElement CreateIconEditor(
            FrameworkElement target,
            PropertyInfo property,
            IEnumerable<string> resourceKeys,
            Type valueType)
        {
            object? currentValue = property.GetValue(target);
            var presets = resourceKeys
                .Select(key => new IconPreset(key, Application.Current.TryFindResource(key)))
                .Where(preset => preset.Value is not null && valueType.IsInstanceOfType(preset.Value))
                .ToList();

            IconPreset? selectedPreset = presets.FirstOrDefault(preset => ReferenceEquals(preset.Value, currentValue));
            if(currentValue is not null && selectedPreset is null)
            {
                selectedPreset = new IconPreset("Current", currentValue);
                presets.Insert(0, selectedPreset);
            }

            var comboBox = new ComboBox
            {
                ItemsSource = presets,
                DisplayMemberPath = nameof(IconPreset.Name),
                SelectedItem = selectedPreset
            };

            comboBox.SelectionChanged += (_, _) =>
            {
                if(comboBox.SelectedItem is IconPreset preset)
                    TrySetValue(target, property, preset.Value, comboBox);
            };

            return comboBox;
        }

        private static bool TrySetValue(FrameworkElement target, PropertyInfo property, object? value, FrameworkElement editor)
        {
            try
            {
                property.SetValue(target, value);
                ClearError(editor);
                return true;
            }
            catch(Exception exception) when(exception is ArgumentException or InvalidOperationException or TargetInvocationException)
            {
                MarkError(editor, exception.InnerException?.Message ?? exception.Message);
                return false;
            }
        }

        private static bool TryConvert(string text, Type propertyType, out object? value)
        {
            Type valueType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

            if(string.IsNullOrWhiteSpace(text) && Nullable.GetUnderlyingType(propertyType) is not null)
            {
                value = null;
                return true;
            }

            if(valueType == typeof(string) || valueType == typeof(object))
            {
                value = text;
                return true;
            }

            if(valueType == typeof(double) && string.Equals(text, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                value = double.NaN;
                return true;
            }

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(valueType);
                value = converter.ConvertFromString(null, CultureInfo.CurrentCulture, text);
                return value is not null;
            }
            catch(Exception exception) when(exception is FormatException or NotSupportedException or ArgumentException)
            {
                value = null;
                return false;
            }
        }

        private static string ConvertToText(object? value, Type valueType)
        {
            if(value is null)
                return string.Empty;

            if(value is double number && double.IsNaN(number))
                return "Auto";

            TypeConverter converter = TypeDescriptor.GetConverter(valueType);
            return converter.CanConvertTo(typeof(string))
                ? converter.ConvertToString(null, CultureInfo.CurrentCulture, value) ?? string.Empty
                : value.ToString() ?? string.Empty;
        }

        private static void MarkError(FrameworkElement editor, string message)
        {
            editor.ToolTip = message;
            editor.SetValue(Control.BorderBrushProperty, Brushes.OrangeRed);
            editor.SetValue(Control.BorderThicknessProperty, new Thickness(2));
        }

        private static void ClearError(FrameworkElement editor)
        {
            editor.ClearValue(Control.BorderBrushProperty);
            editor.ClearValue(Control.BorderThicknessProperty);
            editor.ToolTip = null;
        }

        private sealed record ControlDefinition(
            string Name,
            Func<IReadOnlyList<FrameworkElement>> Factory,
            IReadOnlyList<string> PropertyNames);

        private sealed record PreviewTarget(string Name, FrameworkElement Control);

        private sealed record IconPreset(string Name, object? Value);
    }

    internal enum TesterCategory
    {
        Buttons,
        Inputs,
        Display
    }
}
