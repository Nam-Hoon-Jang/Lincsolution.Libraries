using Lincsolution.Core.Rendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Lincsolution.UI.Viewer;

/// <summary>파트 표면 품질 비교 단계. Part surface quality levels.</summary>
public enum PlasticQuality
{
    /// <summary>기존 환경광/확산광만 사용. Diffuse-only reference.</summary>
    None,
    /// <summary>매끈한 플라스틱 반사광 추가. Smooth plastic highlights.</summary>
    Basic,
    /// <summary>미세 표면 노멀 변화 추가. Fine procedural surface detail.</summary>
    Detailed
}

/// <summary>
/// 파트용 플라스틱과 서포트용 확산광 프로그램 관리.
/// OpenGL Context가 현재 스레드에 활성화된 상태에서만 생성/사용/해제.
/// Owns plastic and diffuse shader programs in the current GL context.
/// </summary>
internal sealed class ModelSurfaceShader : IDisposable
{
    /// <summary>기본/상세 플라스틱을 선택할 수 있는 파트 프로그램.</summary>
    private readonly int plastic;
    /// <summary>미세 질감 계산을 포함하지 않는 서포트 프로그램.</summary>
    private readonly int diffuse;
    /// <summary>프로그램별 uniform 위치. 프레임마다 문자열 조회 방지.</summary>
    private readonly Dictionary<int, Dictionary<string, int>> uniforms = new();
    /// <summary>중복 GPU 해제 방지 상태.</summary>
    private bool disposed;

    /// <summary>
    /// 공통 정점 셰이더와 두 가지 Fragment 프로그램 생성.
    /// 두 번째 프로그램 실패 시 먼저 생성된 프로그램도 해제.
    /// Creates both programs and releases partial allocations on failure.
    /// </summary>
    public ModelSurfaceShader()
    {
        try
        {
            /// 일반 파트는 광택과 선택적 미세 표면 계산 포함.
            plastic = CreateProgram(false);
            /// 서포트는 전처리 정의로 플라스틱 계산 자체를 컴파일 대상에서 제외.
            diffuse = CreateProgram(true);
        }
        catch
        {
            /// 생성 도중 실패한 프로그램도 정리.
            Dispose();
            throw;
        }
    }

    /// <summary>공통 정점 변환 및 월드 위치/노멀 전달. Row-vector convention.</summary>
    private const string VertexSource = """
        #version 450 core
        layout(location = 0) in vec3 aPosition;
        layout(location = 1) in vec3 aNormal;
        uniform mat4 uView;
        uniform mat4 uProjection;
        out vec3 vPosition;
        out vec3 vNormal;
        void main()
        {
            /// Mesh positions already contain the imported world transform.
            vPosition = aPosition;
            /// Model matrix is identity in the current viewer.
            vNormal = aNormal;
            /// Preserve the existing row-vector multiplication convention.
            gl_Position = vec4(aPosition, 1.0) * uView * uProjection;
        }
        """;

