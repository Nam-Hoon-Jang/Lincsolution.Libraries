using LibraryTester.Property;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;

namespace LibraryTester;

public sealed class TestMainViewModel : MainViewModel
{
    public TestMainViewModel(
        MainModel model,
        ISharedModel shared,
        INavigationService navigation,
        IServiceProvider services)
        : base(model, shared, navigation)
    {
        Navigation!.NavigateTo(() => ViewModelLoader.Load<PreferenceViewModel>(services));
    }
}
