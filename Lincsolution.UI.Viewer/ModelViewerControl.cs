using System.Windows;
using System.Diagnostics;
using System.Windows.Forms.Integration;
using System.ComponentModel;
using System.Windows.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Mathematics;
using Lincsolution.Core.Rendering;
using GL = OpenTK.Graphics.OpenGL4.GL;
using UserControl = System.Windows.Controls.UserControl;
using Brushes = System.Windows.Media.Brushes;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Lincsolution.UI.Viewer
{
    public sealed class ModelViewerControl : UserControl
    {
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register(nameof(Model), typeof(Model3DData), typeof(ModelViewerControl), new PropertyMetadata(null, OnModelChanged));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(ModelViewerControl), new PropertyMetadata(false, OnLoadingDisplayChanged));

        public static readonly DependencyProperty LoadingProgressProperty =
            DependencyProperty.Register(nameof(LoadingProgress), typeof(double), typeof(ModelViewerControl), new PropertyMetadata(0.0, OnLoadingDisplayChanged));
        public Model3DData? Model { get => (Model3DData)GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
        public bool IsLoading { get => (bool)GetValue(IsLoadingProperty); set => SetValue(IsLoadingProperty, value); }
        public double LoadingProgress { get => (double)GetValue(LoadingProgressProperty); set => SetValue(LoadingProgressProperty, value);}

        private bool CanReloadModel => isOpenGLReady && Model is not null && !IsLoading && !IsUploadingModel && !displayedModelHandle.IsValid;

        private bool IsUploadingModel;

        private bool ReloadTextureIsUploading;

        /// <summary>
        /// WPF는 자체적인 HWND(윈도우 핸들러)를 못 만들고, OpenGL은 별도 HWND가 있어야만 렌더링 가능.
        /// 그래서 별도 HWND를 가질 수 있는 WindowsFormsHost 위에 렌더링해야 가능.
        /// 이를 위한 중간 hosting 용
        /// </summary>
        private readonly WindowsFormsHost? host;

        /// <summary>
        /// 이 컨트롤을 소유하는 소유자 윈도우. 앱 종료 시 안정적으로 OpenGL 등 종료하기 위함.
        /// </summary>
        private Window? ownerWindow;

        /// <summary>
        /// OpenGL 제어하기 위한 인스턴스
        /// </summary>
        private GLControl? gl;

        /// <summary>
        /// GLControl 사용 가능 여부 확인용
        /// </summary>
        private bool isOpenGLReady;

        /// <summary>파트/서포트 셰이더와 uniform 수명 관리.</summary>
        private ModelSurfaceShader? surfaceShader;
        /// <summary>모델 크기에 맞춘 요철 주파수. 줌/회전 시 재계산하지 않아 표면 위치 유지.</summary>
        private float surfaceDetailFrequency = 1f;

        private OpenGLRenderer? renderer;
        private ModelHandle displayedModelHandle = ModelHandle.Invalid;

        /// <summary></summary>
        private readonly OrbitCamera camera = new OrbitCamera(radius:3.0f, theta: 55.0f, phi:270.0f);


        private bool isRotating, isPanning;

        private int lastMouseX, lastMouseY;

        private const float rotateSensitivity = 0.25f;

        private readonly DirectionalLight defaultLight = 
            new DirectionalLight( 
                new Vector3(1.0f, 1.0f, 1.0f), 
                Color.White,
                Intensity: 1.0f,
                AmbientStrength: 0.2f);

        /// <summary>
        /// 플라스틱 품질 선택. 기본값은 실측 후 선택한 Detailed.
        /// 값 변경 시 기존 그리기 예약 콜백으로 다음 Paint 요청.
        /// Dependency property for part surface quality; defaults to Detailed.
        /// </summary>
        public static readonly DependencyProperty PlasticQualityProperty = DependencyProperty.Register(
            nameof(PlasticQuality), typeof(PlasticQuality), typeof(ModelViewerControl),
            new PropertyMetadata(PlasticQuality.Detailed, OnLoadingDisplayChanged));

        /// <summary>파트 표면 품질. 변경 시 다음 프레임에 반영. Part surface quality.</summary>
        public PlasticQuality PlasticQuality
        {
            /// WPF 속성 저장소에서 현재 품질 단계 조회.
            get => (PlasticQuality)GetValue(PlasticQualityProperty);
            /// CPU 모델/GPU 버퍼 재업로드 없이 셰이더의 품질 입력만 변경.
            set => SetValue(PlasticQualityProperty, value);
        }

        private int reloadButtonTexture;
        private int reloadButtonVertexArray;
        private int reloadButtonProgram;

        private const string ReloadButtonVertexShader = """
        #version 330 core

        out vec2 vTexCoord;

        void main()
        {
            vec2 position = vec2(
                float(gl_VertexID & 1),
                float(gl_VertexID >> 1));

            vTexCoord = vec2(position.x, 1.0 - position.y);

            gl_Position = vec4(position * 2.0 - 1.0, 0.0, 1.0);
        }
        """;

        private const string ReloadButtonFragmentShader = """
        #version 330 core

        in vec2 vTexCoord;
        uniform sampler2D uButtonTexture;
        
        out vec4 FragColor;

        void main()
        {
            FragColor = texture(uButtonTexture, vTexCoord);
        }
        """;


        public ModelViewerControl()
        {
            /// 디자이너 뷰에서 예외 터지는거 방지하는 용도.
            if(DesignerProperties.GetIsInDesignMode(this))
            {
                Content = CreateDesignPlaceHolder();
                return;
            }
            /// GLControl 호스팅용 WindowsFormsHost 생성 후 ModelViewerControl의 Content에 host 할당.
            host = new WindowsFormsHost();
            Content = host;
            /// ModelViewerControl의 Loaded/UnLoaded 이벤트 함수 연결
            Loaded += OnLoaded;
            Unloaded += OnUnLoaded;
        }

        /// <summary>
        /// Model 속성이 변경될 때 호출되는 함수
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d is not ModelViewerControl viewer)
                return;

            /// View 생성 시 기존 모델이 바인딩되는 경우에는 업로드하지 않는다.
            /// 화면이 실제로 표시되고 OpenGL 준비까지 끝난 상태에서
            /// 모델이 변경됐을 때만 자동 업로드한다.
            if(!viewer.IsLoaded || !viewer.IsVisible || !viewer.isOpenGLReady)
                return;
            /// ModelViewerControl의 Model 속성이 변경되면, ReplaceDisplayedModelCore 함수 호출해서 GPU에 올려진 모델 교체.
            viewer.ReplaceDisplayedModel(e.NewValue as Model3DData);
        }

        /// <summary>
        /// 진행률 업데이트나 표시 상태 업데이트 시 호출되는 함수
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnLoadingDisplayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewer = (ModelViewerControl)d;
            /// 진행률이나 표시 상태가 업데이트되면 다시 그리기 예약.
            viewer.gl?.Invalidate();
        }

        public void ResetView()
        {
            camera.Reset();

            if(Model is not null)
            {
                FitCameraToModel(Model);
            }

            gl?.Invalidate();
        }

        /// <summary>
        /// 현재 Model 데이터를 GPU에 업로드하여 다시 표시한다.
        /// </summary>
        public void ReloadModel()
        {
            /// OpenGL이 준비되지 않았으면 아직 업로드할 수 없음.
            if(!isOpenGLReady || gl is null || renderer is null)
                return;
            /// 파일 읽기, 변환 중이거나 모델이 없으면 실행하지 않는다.
            if(IsLoading || Model is null)
                return;
            /// 이미 표시 중이면 같은 모델을 중복 업로드하지 않는다.
            if(displayedModelHandle.IsValid)
                return;
            /// 기존 교체 함수에 GPU 업로드·카메라 맞춤 처리가 들어 있다.
            ReplaceDisplayedModel(Model);
        }

        /// <summary>
        /// ModelViewerControl Load 시 실행 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            /// 디자이너 뷰에서 예외 터지는거 방지
            if(DesignerProperties.GetIsInDesignMode(this))
                return;

            if(gl is not null || host is null)
                return;

            var settings = new GLControlSettings
            {
                /// 컨텍스트 API를 OpenGL 로 사용.
                API = ContextAPI.OpenGL,
                /// API 버전은 4.5
                APIVersion = new Version(4, 5),
                /// 프로파일은 Core 선택을 권장한다고 패키지에서 서술함.
                Profile = ContextProfile.Core,
                /// 버전 3.0 이상의 최신 사용한다는 뜻.
                Flags = ContextFlags.ForwardCompatible,
                /// px 마다 24bit 깊이값을 저장한다는 뜻.
                DepthBits = 24
            };
            /// 위에서 만든 설정 기반으로 GLControl 생성
            gl = new GLControl(settings)
            {
                /// 도킹되는 컨트롤의 배치 형태(Top/Bottom/Left/Right/Fill) 중 Fill 로 설정
                Dock = System.Windows.Forms.DockStyle.Fill
            };
            /// GLControl이 Load 됐을 때의 이벤트에 연결
            gl.Load += OnOpenGLLoaded;
            /// OpenGL이 화면 그릴 때의 이벤트 연결
            gl.Paint += OnOpenGLPaint;
            /// OpenGL 크기 조정 시의 이벤트 연결
            gl.Resize += OnOpenGLResize;

            gl.MouseDown += OnOpenGLMouseDown;
            gl.MouseUp += OnOpenGLMouseUp;
            gl.MouseMove += OnOpenGLMouseMove;
            gl.MouseWheel += OnOpenGLMouseWheel;

            /// GLControl을 WindowsFormsHost에 호스팅
            host.Child = gl;
            /// 이 컨트롤 소유한 Window 가져오기.
            ownerWindow = Window.GetWindow(this);
            /// 소유자 Window 종료 시 연동 이벤트 연결
            if(ownerWindow is not null)
            {
                ownerWindow.Closing += OnOwnerWindowClosing;
            }
        }

        /// <summary>
        /// GLControl Load시 실행 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLLoaded(object? sender, EventArgs e)
        {
            if(gl is null)
                return;

            /// 이 Thread에서 OpenGL 명령을 사용할 컨텍스트로 지정.
            /// OpenGL은 반드시 현재 Thread에 활성된 Context가 존재해야 쓸 수 있음.
            gl.MakeCurrent();
            /// 깊이 테스트 활성화.
            GL.Enable(EnableCap.DepthTest);
            /// DepthFunction.Less => 새 픽셀 깊이가 저장된 깊이보다 작으면 출력, 크거나 같으면 버림.
            /// 각 EBO 배치를 비교해서, 어떤게 앞에 있고 뒤에 있는지 판단한 다음, 뒤에 가려지는 것들은 출력하지 않음.
            GL.DepthFunc(DepthFunction.Less);
            /// Clear 처리할 깊이값 1.0으로 명시.
            GL.ClearDepth(1.0);
            /// GLControl의 기본 Framebuffer로 선택
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            /// Framebuffer 파라미터 중 depth buffer 크기값 확인
            GL.GetFramebufferAttachmentParameter(FramebufferTarget.Framebuffer, FramebufferAttachment.Depth,
                FramebufferParameterName.FramebufferAttachmentDepthSize, out int depthBits);
            Debug.WriteLine($"Depth Buffer Bits: {depthBits}");

            /// 준비된 현재 GL Context에서 파트/서포트 셰이더 생성.
            surfaceShader = new ModelSurfaceShader();

            renderer = new OpenGLRenderer();

            /// 화면 생성 시점이라, GPU에 Model을 올리지는 않음.
            /// 새 모델 전달 또는 RELOAD 요청 시에 실제 업로드 수행.
            displayedModelHandle = ModelHandle.Invalid;

            /// OpenGL 준비상태로 플래그 변경
            isOpenGLReady = true;
            /// 안정적인 로드를 위해, 초기에 한 번 Viewport 업데이트
            UpdateViewport();
            /// 렌더링 예약 기능.
            gl.Invalidate();
        }

        /// <summary>
        /// ModelViewerControl UnLoad 시 실행 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUnLoaded(object sender, RoutedEventArgs e)
        {
            /// OpenGL Relase
            ReleaseOpenGL();
        }

        /// <summary>
        /// 이 컨트롤의 소유자 Window 종료 시 연계되는 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOwnerWindowClosing(object? sender, CancelEventArgs e)
        {
            if(e.Cancel)
                return;
            /// OpenGL Relase
            ReleaseOpenGL();
        }

        /// <summary>
        /// 디자이너 뷰에서 컴파일 에러 & 디자인 깨짐 방지용
        /// </summary>
        /// <returns></returns>
        private static UIElement CreateDesignPlaceHolder()
        {
            return new Border
            {
                Background = Brushes.DimGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "OpenGL Model Viewer",
                    Foreground = Brushes.White,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                }
            };
        }

        /// <summary>
        /// OpenGL 관련 이벤트 및 인스턴스 해제 함수
        /// </summary>
        private void ReleaseOpenGL()
        {
            if(gl is null)
                return;

            /// OpenGL 준비상태 플래그 변경
            isOpenGLReady = false;

            if(ownerWindow is not null)
            {
                ownerWindow.Closing -= OnOwnerWindowClosing;
                ownerWindow = null;
            }

            /// GLControl 재진입 방지용도로 인스턴스 추가
            GLControl control = gl;

            control.MakeCurrent();
            /// RELOAD 버튼 리소스 해제
            ReleaseReloadButtonResources();
            control.Capture = false;
            isRotating = false;
            isPanning = false;

            if(renderer is not null)
            {
                if(displayedModelHandle.IsValid)
                {
                    renderer.ReleaseModel(displayedModelHandle);
                    displayedModelHandle = ModelHandle.Invalid;
                }
                renderer.Dispose();
                renderer = null;
            }

            /// 모델 표면 프로그램 해제. RELOAD 프로그램과 별도 수명 관리.
            surfaceShader?.Dispose();
            surfaceShader = null;

            /// 재진입 방지하기 위해 null 처리
            gl = null;

            /// GLControl Load, Paint, Resize 이벤트 해제
            control.Load -= OnOpenGLLoaded;
            control.Paint -= OnOpenGLPaint;
            control.Resize -= OnOpenGLResize;
            control.MouseDown -= OnOpenGLMouseDown;
            control.MouseUp -= OnOpenGLMouseUp;
            control.MouseMove -= OnOpenGLMouseMove;
            control.MouseWheel -= OnOpenGLMouseWheel;

            /// WindowsFormsHost.Child 해제
            if(host is not null && ReferenceEquals(host.Child, control))
                host.Child = null;
            /// GLControl Dispose
            control.Dispose();

            Debug.WriteLine("Release Success");
        }

        /// <summary>
        /// OpenGL 화면 그릴 때 이벤트 함수
        /// 렌더링 흐름 설명:
        /// 1. GLControl Context 활성: 현재 GLControl의 Context 활성
        /// 2. Clear: 활성된 컨텍스트에서, 배경색 지정(ClearColor) 후 색상 버퍼 초기화. 이후 사용할 Shader 활성
        /// 3. VAO 선택: Vertex 입력 설정 저장된 VAO 활성.
        /// 4. DrawElements: VAO에 저장된 EBO의 인덱스를 사용하여 삼각형을 출력.
        /// 5. Back Buffer를 gl.SwapBuffers 통해서 화면에 표시.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLPaint(object? sender, System.Windows.Forms.PaintEventArgs e)
        {
            if(!isOpenGLReady || 
                gl is null ||
                renderer is null)
                return;

            /// OnLoaded에서 활성 했지만, 다른 GLControl이나 Context가 활성된 경우 에러 발생할 수 있음.
            /// 안정적인 프레임의 로드를 위해 현재 GLControl의 Context 활성
            gl.MakeCurrent();
            /// 배경색 지정
            GL.ClearColor(0f, 0f, 0f, 1f);
            /// 색상 버퍼 & 깊이 버퍼 클리어
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            /// 현재 카메라의 View/Projection 계산. 기존 행렬 전치 규칙 유지.
            Matrix4 view = ToOpenTKMatrix(camera.ViewMatrix);
            float aspect = Math.Max(gl.ClientSize.Width, 1) / (float)Math.Max(gl.ClientSize.Height, 1);
            float near = Math.Max(camera.Radius / 1000f, 0.001f);
            float far = Math.Max(camera.Radius * 10f, near + 1f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), aspect, near, far);
            /// 양쪽 프로그램에 프레임 공통 카메라/조명 정보 전달.
            surfaceShader!.BeginFrame(view, projection, camera.Position, defaultLight, PlasticQuality, surfaceDetailFrequency);
            /// 불투명/반투명 패스에서 Mesh별 재질 선택 후 그리기.
            if(displayedModelHandle.IsValid)
                renderer.DrawModel(displayedModelHandle, surfaceShader.Apply, camera.ViewMatrix);
            /// 버튼과 진행률 오버레이를 위해 모델 프로그램 바인딩 해제.
            GL.UseProgram(0);

            /// 진행률 Progress Bar 오버레이 그리기.
            if(IsLoading && !IsUploadingModel)
            {
                renderer.DrawProgressOverlay((float)(LoadingProgress / 100.0));
            }

            DrawReloadButton();

            /// 완성된 Buffer 화면에 표시.
            gl.SwapBuffers();
        }

        /// <summary>
        /// OpenGL 크기 조정 시 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLResize(object? sender, EventArgs e)
        {
            if(!isOpenGLReady || gl is null)
                return;

            /// OnLoaded에서 활성 했지만, 다른 GLControl이나 Context가 활성된 경우 에러 발생할 수 있음.
            /// 안정적인 프레임의 로드를 위해 현재 GLControl의 Context 활성
            gl.MakeCurrent();
            /// Viewport 업데이트
            UpdateViewport();
            /// Size 조정 후 다시 그리기에 대한 렌더링 예약.
            gl.Invalidate();
        }

        /// <summary>
        /// Viewport 업데이트 함수.
        /// </summary>
        private void UpdateViewport()
        {
            if(gl is null)
                return;

            /// 현재 클라이언트의 width, height 크기 가져오기.
            int width = Math.Max(gl.ClientSize.Width, 1);
            int height = Math.Max(gl.ClientSize.Height, 1);
            /// OpenGL이 그리는 영역을 px 단위로 지정(업데이트)
            GL.Viewport(0, 0, width, height);

            Debug.WriteLine($"OpenGL Viewport : {width} x {height}");
        }

        /// <summary>
        /// OpenGL 윈도우에서 마우스 버튼이 눌렸을 때 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLMouseDown(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            /// Reload 버튼 영역 안에서 마우스 클릭될 경우, ReloadModel 실행.
            if(CanReloadModel && e.Button == MouseButtons.Left 
                && IsInsideReloadButton(e.Location))
            {
                ReloadModel();

                return;
            }
            if(gl is null)
                return;

            gl.Focus();

            if(e.Button == MouseButtons.Middle)
            {
                ResetView();
                return;
            }

            if(e.Button == MouseButtons.Left)
                isRotating = true;
            else if(e.Button == MouseButtons.Right)
                isPanning = true;
            else
                return;

            lastMouseX = e.X;
            lastMouseY = e.Y;

            gl.Capture = true;
        }

        /// <summary>
        /// OpenGL 윈도우에서 마우스 버튼 눌린 후, 올라갔을 때 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLMouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
                isRotating = false;

            if(e.Button == MouseButtons.Right)
                isPanning = false;
            
            if(gl is not null && !isRotating && !isPanning)
                gl.Capture = false;
        }

        /// <summary>
        /// OpenGL 윈도우에서 마우스 버튼 클릭 후 이동할 때 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLMouseMove(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if((!isRotating && !isPanning) || gl is null)
                return;

            int deltaX = e.X - lastMouseX;
            int deltaY = e.Y - lastMouseY;

            lastMouseX = e.X;
            lastMouseY = e.Y;

            if(isRotating)
            {
                camera.Rotate(deltaTheta: -deltaY * rotateSensitivity, deltaPhi: -deltaX * rotateSensitivity);
            }
            else if(isPanning)
            {
                camera.Pan(deltaX, deltaY, gl.ClientSize.Width, gl.ClientSize.Height);
            }

            gl.Invalidate();
        }

        /// <summary>
        /// OpenGL 윈도우에서 마우스 휠 움직일 때 이벤트 함수
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOpenGLMouseWheel(object? sender, System.Windows.Forms.MouseEventArgs e)
        {
            if(gl is null)
                return;

            /// 일반 마우스는 휠 한 칸에 120씩 움직임.
            float wheelSteps = e.Delta / 120.0f;

            camera.ZoomRelative(delta: wheelSteps, speed: 0.12f);

            gl.Invalidate();
        }

        /// <summary>
        /// Model Reload 시, Reloading 중임을 먼저 표시한 뒤에 GPU 모델을 교체하는 함수.
        /// 하단의 ReplaceDisplayedModelCore 함수를 마지막에 실행함.
        /// </summary>
        /// <param name="modelData">교체할 새로운 Model3DData</param>
        private void ReplaceDisplayedModel(Model3DData? modelData)
        {
            /// WPF 및 OpenGL 처리는 이 Control의 UI 스레드에서 실행한다.
            Dispatcher.VerifyAccess();

            if(!isOpenGLReady || gl is null || renderer is null)
                return;

            if(IsUploadingModel)
                return;

            /// Model3DData 가 null 인 경우, Model을 해제. 이 경우에는 Reload 상태 표시가 필요 없음.
            if(modelData is null)
            {
                ReplaceDisplayedModelCore(null);
                return;
            }

            IsUploadingModel = true;

            try
            {
                /// Referesh는 Paint 명령을 즉시 실행하고,
                /// Model 업로드 전 준비 화면의 SwapBuffers 까지 수행함.
                gl.Refresh();
                /// 실제 모델 업로드는 기존 함수 그대로 사용.
                /// MakeCurrent는 기존 함수에 있으므로 여기서는 생략.
                ReplaceDisplayedModelCore(modelData);
            }
            finally
            {
                /// 업로드 실패 시, IsUploadingModel false 처리.
                IsUploadingModel = false;
                /// 그리기 예약.
                gl?.Invalidate();
            }
        }

        /// <summary>
        /// 현재 GPU 모델을, 새로운 Model3DData로 교체하는 함수.
        /// OpenGL이 Ready가 아니면, Loaded 시 처리.
        /// </summary>
        /// <param name="modelData">교체할 새로운 Model3DData</param>
        private void ReplaceDisplayedModelCore(Model3DData? modelData)
        {
            if(!isOpenGLReady || gl is null || renderer is null)
                return;

            /// OpenGL Context 활성화
            gl.MakeCurrent();

            /// input으로 들어온 modelData가 null 인 경우,
            /// displayedModelHandle 이 유효하면, GPU에서 모델 해제 처리.
            if(modelData is null)
            {
                if(displayedModelHandle.IsValid)
                {
                    renderer.ReleaseModel(displayedModelHandle);

                    displayedModelHandle = ModelHandle.Invalid;
                }
                /// OpenGL Context 무효 처리해서, 새로운걸 그릴 수 있게 상태 변경.
                gl.Invalidate();
                /// 이후 즉시 return.
                return;
            }

            /// modelData가 null이 아니므로, 여기서부터는 새 모델을 올리기 위한 처리 진행.
            /// 일단은, 새 모델을 먼저 올림. 만약 실패하면 기존 모델을 계속 유지하기 위함.
            ModelHandle newModelHandle = renderer.UploadModel(modelData);

            /// 이미 GPU에 올려진 모델이 있으면, 기존 모델 Release 처리.
            if(displayedModelHandle.IsValid)
            {
                renderer.ReleaseModel(displayedModelHandle);
            }

            /// GPU에 올린 새 모델로 현재 모델 Handle 갱신
            displayedModelHandle = newModelHandle;
            /// 카메라 위치를 모델 경계에 맞게 조정
            FitCameraToModel(modelData);
            /// OpenGL Context 무효 처리해서, 새로운걸 그릴 수 있게 상태 변경.
            gl.Invalidate();
        }

        /// <summary>
        /// Model 크기 계산해서, OrbitCamera의 위치를 모델에 맞게 조정하는 함수.
        /// </summary>
        /// <param name="modelData">카메라를 맞출 대상 모델 데이터</param>
        /// <exception cref="ArgumentException">모델에 유한하지 않은 값 포함 시</exception>
        private void FitCameraToModel(Model3DData modelData)
        {
            /// 카메라 최대 최솟값은 float 기준으로 설정.
            var minimum = new System.Numerics.Vector3(float.PositiveInfinity);
            var maximum = new System.Numerics.Vector3(float.NegativeInfinity);
            bool hasPosition = false;

            /// Model에 있는 모든 Mesh, Mesh 안의 모든 Position 확인.
            foreach(MeshData mesh in modelData.Meshes)
            {
                foreach(System.Numerics.Vector3 position in mesh.Positions)
                {
                    if(!float.IsFinite(position.X) ||
                       !float.IsFinite(position.Y) ||
                       !float.IsFinite(position.Z))
                    {
                        throw new ArgumentException("Model contains a non-finite position", nameof(modelData));
                    }
                    /// 최소 최대 비교해서 더 작거나 큰 쪽으로 할당.
                    minimum = System.Numerics.Vector3.Min(minimum, position);
                    maximum = System.Numerics.Vector3.Max(maximum, position);
                    hasPosition = true;
                }
            }

            /// 모델에 position 값이 없으면 return
            if(!hasPosition)
                return;

            /// min, max 값 기준으로 경계 설정
            var bounds = new Bounds3D(minimum, maximum);
            /// 모델의 가장 긴 변에 약 200주기를 배치. 파일 단위/크기 차이에도 비슷한 표면 밀도 유지.
            float longestSide = Math.Max(bounds.Size.X, Math.Max(bounds.Size.Y, bounds.Size.Z));
            surfaceDetailFrequency = 200f / Math.Max(longestSide, 0.001f);
            /// 카메라 맞춤.
            camera.Fit(bounds);

            Debug.WriteLine($"Model Center : {bounds.Center}, size : {bounds.Size}, distance : {camera.Radius}");
        }

        /// <summary>
        /// OpenGL 윈도우에 그리는 RELOAD 버튼 영역 계산하는 함수.
        /// GLControl의 클라이언트 좌표로 계산.
        /// </summary>
        /// <returns>계산된 Rectangle 영역</returns>
        private Rectangle GetReloadButtonBounds()
        {
            if(gl is null)
                return Rectangle.Empty;

            float scale = gl.DeviceDpi / 96f;

            int size = Math.Min((int)Math.Round(120 * scale), 
                       Math.Min(gl.ClientSize.Width, gl.ClientSize.Height));

            if(size <= 0)
                return Rectangle.Empty;

            return new Rectangle((gl.ClientSize.Width - size) / 2,
                                 (gl.ClientSize.Height - size) / 2,
                                 size, size);
        }

        /// <summary>
        /// RELOAD 버튼 배경과 글자를 함께 그린다.
        /// Draws the reload button background and text.
        /// </summary>
        private void DrawReloadButton()
        {
            if(gl is null || (!CanReloadModel && !IsUploadingModel))
                return;
            /// RELOAD 버튼 경계 계산
            var bounds = GetReloadButtonBounds();

            if(bounds.IsEmpty)
                return;
            /// 표시 문구가 바뀔 때에만 기존 이미지랑 자원을 재생성.
            if(reloadButtonProgram != 0 && ReloadTextureIsUploading != IsUploadingModel)
            {
                ReleaseReloadButtonResources();
            }
            // 현재 Paint에서 Context는 이미 활성화되어 있다.
            // 처음 한 번만 이미지와 GPU 자원을 생성한다.
            CreateReloadButtonResources();
            ReloadTextureIsUploading = IsUploadingModel;

            bool depthWasEnabled = GL.IsEnabled(EnableCap.DepthTest);
            bool blendWasEnabled = GL.IsEnabled(EnableCap.Blend);

            try
            {
                // 버튼은 3D 모델의 깊이와 관계없이 표시한다.
                GL.Disable(EnableCap.DepthTest);

                // 이번 버튼 이미지는 배경까지 포함한 불투명 이미지다.
                GL.Disable(EnableCap.Blend);

                // 버튼 영역을 Viewport로 사용한다.
                // 클라이언트 좌표의 Y를 OpenGL 좌표로 변환한다.
                GL.Viewport(
                    bounds.X,
                    gl.ClientSize.Height - bounds.Bottom,
                    bounds.Width,
                    bounds.Height);

                GL.UseProgram(reloadButtonProgram);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, reloadButtonTexture);

                int textureLocation =
            GL.GetUniformLocation(reloadButtonProgram, "uButtonTexture");

                // 셰이더가 0번 텍스처 유닛의 이미지를 읽도록 설정한다.
                GL.Uniform1(textureLocation, 0);

                GL.BindVertexArray(reloadButtonVertexArray);

                // 정점 4개로 사각형을 구성한다.
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            }
            finally
            {
                GL.BindVertexArray(0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.UseProgram(0);

                // 다음 프레임을 위해 전체 화면 Viewport를 복원한다.
                GL.Viewport(
                    0, 0,
                    Math.Max(gl.ClientSize.Width, 1),
                    Math.Max(gl.ClientSize.Height, 1));

                if(depthWasEnabled)
                    GL.Enable(EnableCap.DepthTest);

                if(blendWasEnabled)
                    GL.Enable(EnableCap.Blend);
            }
        }

        /// <summary>
        /// RELOAD 버튼 이미지와 표시용 GPU 자원을 생성한다.
        /// Creates the reload button image and GPU resources.
        /// </summary>
        private void CreateReloadButtonResources()
        {
            if(reloadButtonProgram != 0)
                return;

            int vertexShader = 0;
            int fragmentShader = 0;

            try
            {
                vertexShader = CompileShader(
                    ShaderType.VertexShader,
                    ReloadButtonVertexShader);

                fragmentShader = CompileShader(
                    ShaderType.FragmentShader,
                    ReloadButtonFragmentShader);

                reloadButtonProgram = LinkShaderProgram(
                    vertexShader,
                    fragmentShader);

                reloadButtonVertexArray = GL.GenVertexArray();

                /// 원형 버튼 표시용 256×256 이미지 생성.
                /// 화면 확대 시 글자와 원호의 선명도 확보.
                using(var bitmap = new Bitmap(
                    256,
                    256,
                    PixelFormat.Format32bppArgb))
                {
                    using(var graphics = Graphics.FromImage(bitmap))
                    using(var font = new Font(
                        "맑은 고딕",
                        28f,
                        System.Drawing.FontStyle.Bold,
                        GraphicsUnit.Pixel))
                    using(var format = new StringFormat())
                    using(var arrowPen = new System.Drawing.Pen(Color.FromArgb(80, 160, 235), 12f))
                    using(var arrowBrush = new SolidBrush(Color.FromArgb(80, 160, 235)))
                    {
                        /// Viewer 배경과 같은 불투명한 검은색으로 배경 Clear
                        /// 기존 OpenGL의 Blend 비활성화 설정을 그대로 사용
                        graphics.Clear(Color.Black);

                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                        arrowPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;

                        arrowPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                        const float center = 128;
                        const float radius = 94f;
                        const float startAngle = -45;
                        const float sweepAngle = 290f;

                        graphics.DrawArc(arrowPen,
                                         center - radius,
                                         center - radius,
                                         radius * 2,
                                         radius * 2,
                                         startAngle,
                                         sweepAngle);

                        float angle = (startAngle + sweepAngle) * MathF.PI / 180f;
                        float radialX = MathF.Cos(angle);
                        float radialY = MathF.Sin(angle);
                        float tangentX = -radialX;
                        float tangentY = radialX;
                        float endX = center + radius * radialX;
                        float endY = center + radius * radialY;

                        graphics.FillPolygon(arrowBrush,
                                             new[]
                                             {
                                                new PointF(endX + tangentX * 20f, 
                                                           endY + tangentY * 20f),
                                                new PointF(endX - tangentX * 10f + radialX * 20f,
                                                           endY - tangentY * 10f + radialY * 20f),
                                                new PointF(endX - tangentX * 10f - radialX * 20f,
                                                           endY - tangentY * 10f - radialY * 20f)
                                             });

                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;
                        format.FormatFlags = StringFormatFlags.NoWrap;

                        graphics.DrawString(IsUploadingModel ? "LOADING" : "RELOAD",
                                            font,
                                            System.Drawing.Brushes.White,
                                            new RectangleF(0, 0, bitmap.Width, bitmap.Height),
                                            format);
                    }

                // Bitmap 픽셀 메모리를 OpenGL에 전달하기 위해 잠근다.
                var bitmapData = bitmap.LockBits(
                new Rectangle(
                    0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

                    try
                    {
                        // 아래 업로드는 위에서 생성한 연속적인 32비트 Bitmap 기준.
                        if(bitmapData.Stride != bitmap.Width * 4)
                        {
                            throw new NotSupportedException(
                                "Unexpected reload bitmap stride.");
                        }

                        reloadButtonTexture = GL.GenTexture();

                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, reloadButtonTexture);

                        // Bitmap의 실제 바이트 순서는 BGRA다.
                        GL.TexImage2D(
                            TextureTarget.Texture2D,
                            0,
                            PixelInternalFormat.Rgba,
                            bitmap.Width,
                            bitmap.Height,
                            0,
                            OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                            PixelType.UnsignedByte,
                            bitmapData.Scan0);

                        // 밉맵을 사용하지 않고 확대·축소 시 선형 보간한다.
                        GL.TexParameter(
                            TextureTarget.Texture2D,
                            TextureParameterName.TextureMinFilter,
                            (int)TextureMinFilter.Linear);

                        GL.TexParameter(
                            TextureTarget.Texture2D,
                            TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);

                        GL.TexParameter(
                            TextureTarget.Texture2D,
                            TextureParameterName.TextureWrapS,
                            (int)TextureWrapMode.ClampToEdge);

                        GL.TexParameter(
                            TextureTarget.Texture2D,
                            TextureParameterName.TextureWrapT,
                            (int)TextureWrapMode.ClampToEdge);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }
                }
            }
            catch
            {
                // 생성 도중 실패한 자원도 정리한다.
                ReleaseReloadButtonResources();
                throw;
            }
            finally
            {
                if(vertexShader != 0)
                    GL.DeleteShader(vertexShader);

                if(fragmentShader != 0)
                    GL.DeleteShader(fragmentShader);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        /// <summary>
        /// RELOAD 버튼의 GPU 자원을 해제한다.
        /// Releases the reload button GPU resources.
        /// </summary>
        private void ReleaseReloadButtonResources()
        {
            if(reloadButtonTexture != 0)
            {
                GL.DeleteTexture(reloadButtonTexture);
                reloadButtonTexture = 0;
            }

            if(reloadButtonVertexArray != 0)
            {
                GL.DeleteVertexArray(reloadButtonVertexArray);
                reloadButtonVertexArray = 0;
            }

            if(reloadButtonProgram != 0)
            {
                GL.DeleteProgram(reloadButtonProgram);
                reloadButtonProgram = 0;
            }
        }

        private bool IsInsideReloadButton(System.Drawing.Point location)
        {
            Rectangle bounds = GetReloadButtonBounds();

            if(bounds.IsEmpty)
                return false;

            float centerX = bounds.Left + bounds.Width / 2f;
            float centerY = bounds.Top + bounds.Height / 2f;
            float radius = bounds.Width * (100f / 256f);
            float dx = location.X - centerX;
            float dy = location.Y - centerY;
            
            return ((dx * dx) + (dy * dy)) <= (radius * radius);
        }

        /// <summary>
        /// Shader 파일 컴파일하는 함수.
        /// </summary>
        /// <param name="shaderType">ShaderType enum</param>
        /// <param name="source">Shader 소스 코드</param>
        /// <returns>컴파일된 Shader의 핸들</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static int CompileShader(ShaderType shaderType, string source)
        {
            /// Shader 객체 생성
            int handle = GL.CreateShader(shaderType);
            /// GLSL 코드 전달
            GL.ShaderSource(handle, source);
            /// GPU 드라이버가 컴파일 진행
            GL.CompileShader(handle);
            /// 성공여부 확인
            GL.GetShader(handle, ShaderParameter.CompileStatus, out int compileStatus);
            /// Shader 생성 결과 로그 확인.
            string log = GL.GetShaderInfoLog(handle);

            if(!string.IsNullOrWhiteSpace(log))
                Debug.WriteLine(log);

            if(compileStatus == 0)
            {
                GL.DeleteShader(handle);
                throw new InvalidOperationException($"{shaderType} compilation failed:\n{log}");
            }
            return handle;
        }

        /// <summary>
        /// Shader Program Linking 하는 함수
        /// </summary>
        /// <param name="hVertexShader">Shader의 정점 핸들</param>
        /// <param name="hFragmentShader">Shader의 프래그먼트 핸들</param>
        /// <returns>링크된 Shader Program의 핸들</returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static int LinkShaderProgram(int hVertexShader, int hFragmentShader)
        {
            int program = GL.CreateProgram();

            /// Link 시, 어떤 Shader를 사용할건지 Program에 연결.
            GL.AttachShader(program, hVertexShader);
            GL.AttachShader(program, hFragmentShader);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linkStatus);

            string log = GL.GetProgramInfoLog(program);

            if(!string.IsNullOrWhiteSpace(log))
                Debug.WriteLine(log);

            if(linkStatus == 0)
            {
                GL.DeleteProgram(program);
                throw new InvalidOperationException($"Shader Program Linking failed:\n{log}");
            }
            /// Link 성공했으므로, Program이 더 이상 참조할 필요 없음.
            /// 그리고, Program으로 개별 Shader들을 Link해줬으므로, 이후 개별 Shader는 삭제해야 함.
            /// 삭제하려면, 연결한 Shader들 Detach 처리해야 함.
            GL.DetachShader(program, hVertexShader);
            GL.DetachShader(program, hFragmentShader);

            return program;
        }

        /// <summary>
        /// OpenTKMatrix 형식으로 변환하는 함수
        /// </summary>
        /// <param name="value">System.Numerics.Matrix4x4 형식의 행렬</param>
        /// <returns>OpenTK.Matrix4 형식의 행렬</returns>
        private static Matrix4 ToOpenTKMatrix(System.Numerics.Matrix4x4 value)
        {
            return new Matrix4(value.M11, value.M12, value.M13, value.M14,
                                value.M21, value.M22, value.M23, value.M24,
                                value.M31, value.M32, value.M33, value.M34,
                                value.M41, value.M42, value.M43, value.M44);
        }
    }
}
