using Lincsolution.UI.Base.Navigation;

namespace LibraryTester;

public sealed class DesignMainViewModel : MainViewModel
{
    public DesignMainViewModel()
        : base(new MainModel(), new DesignSharedModel(), new DummyNavigationService())
    {
        DesignTimeViewModel = new DesignPreferenceViewModel();
    }
}
