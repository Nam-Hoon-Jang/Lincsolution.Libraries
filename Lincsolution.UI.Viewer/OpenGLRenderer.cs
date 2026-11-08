using Lincsolution.Core.Rendering;
using Lincsolution.UI.Viewer;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;

namespace Lincsolution.UI.Viewer
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                                         용어 설명
    ///
    /// - VAO(Vertex Array Object): 정점(Vertex) 데이터를 읽는 방법과 연결 상태 저장하는 객체.
    /// 구체적으로는, Vertex Attribute의 형식, 활성 상태, VBO 연결, EBO Binding 상태를 저장하는 OpenGL 객체.
    /// 
    /// - VBO(Vertex Buffer Object): 실제 Vertex 데이터를 저장하는 객체.
    /// VBO는 아래와 같은 구조로 구성.
    ///   ┌─────────────────┐
    ///   │ X Y Z │ X Y Z │ X Y Z │ X Y Z │
    ///   └─────────────────┘
    /// 
    /// - IBO(Index Buffer Object): Vertex 읽는 순서를 저장하는 객체.
    /// - EBO(Element Buffer Object): OpenGL에서 Vertex 읽는 순서를 저장하는 객체.
    /// IBO와 EBO는 동일한 개념이나, OpenGL은 IBO를 EBO라고 부르며, 일반적으로는 IBO라고 표현한다.
    ///
    ///
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    //////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// 
    /// </summary>
    public sealed partial class OpenGLRenderer : IDisposable
    {
        /// <summary>UploadMesh로 저장한 MeshHandle 리스트</summary>
        private readonly List<MeshSlot> meshSlots = new List<MeshSlot>();

        /// <summary>ReleaseMesh로 해제되어 재사용할 수 있는 MeshSlot 식별자 목록.</summary>
        private readonly Stack<int> freeMeshIds = new Stack<int>();

        /// <summary>여러 MeshHandle 다루는 ModelSlot의 리스트</summary>
        private readonly List<ModelSlot> modelSlots = new List<ModelSlot>();

        /// <summary>Release로 해제되어 재사용할 수 있는 ModelSlot 식별자 목록</summary>
        private readonly Stack<int> freeModelIds = new Stack<int>();

        /// <summary>재질 미지정 Mesh에 사용할 불투명 플라스틱 기본값.</summary>
        private static readonly MaterialData DefaultSurface = new()
        {
            DiffuseColor = new System.Numerics.Vector4(0.82f, 0.84f, 0.88f, 1f)
        };

        /// <summary>
        /// 정점, 인덱스, 노멀 배열을 받아 GPU에 메쉬 데이터를 업로드한다.
        /// 실제 OpenGL 버퍼 생성과 업로드는 UploadMeshCore에서 수행한다.
        /// Uploads mesh arrays to the GPU through UploadMeshCore.
        /// </summary>
        /// <param name="vertices">XYZ가 순서대로 들어 있는 정점 배열.</param>
        /// <param name="indices">정점을 읽는 순서를 지정하는 인덱스 배열.</param>
        /// <param name="normals">XYZ가 순서대로 들어 있는 노멀 배열. 현재 구현에서는 필수.</param>
        /// <returns>업로드한 메쉬를 식별하는 MeshHandle.</returns>
        /// <exception cref="ArgumentNullException">
        /// vertices, indices 또는 normals가 null일 때 발생.
        /// </exception>
        public MeshHandle UploadMesh(
            float[] vertices,
            uint[] indices,
            float[]? normals = null)
        {
            // vertices null 검사.
            if(vertices is null)
                throw new ArgumentNullException(nameof(vertices));

            // indices null 검사.
            if(indices is null)
                throw new ArgumentNullException(nameof(indices));

            // 기존 코드와 동일하게 현재 구현에서는 normals를 필수로 받는다.
            if(normals is null)
                throw new ArgumentNullException(nameof(normals));

            // 배열을 ReadOnlySpan으로 전달한다.
            // 새로운 배열을 생성하거나 데이터를 복사하지 않는다.
            return UploadMeshCore(vertices, indices, normals);
        }

        /// <summary>
        /// MeshData에 저장된 기존 배열을 사용하여 GPU에 메쉬 데이터를 업로드한다.
        /// Vector3[]를 float[]로 복사하는 중간 배열은 생성하지 않는다.
        /// Uploads MeshData without creating intermediate float arrays.
        /// </summary>
        /// <param name="meshData">GPU에 업로드할 메쉬 데이터.</param>
        /// <returns>업로드한 메쉬를 식별하는 MeshHandle.</returns>
        /// <exception cref="ArgumentNullException">meshData가 null일 때 발생.</exception>
        /// <exception cref="ArgumentException">메쉬 내부 배열이 null일 때 발생.</exception>
        /// <exception cref="NotSupportedException">
        /// Vector3 크기가 예상한 12바이트와 다를 때 발생.
        /// </exception>
        public MeshHandle UploadMesh(MeshData meshData)
        {
            // MeshData 유효성 검사.
            if(meshData is null)
                throw new ArgumentNullException(nameof(meshData));

            // MeshData 내부 배열의 null 검사.
            if(meshData.Positions is null ||
               meshData.Indices is null ||
               meshData.Normals is null)
            {
                throw new ArgumentException(
                    "Mesh arrays must not be null.",
                    nameof(meshData));
            }

            // MeshData에서 사용하는 형식은 System.Numerics.Vector3이다.
            // 각 정점을 XYZ의 float 3개로 읽을 것이므로 크기가 12바이트인지 확인한다.
            if(Marshal.SizeOf<Vector3>() != 3 * sizeof(float))
            {
                throw new NotSupportedException(
                    "Unexpected Vector3 layout.");
            }

            // 기존 코드에서는 Positions를 복사할 float[]를 새로 생성했지만,
            // 이제는 Positions 배열의 메모리를 연속된 float 데이터로 바라본다.
            //
            // Vector3[] : [X,Y,Z] [X,Y,Z]
            // float 뷰  :  X Y Z   X Y Z
            //
            // MemoryMarshal.Cast는 여기서 새 배열이나 복사본을 만들지 않는다.
            ReadOnlySpan<float> vertices =
                MemoryMarshal.Cast<Vector3, float>(
                    meshData.Positions.AsSpan());

            // Normals도 별도의 float[]를 만들지 않고 기존 배열을 사용한다.
            ReadOnlySpan<float> normals =
                MemoryMarshal.Cast<Vector3, float>(
                    meshData.Normals.AsSpan());

            // 두 UploadMesh 함수 모두 같은 OpenGL 업로드 경로를 사용한다.
            return UploadMeshCore(vertices, meshData.Indices, normals);
        }

        /// <summary>
        /// 현재 스레드에서 활성화된 OpenGL Context를 통해
        /// 정점, 노멀, 인덱스 데이터를 GPU Buffer Object에 업로드한다.
        /// Uploads contiguous mesh data using the current OpenGL context.
        /// </summary>
        /// <param name="vertices">XYZ 순서의 정점 데이터를 바라보는 읽기 전용 범위.</param>
        /// <param name="indices">정점을 읽는 순서를 지정하는 인덱스 데이터.</param>
        /// <param name="normals">XYZ 순서의 노멀 데이터를 바라보는 읽기 전용 범위.</param>
        /// <returns>업로드한 메쉬를 식별하는 MeshHandle.</returns>
        /// <exception cref="ArgumentException">
        /// 정점, 인덱스 또는 노멀 데이터의 길이가 올바르지 않을 때 발생.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// 인덱스가 정점 배열의 범위를 벗어날 때 발생.
        /// </exception>
        /// <exception cref="OverflowException">
        /// 업로드할 바이트 수가 표현 가능한 범위를 초과할 때 발생.
        /// </exception>
        private unsafe MeshHandle UploadMeshCore(
            ReadOnlySpan<float> vertices,
            ReadOnlySpan<uint> indices,
            ReadOnlySpan<float> normals)
        {
            // vertices는 XYZ 구조이므로 데이터가 없거나 길이가 3의 배수가 아니면 예외 처리.
            if(vertices.Length == 0 || vertices.Length % 3 != 0)
            {
                throw new ArgumentException(
                    "Vertex data must contain XYZ values.",
                    nameof(vertices));
            }

            // 현재 Primitive는 Triangle 기반이므로 인덱스 3개가 하나의 삼각형을 구성한다.
            // 따라서 인덱스가 없거나 개수가 3의 배수가 아니면 예외 처리.
            if(indices.Length == 0 || indices.Length % 3 != 0)
            {
                throw new ArgumentException(
                    "Index count must be a multiple of three.",
                    nameof(indices));
            }

            // 정점마다 노멀 하나가 있어야 하므로 두 float 데이터의 길이가 같아야 한다.
            if(normals.Length != vertices.Length)
            {
                throw new ArgumentException(
                    "Normal count must match vertex count.",
                    nameof(normals));
            }

            // 각 index는 정점 하나를 가리킨다.
            // vertices.Length는 float 개수이므로 3으로 나눠 실제 정점 개수를 구한다.
            int vertexCount = vertices.Length / 3;

            // 모든 index 값은 실제 정점 개수보다 작아야 한다.
            foreach(uint index in indices)
            {
                if(index >= (uint)vertexCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(indices),
                        $"Index {index} exceeds vertex count {vertexCount}.");
                }
            }

            // BufferData에는 요소 개수가 아니라 전체 바이트 수를 전달한다.
            // nint는 포인터 크기의 정수이며, checked로 곱셈 overflow를 검사한다.
            nint vertexBytes =
                checked((nint)vertices.Length * sizeof(float));

            nint normalBytes =
                checked((nint)normals.Length * sizeof(float));

            nint indexBytes =
                checked((nint)indices.Length * sizeof(uint));

            // MeshResource는 CPU에서 관리하는 OpenGL 객체 이름과 메타데이터다.
            // 정점 배열 자체를 보관하는 객체는 아니다.
            var resource = new MeshResource
            {
                VertexCount = vertexCount,
                IndexCount = indices.Length,

                // indices의 요소가 uint이므로 DrawElements에서도 UnsignedInt로 읽는다.
                IndexType = (int)DrawElementsType.UnsignedInt,

                // 현재 구현에서는 노멀 데이터를 필수로 받는다.
                HasNormals = true
            };

            try
            {
                // VAO, VBO, EBO의 OpenGL 객체 이름을 발급받는다.
                // 실제 버퍼 데이터 저장 공간은 이후 BufferData에서 할당한다.
                //
                // 각 이름을 resource에 즉시 기록하므로,
                // 도중에 예외가 발생해도 이미 생성한 자원을 정리할 수 있다.
                resource.VertexArrayObject = GL.GenVertexArray();
                resource.VertexBuffer = GL.GenBuffer();
                resource.NormalBuffer = GL.GenBuffer();
                resource.IndexBuffer = GL.GenBuffer();

                // VAO Binding:
                // 이제부터 설정하는 정점 입력 상태를 이 VAO에 기록한다.
                GL.BindVertexArray(resource.VertexArrayObject);

                // VBO Binding:
                // ArrayBuffer 대상으로 수행할 명령이 적용될 버퍼를 선택한다.
                GL.BindBuffer(
                    BufferTarget.ArrayBuffer,
                    resource.VertexBuffer);

                // Vertex Upload:
                // 기존 정점 메모리를 호출 중 GC가 이동시키지 못하도록 고정한다.
                // pointer는 복사본이 아니라 기존 데이터의 주소다.
                fixed(float* pointer = vertices)
                {
                    // 현재 ArrayBuffer에 GPU 저장 공간을 할당하고 정점 데이터를 복사한다.
                    // StaticDraw: 보통 한 번 업로드하고 여러 번 그리는 데이터라는 사용 힌트.
                    GL.BufferData(
                        BufferTarget.ArrayBuffer,
                        vertexBytes,
                        (IntPtr)pointer,
                        BufferUsageHint.StaticDraw);
                }

                // BufferData 반환 후에는 이 원본 주소를 계속 고정할 필요가 없다.

                // Vertex Attribute 형식 지정:
                // 현재 ArrayBuffer의 VBO와 정점 읽기 형식을 현재 VAO에 기록한다.
                // ArrayBuffer를 Bind한 것만으로는 이 연결이 VAO에 기록되지 않는다.
                //
                // index      : Shader의 layout(location = 0).
                // size       : 정점 하나의 위치 성분 개수, XYZ이므로 3.
                // type       : 각 성분의 데이터형은 float.
                // normalized : 정수 데이터를 정규화하는 옵션. 여기서는 float이므로 false.
                // stride     : 다음 정점까지의 간격, float 3개이므로 12바이트.
                // offset     : VBO 안에서 첫 정점 데이터가 시작하는 위치, 0바이트.
                GL.VertexAttribPointer(
                    index: 0,
                    size: 3,
                    type: VertexAttribPointerType.Float,
                    normalized: false,
                    stride: 3 * sizeof(float),
                    offset: 0);

                // 0번 Attribute에서 버퍼의 데이터를 읽도록 활성화한다.
                GL.EnableVertexAttribArray(0);

                // 노멀 데이터를 저장할 VBO 선택.
                GL.BindBuffer(
                    BufferTarget.ArrayBuffer,
                    resource.NormalBuffer);

                // 정점과 동일하게 기존 노멀 메모리를 고정하고 GPU로 복사한다.
                fixed(float* pointer = normals)
                {
                    GL.BufferData(
                        BufferTarget.ArrayBuffer,
                        normalBytes,
                        (IntPtr)pointer,
                        BufferUsageHint.StaticDraw);
                }

                // Shader의 layout(location = 1)에서 노멀을 읽도록 연결한다.
                // 노멀도 XYZ의 float 3개이므로 stride는 12바이트다.
                GL.VertexAttribPointer(
                    index: 1,
                    size: 3,
                    type: VertexAttribPointerType.Float,
                    normalized: false,
                    stride: 3 * sizeof(float),
                    offset: 0);

                // 1번 Attribute 활성화.
                GL.EnableVertexAttribArray(1);

                // EBO/IBO Binding:
                // 인덱스 데이터를 저장할 버퍼를 선택한다.
                // ElementArrayBuffer 연결은 BindBuffer 시점에 현재 VAO에 기록된다.
                // 따라서 이 코드는 대상 VAO를 Bind한 상태에서 실행해야 한다.
                GL.BindBuffer(
                    BufferTarget.ElementArrayBuffer,
                    resource.IndexBuffer);

                // Index Upload:
                // 기존 인덱스 메모리를 고정하고 GPU로 복사한다.
                fixed(uint* pointer = indices)
                {
                    GL.BufferData(
                        BufferTarget.ElementArrayBuffer,
                        indexBytes,
                        (IntPtr)pointer,
                        BufferUsageHint.StaticDraw);
                }

                // 정점·노멀·인덱스 데이터는 각각 GPU 버퍼에 저장됐고,
                // VAO에는 정점 입력 형식과 관련 버퍼 연결이 기록된 상태다.
                //
                // CPU측 MeshResource를 meshSlots에 등록하고,
                // 이후 이 메쉬를 식별할 MeshHandle을 반환한다.
                return RegisterMesh(resource);
            }
            catch
            {
                // 등록 전에 예외가 발생하면 지금까지 생성한 GPU 자원을 정리한다.
                // OpenGL의 모든 오류가 C# 예외로 발생하는 것은 아니다.
                DeleteMeshResource(resource);
                throw;
            }
            finally
            {
                // 정상 반환과 예외 발생 모두에서 바인딩을 정리한다.
                // return이 있어도 이 finally 실행 후 호출자에게 반환된다.

                // 현재 VAO 선택 해제.
                GL.BindVertexArray(0);

                // 현재 ArrayBuffer 선택 해제.
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

                // 대상 VAO의 EBO 연결은 유지한다.
                // 대상 VAO를 Bind한 상태에서 ElementArrayBuffer를 0으로 바인딩하면
                // 해당 VAO의 EBO 연결까지 지워지므로 그렇게 하지 않는다.
            }
        }

        /// <summary>
        /// MeshHandle이 가리키는 GPU 메쉬를 현재 프레임버퍼에 그리도록 명령한다.
        /// 호출자는 내부 MeshResource 대신 공개 식별자인 MeshHandle만 전달한다.
        /// </summary>
        /// <param name="handle">GPU에 그리도록 명령할 MeshHandle</param>
        /// <exception cref="ArgumentException">유효하지 않은 MeshHandle 사용 시 발생</exception>
        public void DrawMesh(MeshHandle handle)
        {
            /// 그리려는 handle 번호의 MeshResource를 가져오고, 유효성 검사.
            if(!TryGetMesh(handle, out MeshResource? resource) || resource is null)
            {
                throw new ArgumentException("Invalid or released mesh handle", nameof(handle));
            }
            /// TryGetMesh로 가져온 resource를 가지고 해당 VAO 활성
            GL.BindVertexArray(resource.VertexArrayObject);
            /// GPU에 해당 VAO 그리도록 명령.
            /// 이 시점에서의 OpenGL 흐름:
            /// 1. VAO에 저장된 EBO 확인.
            /// 2. EBO에서 index 확인하여 순서에 맞게 VBO vertex 읽음.
            /// 3. VAO Attribute 규칙에 맞게 XYZ 좌표 해석
            /// 4. Vertex Shader의 aPosition으로 전달
            /// 5. index 3개마다 Triangle 생성
            /// 6. Rasterization
            /// 7. Fragment Shader 실행
            /// 8. 현재 프레임버퍼에 결과 기록. 실제 화면 표시는 SwapBuffers 이후 이루어짐.
            GL.DrawElements(BeginMode.Triangles, resource.IndexCount, (DrawElementsType)resource.IndexType, 0);
            /// VAO Binding 해제.
            GL.BindVertexArray(0);
        }


        /// <summary>
        /// MeshHandle이 가리키는 OpenGL 자원을 삭제하고 MeshSlot을 재사용 가능한 상태로 만든다.
        /// </summary>
        /// <param name="handle">해제할 메쉬의 식별자.</param>
        /// <returns>정상적으로 해제했으면 true, 유효하지 않거나 이미 해제된 핸들이면 false.</returns>
        public bool ReleaseMesh(MeshHandle handle)
        {
            /// Handle로 MeshResource 확인 후 유효성 검사
            if(!TryGetMesh(handle, out MeshResource? resource) || resource is null)
            {
                return false;
            }
            /// resource 에 해당하는 MeshResource 제거.
            DeleteMeshResource(resource);
            /// meshSlots에서 MeshSlot 가져오기.
            MeshSlot slot = meshSlots[handle.Id];
            /// 해당 슬롯 명시적으로 null 처리.
            slot.Resource = null;
            /// MeshSlot Generation 키 overflow 방지 목적. MeshHandle의 id는 int 타입이라, key가 int.MaxValue를 넘지 못함.
            slot.Generation = slot.Generation == int.MaxValue ? 0 : slot.Generation + 1;
            /// 해제한 Mesh로 freeMeshIds에 추가
            freeMeshIds.Push(handle.Id);

            return true;
        }

        /// <summary>
        /// 여러 메쉬를 하나의 모델로 등록한다.
        /// </summary>
        /// <param name="meshHandles">모델을 구성하는 메쉬 핸들.</param>
        /// <returns>등록된 모델을 식별하는 ModelHandle.</returns>
        /// <exception cref="ArgumentNullException">
        /// meshHandles가 null이면 발생한다.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// 메쉬가 없거나 유효하지 않은 MeshHandle이 포함되면 발생한다.
        /// </exception>
        public ModelHandle RegisterModel(params MeshHandle[] meshHandles)
        {
            /// meshHandles 파라미터 유효성 검사.
            if(meshHandles is null)
                throw new ArgumentNullException(nameof(meshHandles));

            if(meshHandles.Length == 0)
                throw new ArgumentException("Model must contain at least one mesh", nameof(meshHandles));
            /// meshHandles 유효성 검사 후, 안의 개별 MeshHandle에 대한 유효성 검사.
            foreach(MeshHandle meshHandle in meshHandles)
            {
                if(!TryGetMesh(meshHandle, out _))
                {
                    throw new ArgumentException($"Invalid or released mesh handle:{meshHandle}", nameof(meshHandles));
                }
            }
            
            int id;
            ModelSlot slot;
            /// 해제된 id 있는지 확인해서 우선 부여.
            if(freeModelIds.Count > 0)
            {
                id = freeModelIds.Pop();
                slot = modelSlots[id];
            }
            else
            {
                id = modelSlots.Count;
                slot = new ModelSlot();

                modelSlots.Add(slot);
            }
            
            slot.MeshHandles = meshHandles.ToArray();
            /// 재사용 슬롯에 이전 모델의 재질/정렬 정보가 남지 않도록 초기화.
            slot.Surfaces = Array.Empty<SurfaceEntry>();

            return new ModelHandle(id, slot.Generation);
        }

        /// <summary>
        /// CPU 모델의 재질을 검증/복사한 뒤 모든 Mesh를 GPU에 업로드.
        /// 실패하면 이번 호출에서 생성한 Mesh 자원만 해제, 기존 표시 모델은 유지.
        /// Uploads meshes with material snapshots and cleans partial allocations on failure.
        /// </summary>
        /// <param name="modelData">정점/노멀/인덱스와 Mesh별 MaterialIndex, 재질 목록을 가진 CPU 모델.</param>
        /// <returns>렌더링과 해제에 사용할 GPU 모델 핸들. CPU 정점 배열은 보관하지 않음.</returns>
        /// <exception cref="ArgumentNullException">모델 또는 필수 재질이 null인 경우.</exception>
        /// <exception cref="ArgumentException">빈 모델, 잘못된 재질 번호/값 또는 Mesh 배열인 경우.</exception>
        public ModelHandle UploadModel(Model3DData modelData)
        {
            /// Model3DData 에 대한 유효성 검사
            if(modelData is null)
                throw new ArgumentNullException(nameof(modelData));

            if(modelData.Meshes.Count == 0)
                throw new ArgumentException("Model must contain at least one mesh", nameof(modelData));

            /// GPU 할당 전에 전체 재질/정점 검증. 뒤쪽 Mesh 실패 시 불필요한 GPU 작업 방지.
            var surfaces = modelData.Meshes.Select(mesh => CreateSurface(mesh, modelData)).ToArray();
            /// 업로드 도중 실패한 Mesh를 역으로 정리하기 위한 핸들 목록 준비.
            var meshHandles = new List<MeshHandle>(modelData.Meshes.Count);

            try
            {
                /// modelData에 있는 Meshes 에서 MeshData가져오고, 업로드 후 핸들 반환받아 meshHandles에 추가.
                foreach(MeshData meshData in modelData.Meshes)
                {
                    MeshHandle meshHandle = UploadMesh(meshData);

                    /// 새 GPU 핸들을 먼저 정리 목록에 등록.
                    meshHandles.Add(meshHandle);
                    /// CPU 모델 참조 없이 재질 스냅샷과 핸들만 연결.
                    surfaces[meshHandles.Count - 1].Handle = meshHandle;
                }
                /// Mesh 핸들 등록 완료 후 표시 메타데이터 연결.
                ModelHandle result = RegisterModel(meshHandles.ToArray());
                /// 등록 슬롯에 스냅샷을 연결하여 이후 Paint에서 원본 모델 접근 생략.
                modelSlots[result.Id].Surfaces = surfaces;
                /// 모든 Mesh 등록이 완료된 핸들 반환.
                return result;
            }
            catch
            {
                /// 예외 발생 시, 지금까지 만든 모든 meshHandle 제거.
                foreach(MeshHandle meshHandle in meshHandles)
                {
                    ReleaseMesh(meshHandle);
                }
                throw;
            }
        }

        /// <summary>
        /// 모델에 등록된 모든 메쉬를 현재 프레임버퍼에 그린다.
        /// </summary>
        /// <param name="handle">그릴 모델의 핸들.</param>
        /// <exception cref="ArgumentException">
        /// 유효하지 않거나 해제된 ModelHandle이면 발생한다.
        /// </exception>
        public void DrawModel(ModelHandle handle)
        {
            /// ModelHandle 유효성 검사
            if(!TryGetModel(handle, out ModelSlot? slot) || slot is null)
                throw new ArgumentException("Invalid or Released model handle", nameof(handle));

            /// ModelSlot 안의 MeshHandles에서 각 MeshHandle 마다 DrawMesh 수행
            foreach(MeshHandle meshHandle in slot.MeshHandles)
            {
                DrawMesh(meshHandle);
            }
        }


        /// <summary>
        /// 불투명 Mesh 먼저, 반투명 Mesh는 카메라에서 먼 순서로 렌더링.
        /// Mesh 중심 정렬이므로 서로 교차하는 면의 완전한 투명 합성은 보장하지 않음.
        /// Draws opaque meshes followed by back-to-front transparent meshes.
        /// </summary>
        /// <param name="handle">UploadModel 또는 RegisterModel로 등록한 GPU 모델 핸들.</param>
        /// <param name="applyMaterial">각 Mesh 직전에 프로그램과 uniform을 설정할 콜백. 전달 재질 변경 금지.</param>
        /// <param name="view">월드 중심을 카메라 좌표로 변환하는 System.Numerics 행렬.</param>
        public void DrawModel(ModelHandle handle, Action<MaterialData> applyMaterial, Matrix4x4 view)
        {
            /// 잘못된 모델/콜백은 GL 상태 변경 전에 차단.
            ArgumentNullException.ThrowIfNull(applyMaterial);
            if(!TryGetModel(handle, out ModelSlot? slot) || slot is null)
                throw new ArgumentException("Invalid or released model handle.", nameof(handle));
            /// 호출자가 설정했던 깊이/블렌딩 상태를 저장하여 오버레이에 영향 방지.
            bool blend = GL.IsEnabled(EnableCap.Blend);
            bool depthTest = GL.IsEnabled(EnableCap.DepthTest);
            bool depthWrite = GL.GetBoolean(GetPName.DepthWritemask);
            int srcRgb = GL.GetInteger(GetPName.BlendSrcRgb), dstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int srcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha), dstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            int eqRgb = GL.GetInteger(GetPName.BlendEquationRgb), eqAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            try
            {
                /// 불투명 패스는 깊이 비교와 쓰기 모두 활성화, 블렌딩은 비활성화.
                GL.Enable(EnableCap.DepthTest);
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
                /// 기존 RegisterModel API로 생성된 모델은 기본 재질로 그리기.
                if(slot.Surfaces.Length == 0)
                {
                    applyMaterial(DefaultSurface);
                    DrawModel(handle);
                    return;
                }
                /// 투명 정렬 키 갱신 및 불투명 Mesh 렌더링. 프레임별 배열 복사 없음.
                foreach(SurfaceEntry entry in slot.Surfaces)
                {
                    entry.ViewDepth = Vector3.Transform(entry.Center, view).Z;
                    if(entry.Material.DiffuseColor.W >= 1f)
                    {
                        applyMaterial(entry.Material);
                        DrawMesh(entry.Handle);
                    }
                }
                /// 카메라 앞은 음수 Z이므로 작은 Z부터 렌더링.
                Array.Sort(slot.Surfaces, static (a, b) => a.ViewDepth.CompareTo(b.ViewDepth));
                /// 파트의 깊이는 검사하되 서포트 깊이는 기록하지 않도록 설정.
                GL.DepthMask(false);
                GL.Enable(EnableCap.Blend);
                GL.BlendEquation(BlendEquationMode.FuncAdd);
                /// RGB는 일반 알파 합성, 출력 알파는 누적 불투명도 계산.
                GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha,
                    BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                foreach(SurfaceEntry entry in slot.Surfaces)
                {
                    /// 완전 투명 Mesh와 이미 그린 불투명 Mesh 제외.
                    if(entry.Material.DiffuseColor.W <= 0f || entry.Material.DiffuseColor.W >= 1f)
                        continue;
                    applyMaterial(entry.Material);
                    DrawMesh(entry.Handle);
                }
            }
            finally
            {
                /// 콜백이나 DrawMesh 실패 시에도 호출 전 GL 상태 복원.
                GL.DepthMask(depthWrite);
                GL.BlendFuncSeparate((BlendingFactorSrc)srcRgb, (BlendingFactorDest)dstRgb,
                    (BlendingFactorSrc)srcAlpha, (BlendingFactorDest)dstAlpha);
                GL.BlendEquationSeparate((BlendEquationMode)eqRgb, (BlendEquationMode)eqAlpha);
                if(blend)
                    GL.Enable(EnableCap.Blend);
                else
                    GL.Disable(EnableCap.Blend);
                if(depthTest)
                    GL.Enable(EnableCap.DepthTest);
                else
                    GL.Disable(EnableCap.DepthTest);
            }
        }

        /// <summary>
        /// 모델과 모델이 소유한 모든 메쉬 자원을 해제한다.
        /// </summary>
        /// <param name="handle">해제할 모델 핸들.</param>
        /// <returns>
        /// 정상적으로 해제했으면 true,
        /// 유효하지 않거나 이미 해제된 핸들이면 false.
        /// </returns>
        public bool ReleaseModel(ModelHandle handle)
        {
            if(!TryGetModel(handle, out ModelSlot? slot) || slot is null)
                return false;

            /// Model에 포함된 MeshHandle 별로 GPU 에서 Release
            foreach(MeshHandle meshHandle in slot.MeshHandles)
            {
                ReleaseMesh(meshHandle);
            }
            /// ModelSlot 비우기.
            slot.MeshHandles = Array.Empty<MeshHandle>();
            /// 해제된 모델의 재질 스냅샷도 참조 해제.
            slot.Surfaces = Array.Empty<SurfaceEntry>();

            slot.Generation = slot.Generation == int.MaxValue ? 0 : slot.Generation + 1;

            freeModelIds.Push(handle.Id);

            return true;
        }

        /// <summary>
        /// meshSlots 안에 있는 모든 MeshSlot의 MeshResource 삭제 및 Dispose
        /// </summary>
        public void Dispose()
        {
            foreach(MeshSlot slot in meshSlots)
            {
                if(slot.Resource is not null)
                {
                    DeleteMeshResource(slot.Resource);
                    slot.Resource = null;
                }
            }
            /// 모든 meshSlots, freeMeshIds 클리어.
            meshSlots.Clear();
            freeMeshIds.Clear();
            /// 모든 modelSlots, freeModelIds 클리어
            modelSlots.Clear();
            freeModelIds.Clear();
        }

        /// <summary>
        /// meshSlots에 MeshResource 추가.
        /// </summary>
        /// <param name="resource">meshSlots에 등록할 MeshResource</param>
        /// <returns>등록한 MeshResource의 MeshHandle</returns>
        private MeshHandle RegisterMesh(MeshResource resource)
        {
            /// meshSlots 리스트에서 MeshResource를 저장할 위치.
            int id;
            MeshSlot slot;
            /// 해제된 슬롯이 있으면 재사용하고, 없으면 새 슬롯을 추가.
            /// MeshHandle의 id는 int 타입이라, 가능하면 id를 재사용하는 것이 좋음.
            /// 따라서, 안 쓰기 때문에 해제한 MeshResource의 MeshHandle id를 재사용하기 위해,
            /// 해제한 것들은 freeMeshIds에 저장해두고, 이후에 재생성할 때 이 id를 재사용함.
            if(freeMeshIds.Count > 0)
            {
                id = freeMeshIds.Pop();
                slot = meshSlots[id];
            }
            else
            {
                id = meshSlots.Count;
                slot = new MeshSlot();

                meshSlots.Add(slot);
            }

            slot.Resource = resource;

            return new MeshHandle(id, slot.Generation);
        }

        /// <summary>
        /// 현재 FrameBuffer 아래쪽에 진행률 표시용 Progress Bar를 그린다.
        /// </summary>
        /// <param name="progress">진행률 0.0 - 1.0</param>
        public void DrawProgressOverlay(float progress)
        {
            if(!float.IsFinite(progress))
                return;

            progress = Math.Clamp(progress, 0.0f, 1.0f);

            /// 현재 Viewport의 위치 및 크기 저장
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            /// Viewport 폭과 높이값 저장
            int width = viewport[2];
            int height = viewport[3];
            /// 값 검사해서 0 이하이면 그리지 않고 종료
            if(width <= 0 || height <= 0)
                return;
            /// Progress Bar의 폭, 높이, 바닥 마진 지정.
            int barWidth = Math.Max(1, (int)(width * 0.6f));
            int barHeight = Math.Min(12, height);
            int bottomMargin = Math.Min(24, height - barHeight);

            /// Progress Bar의 위치 계산
            int x = viewport[0] + (width - barWidth) / 2;
            int y = viewport[1] + bottomMargin;

            /// 이 함수가 변경할 OpenGL 상태 보관
            bool wasScissorEnabled = GL.IsEnabled(EnableCap.ScissorTest);

            int[] previousScissor = new int[4];
            GL.GetInteger(GetPName.ScissorBox, previousScissor);

            float[] previousClearColor = new float[4];
            GL.GetFloat(GetPName.ColorClearValue, previousClearColor);

            try
            {
                GL.Enable(EnableCap.ScissorTest);
                /// Progress Bar 배경
                GL.Scissor(x, y, barWidth, barHeight);
                GL.ClearColor(0.15f, 0.15f, 0.15f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                /// 진행 완료율만큼 채우기.
                int filledWidth = (int)(barWidth * progress);

                if(filledWidth > 0)
                {
                    GL.Scissor(x,y, filledWidth, barHeight);
                    GL.ClearColor(0.15f, 0.65f, 1.0f, 1.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                }
            }
            finally
            {
                GL.ClearColor(previousClearColor[0], previousClearColor[1], previousClearColor[2], previousClearColor[3]);

                GL.Scissor(previousScissor[0], previousScissor[1], previousScissor[2], previousScissor[3]);

                if(!wasScissorEnabled)
                {
                    GL.Disable(EnableCap.ScissorTest);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle">가져올 MeshResource의 MeshHandle값</param>
        /// <param name="resource">out으로 내보내는 MeshResource</param>
        /// <returns>MeshResource 반환 성공 시 true, 유효성 검사 실패 시 false</returns>
        private bool TryGetMesh(MeshHandle handle, out MeshResource? resource)
        {
            /// 실패 경로에서도 out 매개변수가 확정되도록 null로 초기화.
            resource = null;
            /// handle 값 유효성 검사.
            if(!handle.IsValid || handle.Id >= meshSlots.Count)
            {
                return false;
            }
            /// meshSlots 리스트에서 MeshSlot 데이터 가져오기.
            MeshSlot slot = meshSlots[handle.Id];
            /// 슬롯 재사용 전의 오래된 핸들을 차단하고 리소스 상태를 검사.
            if(slot.Generation != handle.Generation ||
                slot.Resource is null ||
                slot.Resource.IsDisposed)
            {
                return false;
            }
            /// resource 데이터에 handle로 가져온 MeshResource 할당.
            resource = slot.Resource;

            return true;
        }

        /// <summary>
        /// MeshResource가 보관하는 OpenGL 객체 이름이 0이 아니면 해당 객체를 삭제한다.
        /// OpenGL에서 객체 이름 0은 기본 객체 또는 바인딩 없음에 사용되므로 생성된 사용자 객체가 아니다.
        /// 삭제를 요청한 뒤 각 객체 이름을 0으로 초기화하고 IsDisposed를 true로 변경한다.
        /// </summary>
        /// <param name="resource">삭제할 MeshResource 데이터</param>
        private static void DeleteMeshResource(MeshResource resource)
        {
            if(resource.IsDisposed)
                return;
            
            if(resource.IndexBuffer != 0)
            {
                GL.DeleteBuffer(resource.IndexBuffer);
                resource.IndexBuffer = 0;
            }

            if(resource.VertexBuffer != 0)
            {
                GL.DeleteBuffer(resource.VertexBuffer);
                resource.VertexBuffer = 0;
            }

            if(resource.NormalBuffer != 0)
            {
                GL.DeleteBuffer(resource.NormalBuffer);
                resource.NormalBuffer = 0;
            }

            if(resource.TexCoordBuffer != 0)
            {
                GL.DeleteBuffer(resource.TexCoordBuffer);
                resource.TexCoordBuffer = 0;
            }

            if(resource.VertexArrayObject != 0)
            {
                GL.DeleteVertexArray(resource.VertexArrayObject);
                resource.VertexArrayObject = 0;
            }

            resource.IsDisposed = true;
        }

        /// <summary>
        /// ModelHandle이 가리키는 유효한 ModelSlot을 가져온다.
        /// </summary>
        /// <param name="handle">조회할 모델 핸들.</param>
        /// <param name="slot">조회된 ModelSlot.</param>
        /// <returns>
        /// 유효한 모델이면 true, 잘못됐거나 해제된 핸들이면 false.
        /// </returns>
        private bool TryGetModel(ModelHandle handle, out ModelSlot? slot)
        {
            slot = null;

            /// Invalid 인 handle, modelSlots 범위 넘어간 ID의 handle 차단
            if(!handle.IsValid || handle.Id >= modelSlots.Count)
                return false;

            ModelSlot candidate = modelSlots[handle.Id];


            if(candidate.Generation != handle.Generation || candidate.MeshHandles.Length == 0)
                return false;

            slot = candidate;
            return true;
        }

        /// <summary>GPU 할당 전 재질과 정점을 검증하고 렌더링용 복사본 생성.</summary>
        /// <param name="mesh">재질 번호와 월드 정점을 가진 CPU Mesh.</param>
        /// <param name="model">MaterialIndex가 가리키는 재질 목록의 소유 모델.</param>
        /// <returns>CPU 배열을 참조하지 않는 재질/중심 정보.</returns>
        /// <exception cref="ArgumentException">재질 번호, 색/광택 범위, 정점 좌표가 잘못된 경우.</exception>
        private static SurfaceEntry CreateSurface(MeshData mesh, Model3DData model)
        {
            /// 재질을 읽기 전에 Mesh와 정점 배열의 존재 여부 검사.
            ArgumentNullException.ThrowIfNull(mesh);
            ArgumentNullException.ThrowIfNull(mesh.Positions);
            /// -1만 재질 미지정으로 허용. 잘못된 양수 인덱스를 조용히 무시하지 않도록 검사.
            if(mesh.MaterialIndex < -1 || mesh.MaterialIndex >= model.Materials.Count)
                throw new ArgumentException("Mesh material index is out of range.");
            /// 기본값 또는 모델에 등록된 명시적 재질 선택.
            MaterialData source = mesh.MaterialIndex == -1 ? DefaultSurface : model.Materials[mesh.MaterialIndex];
            ArgumentNullException.ThrowIfNull(source);
            var c = source.DiffuseColor;
            /// NaN, 무한대, 허용 범위 밖 색과 광택 값 차단.
            if(!float.IsFinite(c.X) || !float.IsFinite(c.Y) || !float.IsFinite(c.Z) || !float.IsFinite(c.W)
                || c.X < 0 || c.X > 1 || c.Y < 0 || c.Y > 1 || c.Z < 0 || c.Z > 1 || c.W < 0 || c.W > 1
                || !float.IsFinite(source.SpecularStrength) || source.SpecularStrength < 0 || source.SpecularStrength > 1
                || !float.IsFinite(source.Shininess) || source.Shininess < 1 || source.Shininess > 256
                || !Enum.IsDefined(source.Shading))
                throw new ArgumentException("Invalid surface color, shading or specular parameters.");
            /// Mesh.Bounds가 채워지지 않은 일반 모델도 지원하도록 실제 정점에서 중심 계산.
            Vector3 min = new(float.PositiveInfinity), max = new(float.NegativeInfinity);
            foreach(Vector3 p in mesh.Positions)
            {
                /// 카메라 정렬과 후속 Fit 계산에 유한한 좌표만 허용.
                if(!float.IsFinite(p.X) || !float.IsFinite(p.Y) || !float.IsFinite(p.Z))
                    throw new ArgumentException("Model contains a non-finite position.");
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            /// 반씩 더하여 큰 좌표에서 min+max 덧셈 overflow 완화.
            return new SurfaceEntry
            {
                Center = mesh.Positions.Length == 0 ? Vector3.Zero : min * 0.5f + max * 0.5f,
                Material = new MaterialData
                {
                    /// 색/조명/광택은 복사본으로 유지. 이름은 진단 시 재질 식별용.
                    Name = source.Name,
                    DiffuseColor = c,
                    Shading = source.Shading,
                    SpecularStrength = source.SpecularStrength,
                    Shininess = source.Shininess
                }
            };
        }


        /// <summary>
        /// MeshResource와 슬롯 재사용 여부를 판별할 세대 번호를 저장한다.
        /// </summary>
        private sealed class MeshSlot
        {
            public MeshResource? Resource
            {
                get; set;
            }

            public int Generation
            {
                get; set;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class ModelSlot
        {
            public MeshHandle[] MeshHandles { get; set; } = Array.Empty<MeshHandle>();
            public int Generation { get; set; }
            /// <summary>Mesh별 재질과 정렬 중심. CPU 정점 배열 참조 없음.</summary>
            public SurfaceEntry[] Surfaces { get; set; } = Array.Empty<SurfaceEntry>();
        }

        /// <summary>GPU 핸들, 재질 스냅샷, 카메라 정렬 중심만 보관하는 표시 정보.</summary>
        private sealed class SurfaceEntry
        {
            /// <summary>등록된 GPU Mesh 핸들.</summary>
            public MeshHandle Handle;
            /// <summary>원본 재질의 사후 변경 영향을 받지 않는 업로드 시점 복사본.</summary>
            public required MaterialData Material;
            /// <summary>월드 좌표의 AABB 중심. Mesh 단위 투명 정렬에 사용.</summary>
            public Vector3 Center;
            /// <summary>현재 카메라 좌표 Z. 먼 Mesh부터 그리기 위한 임시 값.</summary>
            public float ViewDepth;
        }
    }

    /// <summary>
    /// <para><see cref="OpenGLRenderer.UploadMesh"/>로 등록된 메쉬를 찾기 위한 키 값.</para>
    /// <para>Lookup token for a mesh registered by <see cref="OpenGLRenderer.UploadMesh"/>.</para>
    /// </summary>
    public readonly record struct MeshHandle(int Id, int Generation)
    {
        public static readonly MeshHandle Invalid = new(-1, -1);
        public bool IsValid => Id >= 0 && Generation >= 0;

        public MeshHandle(int id)
            : this(id, 0)
        {
        }
    }

    /// <summary>
    /// <para>여러 메쉬로 구성될 모델을 찾기 위한 키 값.</para>
    /// <para>Lookup token for a model composed of multiple meshes.</para>
    /// </summary>
    public readonly record struct ModelHandle(int Id, int Generation)
    {
        public static readonly ModelHandle Invalid = new(-1, -1);
        public bool IsValid => Id >= 0 && Generation >= 0;

        public ModelHandle(int id)
            : this(id, 0)
        {
        }
    }

    /// <summary>
    /// <para>GPU 메쉬를 사용하고 해제하는 데 필요한 OpenGL 객체 이름과 그리기 메타데이터.</para>
    /// <para>OpenGL object names and draw metadata required to use and release a GPU mesh.</para>
    /// </summary>
    public sealed class MeshResource
    {
        /// <summary>
        /// <para>OpenGL VAO 핸들.</para>
        /// <para>OpenGL Vertex Array Object handle.</para>
        /// </summary>
        public int VertexArrayObject { get; set; } 

        /// <summary>
        /// <para>위치 정점 데이터를 보관하는 VBO의 OpenGL 객체 이름.</para>
        /// <para>OpenGL object name of the VBO containing vertex positions.</para>
        /// </summary>
        public int VertexBuffer { get; set; } 

        /// <summary>
        /// <para>노멀 데이터를 보관하는 VBO의 OpenGL 객체 이름.</para>
        /// <para>OpenGL object name of the VBO containing normals.</para>
        /// </summary>
        public int NormalBuffer { get; set; } 

        /// <summary>
        /// <para>텍스처 좌표를 보관하는 VBO의 OpenGL 객체 이름.</para>
        /// <para>OpenGL object name of the VBO containing texture coordinates.</para>
        /// </summary>
        public int TexCoordBuffer { get; set; } 

        /// <summary>
        /// <para>인덱스 데이터를 보관하는 EBO의 OpenGL 객체 이름.</para>
        /// <para>OpenGL object name of the EBO containing indices.</para>
        /// </summary>
        public int IndexBuffer { get; set; } 

        /// <summary>
        /// <para>인덱스 개수.</para>
        /// <para>Index count to draw.</para>
        /// </summary>
        public int IndexCount { get; set; }

        /// <summary>
        /// <para>인덱스 타입(OpenGL type enum 값).</para>
        /// <para>Index type value for OpenGL.</para>
        /// </summary>
        public int IndexType { get; set; }

        /// <summary>
        /// <para>노멀 정점 스트림 보유 여부.</para>
        /// <para>Whether this mesh has a normal vertex stream.</para>
        /// </summary>
        public bool HasNormals { get; set; }

        /// <summary>
        /// <para>텍스처 좌표 정점 스트림 보유 여부.</para>
        /// <para>Whether this mesh has a texture-coordinate vertex stream.</para>
        /// </summary>
        public bool HasTexCoords { get; set; }

        /// <summary>
        /// <para>리소스의 현재 생명주기 상태.</para>
        /// <para>Current lifetime state.</para>
        /// </summary>
        public bool IsDisposed { get; set; }

        /// <summary>
        /// <para>업로드 당시 입력 버퍼 정점 수.</para>
        /// <para>Vertex count at upload time.</para>
        /// </summary>
        public int VertexCount { get; set; }
    }

    /// <summary>
    /// Mesh 표면의 기본 재질 정보
    /// </summary>
    /// <param name="BaseColor"></param>
    public readonly record struct Material(Color4 BaseColor);

    /// <summary>
    /// Model에 비추는 빛 방향과 최소 환경광 정보
    /// </summary>
    /// <param name="Direction"></param>
    /// <param name="AmbientStrength"></param>
    public readonly record struct DirectionalLight(OpenTK.Mathematics.Vector3 Direction, Color4 Color, float Intensity = 1.0f, float AmbientStrength = 0.2f);
}


