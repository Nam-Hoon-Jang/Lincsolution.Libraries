using System;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using LibraryTester.Property;
using Lincsolution.UI.Base.Common;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;
using static LibraryTester.Property.Enum;

namespace LibraryTester
{
    public partial class MainViewModel : BaseViewModel<MainModel, ISharedModel, MainViewCommand>
    {
        public MainViewModel(MainModel m, ISharedModel sm, INavigationService nav)
            : base(m, sm, nav)
        {
            
        }

        //protected override void Initialise()
        //{
        //    
        //}

        protected override void Execute(MainViewCommand cmd)
        {
            switch(cmd)
            {
                case MainViewCommand.OpenDashboard:
                    Navigation!.NavigateTo<DashboardViewModel>();
                    break;

                case MainViewCommand.OpenPreference:
                    Navigation!.NavigateTo<PreferenceViewModel>();
                    break;

                case MainViewCommand.EmergencyStop:
                    OpenPopup(AlarmType.ERROR, "Emergency Stopped");
                    break;

                case MainViewCommand.EmergencyReset:
                    OpenPopup(AlarmType.WARNING, "Emergency Reset");
                    break;
            }
        }

        private void OpenPopup(AlarmType type, string popupContent)
        {
            AlarmType = type;
            PopupContent = popupContent;
            PopupVisibility = Visibility.Visible;
        }

        //protected override void PropertiesMapping()
        //{
        //    
        //}

        [ObservableProperty] private AlarmType _AlarmType;
        [ObservableProperty] private Visibility _PopupVisibility;
        [ObservableProperty] private bool _PopupIsSingleButton;
        [ObservableProperty] private string _PopupContent;
        [ObservableProperty] private MainViewCommand _CmdParBtnPopupLeft = MainViewCommand.OK;
        [ObservableProperty] private MainViewCommand _CmdParBtnPopupRight = MainViewCommand.Cancel;
    }
}
