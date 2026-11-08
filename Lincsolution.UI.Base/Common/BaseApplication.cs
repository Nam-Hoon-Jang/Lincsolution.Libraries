using Lincsolution.UI.Base.Common;
using static Lincsolution.Core.Base.Utilities.FileUtility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.IO;
using System.Windows;
using Lincsolution.UI.Base.Popup;

namespace Lincsolution.UI.Base.Common
{
    public abstract class BaseApplication : Application
    {
        /// <summary>
        /// KO : 중복 실행 방지용 Mutex
        /// EN : Mutex for preventing duplicate execution
        /// </summary>
        private Mutex? _mutex;
        protected abstract string MutexName { get; }
        /// <summary>
        /// KO : UI의 의존성 주입을 위한 서비스 제공자
        /// EN : Service provider for dependency injection in the UI
        /// </summary>
        public IServiceProvider Services { get; private set; }

        /// <summary>
        /// KO : 어플리케이션 전역 설정값 보관하는 인스턴스
        /// EN : Instance that holds global application settings
        /// </summary>
        protected BaseAppSettings Settings { get; private set; }
        
        /// <summary>
        /// KO : 어플리케이션 시작 시 실행 함수
        /// EN : Function executed when the application starts
        /// </summary>
        /// <param name="e">Startup event arguments</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if(!TryAcquireMutex())
            {
                Shutdown();
                return;
            }

            Settings = CreateApplicationSettings();

            Settings.Initialise(GetConfigurationPath());

            CreateDirectories();

            CreateLogger();

            SetLogUnhandledException();

            InitialisePreferences();

            Services = CreateServiceCollections().BuildServiceProvider();

            CacheOnViewModelLoader();

            Window mainWindow = CreateMainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        /// <summary>
        /// KO : BaseAppSettings 생성 및 초기화 함수. 프로젝트 별로 BaseAppSettings를 상속받은 AppSettings 클래스를 생성하여 반환하도록 구현해야 함.
        /// EN : Function to create and initialize BaseAppSettings. Each project should implement this to return an AppSettings class that inherits from BaseAppSettings.
        /// </summary>
        /// <returns>BaseAppSettings</returns>
        protected abstract BaseAppSettings CreateApplicationSettings();

        /// <summary>
        /// KO : BaseAppSettings 생성 시 참조하는 .cfg 파일 경로를 반환하는 함수
        /// EN : Function to return the path of the .cfg file referenced when creating BaseAppSettings
        /// </summary>
        /// <returns></returns>
        protected abstract string GetConfigurationPath();

        /// <summary>
        /// KO : Settings에서 사용할 로거 인스턴스를 생성하고 초기화합니다.
        /// EN : Creates and initializes the logger instances used by Settings.
        /// </summary>
        protected abstract void CreateLogger();

        /// <summary>
        /// KO : 사용자 설정 파일의 기본 경로를 반환합니다. 프로젝트별 경로가 필요하면 override 합니다.
        /// EN : Returns the default user-preference file path. Override it when a project needs another path.
        /// </summary>
        /// <returns>사용자 설정 JSON 파일 경로 / User-preference JSON file path</returns>
        protected virtual string GetPreferenceFilePath()
        {
            string applicationName = GetType().Assembly.GetName().Name ?? MutexName;

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Lincsolution",
                applicationName,
                "UserData.json");
        }

        /// <summary>
        /// KO : 기본 경로에서 사용자 설정 관리자를 생성하고 설정을 불러옵니다.
        /// EN : Creates a preference manager for the default path and loads its preferences.
        /// </summary>
        /// <typeparam name="TPreference">사용자 설정 형식 / User-preference type</typeparam>
        /// <returns>초기화된 사용자 설정 관리자 / Initialized preference manager</returns>
        protected PreferenceManager<TPreference> CreatePreferenceManager<TPreference>()
            where TPreference : class, new()
        {
            var manager = new PreferenceManager<TPreference>(GetPreferenceFilePath());
            manager.Load();

            if(manager.LastLoadError is not null)
            {
                Settings.SysLog?.Warn(
                    $"Failed to load user preferences from '{manager.FilePath}'. Defaults were applied. " +
                    manager.LastLoadError.Message);
            }

            return manager;
        }

