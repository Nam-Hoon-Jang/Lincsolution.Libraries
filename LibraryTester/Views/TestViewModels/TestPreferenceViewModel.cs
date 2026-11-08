using LibraryTester.Property;
using Lincsolution.UI.Base.Navigation;

namespace LibraryTester;

public sealed class TestPreferenceViewModel : PreferenceViewModel
{
    public TestPreferenceViewModel(
        PreferenceModel model,
        ISharedModel shared,
        INavigationService navigation)
        : base(model, shared, navigation)
    {
    }
}
