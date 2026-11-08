using Lincsolution.UI.Resources;

namespace LibraryTester.Property;

public sealed class UserPreferences
{
    public UiTheme Theme { get; set; } = UiTheme.Light;

    public string Language { get; set; } = "en-US";
}