        /// <summary>
        /// KO : 사용자 설정을 초기화합니다. 프로젝트에서 설정 형식을 정의한 뒤 override 합니다.
        /// EN : Initializes user preferences. Override after defining the project's preference type.
        /// </summary>
        protected virtual void InitialisePreferences()
        {
        }

        /// <summary>
        /// KO : 어플리케이션 미처리 예외 발생 시 로그 기록. DispatcherUnhandledException, AppDomain.CurrentDomain.UnhandledException, TaskScheduler.UnobservedTaskException 이벤트에 대한 핸들러 등록.
        /// EN : Log unhandled exceptions in the application. Registers handlers for DispatcherUnhandledException, AppDomain.CurrentDomain.UnhandledException, and TaskScheduler.UnobservedTaskException events.
        /// </summary>
        protected virtual void SetLogUnhandledException()
        {
            this.DispatcherUnhandledException += (s, ex) =>
            {
                Settings.SysLog?.Fatal("Dispatcher Unhandled Exception : ", ex.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                Settings.SysLog?.Fatal("AppDomain Unhandled Exception : ", ex.ExceptionObject as Exception);
            };
            TaskScheduler.UnobservedTaskException += (s, ex) =>
            {
                Settings.SysLog?.Fatal("TaskScheduler Unobserved Task Exception : ", ex.Exception);
                ex.SetObserved();
            };
        }

        /// <summary>
        /// KO : 어플리케이션에서 기본적으로 사용되는 경로 생성 함수. 새 프로젝트에서 사용하는 경로에 맞게 override 해서 사용.
        /// EN : Function to create default directories used by the application. Override this in a new project to use directories specific to that project.
        /// </summary>
        protected virtual void CreateDirectories()
        {
            if(!DirExists(Settings.LogDir))
            {
                Directory.CreateDirectory(Settings.LogDir);
            }
            if(!DirExists(Settings.SysLogDir))
            {
                Directory.CreateDirectory(Settings.SysLogDir);
            }
        }

        /// <summary>
        /// KO : ServiceCollection 생성 함수. 새 프로젝트에서 사용하는 주요 서비스들을 등록하도록 override 해서 사용.
        /// EN : Function to create a ServiceCollection. Override this in a new project to register the main services used by that project.
        /// </summary>
        /// <returns></returns>
        protected virtual ServiceCollection CreateServiceCollections()
        {
            var services = new ServiceCollection();
            services.TryAddSingleton<IPopupService, PopupService>();
            return services;
        }

        /// <summary>
        /// KO : ViewModelLoader에서 캐싱할 ViewModel들을 등록하는 함수. 새 프로젝트에서 사용하는 ViewModel들을 등록하도록 override 해서 사용.
        /// EN : Function to register ViewModels to be cached by the ViewModelLoader. Override this in a new project to register the ViewModels used by that project.
        /// </summary>
        protected virtual void CacheOnViewModelLoader()
        {
        
        }

        /// <summary>
        /// KO : 메인 윈도우를 생성하는 함수. 새 프로젝트에서 사용하는 메인 윈도우를 생성하도록 override 해서 사용.
        /// EN : Function to create the main window. Override this in a new project to create the main window used by that project.
        /// </summary>
        protected abstract Window CreateMainWindow();

        /// <summary>
        /// KO : 어플리케이션 종료 시 호출되는 함수. 새 프로젝트에서 필요한 종료 작업을 수행하도록 override 해서 사용.
        /// EN : Function called when the application exits. Override this in a new project to perform necessary cleanup tasks.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            (_mutex as IDisposable)?.Dispose();
            (Services as IDisposable)?.Dispose();
            
            base.OnExit(e);
        }

        /// <summary>
        /// KO : 뮤텍스를 획득하려고 시도하는 함수. 다른 인스턴스가 이미 실행 중인 경우 false를 반환.
        /// EN : Function to attempt to acquire a mutex. Returns false if another instance is already running.
        /// </summary>
        protected bool TryAcquireMutex()
        {
            _mutex = new Mutex(false, MutexName);
            return _mutex.WaitOne(0, false);
        }
    }
}
