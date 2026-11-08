using CommunityToolkit.Mvvm.ComponentModel;
using LibraryTester.Property;
using Lincsolution.UI.Base.Common;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using static LibraryTester.Property.Enum;

namespace LibraryTester
{
    public partial class DashboardViewModel :BaseViewModel<DashboardModel, ISharedModel, DashboardViewCommand>
    {
        public DashboardViewModel(DashboardModel m, ISharedModel sm, INavigationService nav)
            : base(m, sm, nav)
        {

        }

        protected override void Initialise()
        {

        }

        protected override void Execute(DashboardViewCommand cmd)
        {
            if(cmd is DashboardViewCommand.OpenControlsA or DashboardViewCommand.OpenControlsB or DashboardViewCommand.OpenControlsC)
            {
                IsButton1Checked = cmd == DashboardViewCommand.OpenControlsA;
                IsButton2Checked = cmd == DashboardViewCommand.OpenControlsB;
                IsButton3Checked = cmd == DashboardViewCommand.OpenControlsC;
            }
        }

        protected void NavigateTo<TViewModel>(
            object? parameter = null,
            bool forceRefresh = false)
            where TViewModel : class
        {
            Navigation!.NavigateTo<TViewModel>(parameter, forceRefresh);
        }

        protected void NavigateTo<TViewModel>(
            Func<TViewModel> factory,
            object? parameter = null,
            bool forceRefresh = false)
            where TViewModel : class
        {
            Navigation!.NavigateTo(factory, parameter, forceRefresh);
        }

        protected override void PropertiesMapping()
        {
            
        }

        [ObservableProperty] private bool _IsButton1Checked = true;
        [ObservableProperty] private bool _IsButton2Checked;
        [ObservableProperty] private bool _IsButton3Checked;
    }
}
