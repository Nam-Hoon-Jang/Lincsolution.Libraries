using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibraryTester.Property;
using Lincsolution.Core.Rendering;
using Lincsolution.UI.Base.Navigation;
using Lincsolution.UI.Base.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.IO;
using static LibraryTester.Property.Enum;

namespace LibraryTester
{
    public partial class PreferenceViewModel : BaseViewModel<PreferenceModel, ISharedModel, PreferenceViewCommand>
    {
        public PreferenceViewModel(PreferenceModel m, ISharedModel sm, INavigationService nav)
            : base(m, sm, nav)
        {
            
        }

        //protected override void Initialise()
        //{
        //
        //}

        protected override void Execute(PreferenceViewCommand cmd)
        {

        }

        //protected override void PropertiesMapping()
        //{
        //
        //}

        /// <summary>
        /// 파일을 백그라운드에서 읽고, 파트/서포트에 서로 다른 표시 재질을 지정한 뒤 UI 모델 교체.
        /// 파일 읽기 실패 시 기존 모델 유지, 진행률 이벤트 해제와 로딩 상태 복원은 finally에서 처리.
        /// Loads STL or 3MF geometry and assigns display materials.
        /// </summary>
        /// <param name="filePath">사용자가 선택한 로컬 STL 또는 3MF 전체 경로. null/공백 또는 중복 로딩 요청은 무시.</param>
        /// <returns>파일 읽기와 UI 모델 교체가 끝날 때 완료되는 Task.</returns>
        private async Task LoadViewerModelAsync(string? filePath)
        {
            if(IsViewerLoading || string.IsNullOrWhiteSpace(filePath))
                return;

            IsViewerLoading = true;
            ViewerLoadingProgress = 0;
            ViewerError = string.Empty;
            long version = ++viewerLoadVersion;

            /// UI 스레드에서 Progress 생성. 이전 작업의 지연 보고는 무시.
            IProgress<double> progress = new Progress<double>(value =>
            {
                if(IsViewerLoading && viewerLoadVersion == version)
                    ViewerLoadingProgress = value;
            });

            PropertyChangedEventHandler handler = (_, args) =>
            {
                if(args.PropertyName == nameof(viewerLoader.LoadingProcess))
                    progress.Report(viewerLoader.LoadingProcess);
            };

            viewerLoader.PropertyChanged += handler;
            try
            {
                /// 파일 형식에 관계없이 최종 결과는 같은 Model3DData로 전달.
                Model3DData model;

                /// 확장자를 소문자로 통일하여 .STL 같은 대문자 파일도 처리.
                string extension = Path.GetExtension(filePath).ToLowerInvariant();

                if(extension == ".stl")
                {
                    /// Binary STL 로딩. MaterialIndex=-1이므로 Viewer 기본 플라스틱 사용.
                    model = await Task.Run(() => stlLoader.LoadBinary(filePath));
                }
                else if(extension == ".3mf")
                {
                    /// 기존 3MF 로더에서 파트와 서포트를 분리하여 읽기.
                    var (partModel, supports) = await Task.Run(() =>
                        viewerLoader.LoadMeshData_All(filePath));

                    /// 파트 모델을 최종 표시 모델로 사용.
                    model = partModel;

                    /// 기존 목록 뒤에 불투명 플라스틱 재질 추가.
                    int partMaterial = model.Materials.Count;

                    model.Materials.Add(new MaterialData
                    {
                        Name = "Part plastic",
                        DiffuseColor = new Vector4(0.82f, 0.84f, 0.88f, 1f)
                    });

                    /// 서포트용 하늘색 반투명 재질 추가.
                    int supportMaterial = model.Materials.Count;

                    model.Materials.Add(new MaterialData
                    {
                        Name = "Support translucent",
                        DiffuseColor = new Vector4(0.35f, 0.78f, 1f, 0.38f),
                        Shading = SurfaceShading.Diffuse,
                        SpecularStrength = 0f
                    });

                    /// 합쳐질 모델 기준으로 파트와 서포트의 재질 번호 지정.
                    foreach(MeshData mesh in model.Meshes)
                        mesh.MaterialIndex = partMaterial;

                    foreach(MeshData mesh in supports.Meshes)
                        mesh.MaterialIndex = supportMaterial;

                    /// 정점 배열 복사 없이 서포트 Mesh 참조를 최종 모델에 추가.
                    model.Meshes.AddRange(supports.Meshes);
                }
                else
                {
                    /// 지원하지 않는 파일은 기존 catch의 오류 표시 경로로 전달.
                    throw new NotSupportedException(
                        $"지원하지 않는 파일 형식: {extension}");
                }

                if(model.Meshes.Count == 0)
                    throw new InvalidOperationException("읽어온 메쉬가 없습니다.");

                ViewerModel = model;
                ViewerLoadingProgress = 100;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                ViewerError = ex.Message;
            }
            finally
            {
                viewerLoader.PropertyChanged -= handler;
                IsViewerLoading = false;
            }
        }

        #region Variables
        [ObservableProperty] private bool _IsButton1Checked = false;
        [ObservableProperty] private bool _IsButton2Checked = false;
        [ObservableProperty] private bool _IsButton3Checked = false;
        private readonly Lincsolution.Core.Rendering.ThreeMF.ThreeMF viewerLoader = new();
        private readonly Lincsolution.Core.Rendering.STL.Stl stlLoader = new();
        private Model3DData? viewerModel = null;
        private bool isViewerLoading;
        private double viewerLoadingProgress;
        private string viewerError = string.Empty;
        private long viewerLoadVersion;
        private AsyncRelayCommand<string>? loadViewerModelCommand;
        /// <summary>표시할 CPU 모델. CPU model retained across view recreation.</summary>
        public Model3DData? ViewerModel
        {
            get => viewerModel;
            private set => SetProperty(ref viewerModel, value);
        }

        /// <summary>파일 로딩 상태. File loading state.</summary>
        public bool IsViewerLoading
        {
            get => isViewerLoading;
            private set
            {
                if(SetProperty(ref isViewerLoading, value))
                {
                    OnPropertyChanged(nameof(CanEditViewer));
                    loadViewerModelCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>입력 가능 여부. Whether viewer input is available.</summary>
        public bool CanEditViewer => !IsViewerLoading;

        /// <summary>로딩 진행률 0~100. Loading progress from 0 to 100.</summary>
        public double ViewerLoadingProgress
        {
            get => viewerLoadingProgress;
            private set => SetProperty(ref viewerLoadingProgress, value);
        }

        /// <summary>최근 로딩 오류. Last loading error.</summary>
        public string ViewerError
        {
            get => viewerError;
            private set => SetProperty(ref viewerError, value);
        }

        /// <summary>파일 로딩 명령. Loads a file without retaining a view.</summary>
        public IAsyncRelayCommand<string> LoadViewerModelCommand =>
            loadViewerModelCommand ??= new AsyncRelayCommand<string>(
                LoadViewerModelAsync, _ => CanEditViewer);
        #endregion
    }
}
