using Lincsolution.UI.Base.Navigation;

namespace LibraryTester;

public sealed class DesignPreferenceViewModel : PreferenceViewModel
{
    public DesignPreferenceViewModel()
        : base(new PreferenceModel(), new DesignSharedModel(), new DummyNavigationService())
    {
    }
}