    /// <summary>
    /// 환경광/확산광은 기존 식 유지. 플라스틱만 Blinn-Phong 반사광 추가.
    /// 세밀한 표현은 모델 크기에 비례한 노멀 변화, 픽셀보다 작아지면 감쇠하여 깜빡임 완화.
    /// GLSL 문자열 내부는 드라이버 호환성을 위해 ASCII 주석만 사용.
    /// </summary>
    private const string FragmentBody = """
        in vec3 vPosition;
        in vec3 vNormal;
        uniform vec4 uColor;
        uniform vec3 uLightDirection;
        uniform vec3 uLightColor;
        uniform float uAmbientStrength;
        uniform float uLightIntensity;
        #ifndef DIFFUSE_ONLY
        uniform vec3 uEye;
        uniform int uQuality;
        uniform float uSpecular;
        uniform float uShininess;
        uniform float uDetailFrequency;
        /// Stable pseudo-random scalar for each 3D lattice corner.
        float grainHash(vec3 p)
        {
            return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453);
        }
        /// Smooth value noise: eight corners, interpolated in world space.
        float grainNoise(vec3 p)
        {
            vec3 cell = floor(p);
            vec3 f = fract(p);
            vec3 u = f * f * (3.0 - 2.0 * f);
            float lower = mix(
                mix(grainHash(cell), grainHash(cell + vec3(1,0,0)), u.x),
                mix(grainHash(cell + vec3(0,1,0)), grainHash(cell + vec3(1,1,0)), u.x), u.y);
            float upper = mix(
                mix(grainHash(cell + vec3(0,0,1)), grainHash(cell + vec3(1,0,1)), u.x),
                mix(grainHash(cell + vec3(0,1,1)), grainHash(cell + vec3(1,1,1)), u.x), u.y);
            return mix(lower, upper, u.z);
        }
        #endif
        out vec4 FragColor;
        void main()
        {
            /// Avoid undefined normalization for a degenerate input normal.
            vec3 n = vNormal / max(length(vNormal), 0.000001);
            /// Use the same light direction convention as the original shader.
            vec3 l = normalize(uLightDirection);
            #ifndef DIFFUSE_ONLY
            if (uQuality == 2)
            {
                /// Frequency is fixed at model fit time, independent of camera zoom.
                vec3 p = vPosition * uDetailFrequency;
                /// Fade detail when the texture period is smaller than a pixel.
                float fade = 1.0 - smoothstep(0.8, 2.5, length(fwidth(p)));
                /// Height amplitude scales with grain size rather than model units.
                float height = grainNoise(p) * 0.05 * fade / uDetailFrequency;
                /// Build a local surface gradient without requiring UV coordinates.
                vec3 dx = dFdx(vPosition);
                vec3 dy = dFdy(vPosition);
                vec3 r1 = cross(dy, n);
                vec3 r2 = cross(n, dx);
                float det = dot(dx, r1);
                /// Evaluate derivatives before the per-fragment degeneracy branch.
                vec3 gradient = dFdx(height) * r1 + dFdy(height) * r2;
                /// Degenerate projected triangles keep their original normal.
                if (abs(det) > 0.000000000001)
                    n = normalize(abs(det) * n - sign(det) * gradient);
            }
            #endif
            /// Ambient and Lambert diffuse match the untextured baseline.
            float ndl = max(dot(n, l), 0.0);
            vec3 color = uColor.rgb * uAmbientStrength
                + uColor.rgb * uLightColor * ndl * uLightIntensity * (1.0 - uAmbientStrength);
            #ifdef DIFFUSE_ONLY
            /// Keep translucent supports readable against a black background.
            /// Two-sided diffuse avoids very dark thin support back faces.
            color = uColor.rgb * (0.65 + 0.35 * abs(dot(n, l))) * uLightColor
                * max(uLightIntensity, uAmbientStrength);
            #endif
            #ifndef DIFFUSE_ONLY
            if (uQuality > 0 && ndl > 0.0)
            {
                /// Camera-to-surface direction controls the highlight location.
                vec3 delta = uEye - vPosition;
                vec3 v = delta / max(length(delta), 0.000001);
                /// Safe half vector for opposite light and view directions.
                vec3 sum = l + v;
                vec3 h = sum / max(length(sum), 0.000001);
                /// White dielectric highlight for non-metallic plastic.
                color += uLightColor * uLightIntensity * uSpecular
                    * pow(max(dot(n, h), 0.0), uShininess);
            }
            #endif
            /// Alpha is consumed by the transparent render pass.
            FragColor = vec4(color, uColor.a);
        }
        """;

    /// <summary>Fragment 분기와 uniform 캐시를 포함한 프로그램 생성.</summary>
    /// <param name="diffuseOnly">true면 서포트용 확산광만 컴파일, false면 플라스틱 전체 컴파일.</param>
    /// <returns>링크가 완료된 GL 프로그램 핸들.</returns>
    private int CreateProgram(bool diffuseOnly)
    {
        /// 실패 시에도 개별 셰이더를 해제할 수 있도록 0으로 초기화.
        int vertex = 0, fragment = 0, program = 0;
        try
        {
            /// 기존 컴파일/링크 검증 함수를 재사용.
            vertex = ModelViewerControl.CompileShader(ShaderType.VertexShader, VertexSource);
            fragment = ModelViewerControl.CompileShader(ShaderType.FragmentShader,
                "#version 450 core\n" + (diffuseOnly ? "#define DIFFUSE_ONLY\n" : "") + FragmentBody);
            program = ModelViewerControl.LinkShaderProgram(vertex, fragment);
            /// 공통 uniform 위치는 프로그램 생성 시 한 번만 조회.
            string[] names = ["uView", "uProjection", "uColor", "uLightDirection",
                "uLightColor", "uAmbientStrength", "uLightIntensity"];
            var locations = names.ToDictionary(name => name, name => GL.GetUniformLocation(program, name));
            /// 서포트 프로그램에는 없는 플라스틱 전용 uniform을 별도로 조회.
            if(!diffuseOnly)
                foreach(string name in new[] { "uEye", "uQuality", "uSpecular", "uShininess", "uDetailFrequency" })
                    locations.Add(name, GL.GetUniformLocation(program, name));
            /// 이름 오타나 최적화로 제거된 필수 입력을 시작 시 검출.
            if(locations.Values.Any(location => location < 0))
                throw new InvalidOperationException("Required surface uniform not found.");
            uniforms.Add(program, locations);
            return program;
        }
        catch
        {
            /// 링크 이후 캐시 생성 실패까지 포함하여 부분 프로그램 정리.
            if(program != 0) GL.DeleteProgram(program);
            throw;
        }
        finally
        {
            /// 링크 결과와 무관하게 임시 셰이더 객체 해제.
            if(vertex != 0) GL.DeleteShader(vertex);
            if(fragment != 0) GL.DeleteShader(fragment);
        }
    }

