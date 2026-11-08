using System.Configuration;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using LibraryTester.Property;
using log4net;
using Lincsolution.Core.Logger;
using Lincsolution.UI.Base.Common;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;

namespace LibraryTester
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : BaseApplication
    {
        public PreferenceManager<UserPreferences> Preferences { get; private set; } = null!;

        protected override string MutexName => "LibraryTester";
        protected override BaseAppSettings CreateApplicationSettings() => new AppSettings();
        protected override string GetConfigurationPath() => "App.cfg";

        protected override void CreateLogger()
        {
            GlobalContext.Properties["SysLogDir"] = Settings.SysLogDir;

            Settings.SysLog = new Log4NetLogger(
                Settings.Log4NetConfigPath,
                logName: "SysLogger",
                appenderName: "SysLog");

            Settings.SysLog.Info($"{MutexName} UI Started.");
        }

        protected override void CreateDirectories()
        {
            base.CreateDirectories();
        }
        protected override void InitialisePreferences()
        {
            Preferences = CreatePreferenceManager<UserPreferences>();
        }
        protected override ServiceCollection CreateServiceCollections()
        {
            var services = base.CreateServiceCollections();

            services.AddSingleton(Preferences);
            services.AddSingleton<SharedModel>();
            services.AddSingleton<ISharedModel>(sp => sp.GetRequiredService<SharedModel>());
            services.AddTransient<MainModel>();
            services.AddTransient<PreferenceModel>();
            services.AddTransient<DashboardModel>();
            services.AddSingleton<NavigationService>();
            services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());

            services.AddTransient<MainViewModel>();
            services.AddTransient<PreferenceViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<TestMainViewModel>();
            services.AddTransient<TestPreferenceViewModel>();
            services.AddTransient<TestDashboardViewModel>();
            services.AddTransient<DashboardView>();
            services.AddSingleton<PreferenceView>();
            services.AddSingleton<MainView>();

            return services;
        }
        protected override void CacheOnViewModelLoader()
        {
            base.CacheOnViewModelLoader();

            ViewModelLoader.RegisterTestViewModel<MainViewModel, TestMainViewModel>();
            ViewModelLoader.RegisterTestViewModel<PreferenceViewModel, TestPreferenceViewModel>();
            ViewModelLoader.RegisterTestViewModel<DashboardViewModel, TestDashboardViewModel>();
            ViewModelLoader.SetTestMode(enabled: true);

            ViewModelLoader.Load<MainViewModel>(Services);
            ViewModelLoader.Load<PreferenceViewModel>(Services);
            ViewModelLoader.Load<DashboardViewModel>(Services);
        }
        protected override Window CreateMainWindow()
        {
            var view = Services.GetRequiredService<MainView>();
            view.DataContext = ViewModelLoader.Load<MainViewModel>(Services);
            return view;
        }
    }
}
