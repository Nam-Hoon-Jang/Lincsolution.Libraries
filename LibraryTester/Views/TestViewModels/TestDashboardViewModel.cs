using LibraryTester.Property;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;

namespace LibraryTester
{
    public sealed class TestDashboardViewModel : DashboardViewModel
    {
        public TestDashboardViewModel(
            DashboardModel model,
            ISharedModel shared,
            INavigationService navigation,
            IServiceProvider services)
            : base(model, shared, navigation)
        {
            
        }
    }
}