    /// <summary>프레임 공통 카메라/조명 값을 두 프로그램에 전달. Updates frame uniforms.</summary>
    /// <param name="view">월드 좌표를 카메라 좌표로 변환할 행렬.</param>
    /// <param name="projection">현재 화면 종횡비와 near/far가 적용된 투영 행렬.</param>
    /// <param name="eye">월드 좌표의 카메라 위치. 플라스틱 반사광 방향 계산용.</param>
    /// <param name="light">빛 방향/색/강도와 최소 환경광.</param>
    /// <param name="quality">파트에만 적용할 기준/기본/상세 품질.</param>
    /// <param name="detailFrequency">월드 좌표당 요철 위상 변화량. 카메라 이동과 무관하게 모델에 고정.</param>
    public void BeginFrame(Matrix4 view, Matrix4 projection, System.Numerics.Vector3 eye,
        DirectionalLight light, PlasticQuality quality, float detailFrequency)
    {
        /// 같은 카메라와 조명을 두 프로그램에 적용하여 경계 표현 일치.
        foreach(var entry in uniforms)
        {
            /// 현재 프로그램에 해당하는 uniform 위치 사용.
            var u = entry.Value;
            GL.UseProgram(entry.Key);
            /// 기존 Viewer와 동일한 transpose=true 규칙 유지.
            GL.UniformMatrix4(u["uView"], true, ref view);
            GL.UniformMatrix4(u["uProjection"], true, ref projection);
            /// 방향광 및 환경광을 프레임 공통 입력으로 전달.
            GL.Uniform3(u["uLightDirection"], light.Direction);
            GL.Uniform3(u["uLightColor"], light.Color.R, light.Color.G, light.Color.B);
            GL.Uniform1(u["uAmbientStrength"], light.AmbientStrength);
            GL.Uniform1(u["uLightIntensity"], light.Intensity);
            /// 서포트에는 품질 모드와 시선 입력이 불필요.
            if(entry.Key == plastic)
            {
                GL.Uniform3(u["uEye"], eye.X, eye.Y, eye.Z);
                GL.Uniform1(u["uQuality"], (int)quality);
                /// 모델 크기에서 계산한 공간 주파수를 파트 프로그램에 전달.
                GL.Uniform1(u["uDetailFrequency"], detailFrequency);
            }
        }
    }

    /// <summary>Mesh를 그리기 직전 프로그램과 재질 선택. Applies per-mesh material.</summary>
    /// <param name="material">업로드 때 검증/복사된 색, 알파, 조명 방식, 광택 값.</param>
    public void Apply(MaterialData material)
    {
        /// 파일 확장자 대신 재질의 조명 방식만으로 프로그램 선택.
        int program = material.Shading == SurfaceShading.Diffuse ? diffuse : plastic;
        var u = uniforms[program];
        GL.UseProgram(program);
        /// RGB와 알파를 함께 전달하여 반투명 패스에서 사용.
        var color = material.DiffuseColor;
        GL.Uniform4(u["uColor"], color.X, color.Y, color.Z, color.W);
        /// 플라스틱 프로그램에만 반사광 강도와 퍼짐 지수 전달.
        if(program == plastic)
        {
            GL.Uniform1(u["uSpecular"], material.SpecularStrength);
            GL.Uniform1(u["uShininess"], material.Shininess);
        }
    }

    /// <summary>현재 Context에서 프로그램 해제. Repeated disposal is harmless.</summary>
    public void Dispose()
    {
        /// 이미 해제된 GL 이름을 다시 삭제하지 않도록 차단.
        if(disposed) return;
        disposed = true;
        /// 한쪽만 생성된 실패 경로도 처리.
        if(plastic != 0) GL.DeleteProgram(plastic);
        if(diffuse != 0) GL.DeleteProgram(diffuse);
        /// 관리 측 uniform 캐시 정리.
        uniforms.Clear();
    }
}
