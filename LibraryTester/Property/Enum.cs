using System;
namespace LibraryTester.Property
{
    public class Enum
    {
        public enum MainViewCommand
        {
            OK,
            Cancel,
            OpenDashboard,
            OpenPreference,
            EmergencyStop,
            EmergencyReset
        }

        public enum PreferenceViewCommand
        {
            OpenChildA,
            OpenChildB,
            OpenChildC,
        }

        public enum DashboardViewCommand
        {
            OpenControlsA,
            OpenControlsB,
            OpenControlsC,
            StatusIdle,
            StatusBusy,
            StatusReady,
            StatusBuild,
            StatusPause,
            StatusDone,
            StatusStop,
            StatusWarning,
            StatusError,

            AlarmButton1,
            AlarmButton2,
            BasicButton1,
            BasicButton2,
            BasicToggleButton1,
            BasicToggleButton2,

            
        }
    }
}
