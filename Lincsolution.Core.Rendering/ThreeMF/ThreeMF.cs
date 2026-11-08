using System.Linq;
using System.Numerics;
using System.ComponentModel;
using System.Diagnostics;
using System;
using System.Collections.Generic;

namespace Lincsolution.Core.Rendering.ThreeMF
{
    /// <summary>
    /// ThreeMF 클래스용 인터페이스.
    /// </summary>
    public interface IThreeMF
    {
        event PropertyChangedEventHandler PropertyChanged;
        Model3DData LoadMeshData_ModelOnly(string filePath, Unit sourceUnit = Unit.mm);
        (Model3DData Model, Model3DData Supports) LoadMeshData_All(string filePath, Unit sourceUnit = Unit.mm);
        double LoadingProcess { get; }
        ThreeMFState State { get; }
    }

    /// <summary>
    /// .3mf 파일 처리를 위한 클래스.
    /// </summary>
    public sealed class ThreeMF : INotifyPropertyChanged, IThreeMF
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _LoadingProcess;
        public double LoadingProcess
        {
            get => _LoadingProcess;
            private set
            {
                if(!Equals(_LoadingProcess, value))
                {
                    _LoadingProcess = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LoadingProcess)));
                }
            }
        }

        private ThreeMFState _State = ThreeMFState.Idle;
        public ThreeMFState State
        {
            get => _State;
            private set
            {
                if(!Equals(_State, value))
                {
                    _State = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
                }
            }
        }

        private readonly object loadLock = new object();

        /// <summary>
        /// .3mf 파일 내의 "Model" 타입 Mesh만 로드하는 함수.
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="sourceUnit">원본 단위</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">.3mf 파일 읽거나 변환 중일 때 중복 실행 시</exception>
        /// <exception cref="InvalidOperationException">로드 중인 Mesh중 CMeshObject가 아닌 데이터가 있을 때</exception>
        public Model3DData LoadMeshData_ModelOnly(string filePath, Unit sourceUnit = Unit.mm)
        {

            lock(loadLock)
            {
                if(State != ThreeMFState.Idle)
                {
                    throw new InvalidOperationException("Cannot load mesh data while another loading operation is in progress.");
                }

                State = ThreeMFState.Reading;
                LoadingProcess = 0;
            }

            /// 마지막에 전달할 Model3DData
            var result = new Model3DData();

            /// 3mf 파일 처리를 위한 객체.
            CModel cModel = null;
            try
            {
                /// 3mf 파일 처리를 위한 CModel 객체 생성.
                cModel = Lib3mfWrapper.CreateModel();
                /// 생성한 CModel 객체를 참조 카운트 증가.
                Lib3mfWrapper.Acquire(cModel);
                /// 3mf 파일 읽기 위한 CReader 객체 생성 후 파일 읽어오기.
                CReader cReader = cModel.QueryReader("3mf");
                cReader.ReadFromFile(filePath, OnReadProgress);
                /// 3mf 파일 내의 BuildItem들을 순회하기 위한 CBuildItemIterator 객체 생성.
                CBuildItemIterator cBuildIter = cModel.GetBuildItems();
                /// 진행률 표시하기 위해, 읽어야 하는 Mesh 수 계산
                int meshesToLoad = Convert.ToInt32(cBuildIter.Count());
                int currentLayer = 0;
                /// cReader에서 읽는데 시간 오래 걸려서 여기까지 진행되면 40%로 처리.
                LoadingProcess = 40;
                /// 현재 상태는 변환 중으로 표시.
                State = ThreeMFState.ConvertingToMeshData;
                result.Unit = sourceUnit;

                /// cBuildIter가 BuildItem 순환하면서 다음거를 가져올 수 있는지 확인. 없으면 false 반환되면서 반복문 종료.
                while(cBuildIter.MoveNext())
                {
                    /// 현재 BuildItem 가져오기
                    var cBuild = cBuildIter.GetCurrent();
                    /// BuildItem의 Transform 행렬 가져오기
                    var transform = ConvertToMatrix(cBuild.GetObjectTransform());
                    /// BuildItem의 ObjectResource 가져오기
                    var cObject = cBuild.GetObjectResource();
                    /// ObjectResource가 MeshObject가 아니면, 다음 BuildItem으로 넘어가고, 진행률 업데이트
                    if(!cObject.IsMeshObject())
                    {
                        currentLayer++;
                        if(meshesToLoad > 0)
                        {
                            double percent = Convert.ToDouble(((double)currentLayer / (double)meshesToLoad));
                            LoadingProcess = 40 + Math.Round(percent * 50, 1);
                        }
                        continue;
                    }
                    /// ObjectResource가 MeshObject이면, CMeshObject로 캐스팅
                    if(cObject.GetType().Equals(eObjectType.Model))
                    {
                        CMeshObject cMesh = cObject as CMeshObject;
                        /// CMeshObject로 캐스팅. CMeshObject가 아니면 예외 처리
                        if(cMesh is null)
                        {
                            throw new InvalidOperationException("Mesh iterator ended before build item iteration.");
                        }
                        /// MeshData로 변환
                        var meshData = ConvertToMeshData(cMesh, transform, sourceUnit);
                        meshData.Name = cObject.GetName();
                        meshData.LayerIndex = currentLayer;
                        /// Model3DData에 추가
                        AddToModel(result, meshData, currentLayer);
                    }
                    /// 변환 중인 레이어 증가
                    currentLayer++;
                    /// 진행률 업데이트
                    if(meshesToLoad > 0)
                    {
                        double percent = Convert.ToDouble(((double)currentLayer / (double)meshesToLoad));
                        LoadingProcess = 40 + Math.Round(percent * 50, 1);
                    }
                }
                /// 모든 BuildItem 순회 완료 후 상태를 완료로 변경하고, 진행률 100%로 설정
                State = ThreeMFState.Idle;
                LoadingProcess = 100;
            }
            finally
            {
                try
                {
                    /// CModel 객체 해제
                    if(cModel != null)
                    {
                        Debug.WriteLine("Released");
                        Lib3mfWrapper.Release(cModel);
                        /// Garbage Collector 불러서 남은 객체 정리.
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
                finally
                {
                    lock(loadLock)
                    {
                        State = ThreeMFState.Idle;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// .3mf 파일에 있는 모든 Mesh 로드하는 함수
        /// </summary>
        /// <param name="filePath">파일 경로</param>
        /// <param name="sourceUnit">원본 단위</param>
        /// <returns>Model3DData 타입의 Model/Support 각각 Tuple로 반환</returns>
        /// <exception cref="InvalidOperationException">.3mf 파일 읽거나 변환 중일 때 중복 실행 시</exception>
        /// <exception cref="InvalidOperationException">로드 중인 Mesh중 CMeshObject가 아닌 데이터가 있을 때</exception>
        public (Model3DData Model, Model3DData Supports) LoadMeshData_All(string filePath, Unit sourceUnit = Unit.mm)
        {
            lock(loadLock)
            {
                if(State != ThreeMFState.Idle)
                {
                    throw new InvalidOperationException("Cannot load mesh data while another loading operation is in progress.");
                }

                State = ThreeMFState.Reading;
                LoadingProcess = 0;
            }

            /// 마지막에 전달할 Model3DData들
            var modelMeshes = new Model3DData();
            var supportMeshes = new Model3DData();

            /// 3mf 파일 처리를 위한 객체.
            CModel cModel = null;
            try
            {
                /// 3mf 파일 처리를 위한 CModel 객체 생성.
                cModel = Lib3mfWrapper.CreateModel();
                /// 3mf 파일 읽기 위한 CReader 객체 생성 후 파일 읽어오기.
                using(CReader cReader = cModel.QueryReader("3mf"))
                {
                    cReader.ReadFromFile(filePath, OnReadProgress);
                }
                /// 3mf 파일 내의 BuildItem들을 순회하기 위한 CBuildItemIterator 객체 생성.
                using(CBuildItemIterator cBuildIter = cModel.GetBuildItems())
                {
                    /// 진행률 표시하기 위해, 읽어야 하는 Mesh 수 계산
                    int meshesToLoad = Convert.ToInt32(cBuildIter.Count());
                    int currentLayer = 0;
                    /// cReader에서 읽는데 시간 오래 걸려서 여기까지 진행되면 40%로 처리.
                    LoadingProcess = 40;
                    /// 현재 상태는 변환 중으로 표시.
                    State = ThreeMFState.ConvertingToMeshData;
                    modelMeshes.Unit = sourceUnit;
                    supportMeshes.Unit = sourceUnit;

                    /// cBuildIter가 BuildItem 순환하면서 다음거를 가져올 수 있는지 확인. 없으면 false 반환되면서 반복문 종료.
                    while(cBuildIter.MoveNext())
                    {
                        /// 현재 BuildItem 가져오기
                        using(CBuildItem cBuild = cBuildIter.GetCurrent())
                        {
                            /// BuildItem의 Transform 행렬 가져오기
                            var transform = ConvertToMatrix(cBuild.GetObjectTransform());
                            /// BuildItem의 ObjectResource 가져오기
                            using(CObject cObject = cBuild.GetObjectResource())
                            {
                                /// ObjectResource가 MeshObject가 아니면, 다음 BuildItem으로 넘어가고, 진행률 업데이트
                                if(!cObject.IsMeshObject())
                                {
                                    currentLayer++;
                                    if(meshesToLoad > 0)
                                    {
                                        double percent = Convert.ToDouble(((double)currentLayer / (double)meshesToLoad));
                                        LoadingProcess = 40 + Math.Round(percent * 50, 1);
                                    }
                                    continue;
                                }
                                /// CMeshObject로 캐스팅. CMeshObject가 아니면 예외 처리
                                CMeshObject cMesh = cObject as CMeshObject;
                                if(cMesh is null)
                                {
                                    throw new InvalidOperationException("Mesh iterator ended before build item iteration.");
                                }
                                /// MeshData로 변환
                                var meshData = ConvertToMeshData(cMesh, transform, sourceUnit);
                                meshData.Name = cObject.GetName();
                                meshData.LayerIndex = currentLayer;

                                /// Model3DData에 추가. cObject Type에 따라 Model/Support 구분
                                if(cObject.GetType().Equals(eObjectType.Model))
                                {
                                    AddToModel(modelMeshes, meshData, currentLayer);
                                }
                                else
                                {
                                    AddToModel(supportMeshes, meshData, currentLayer);
                                }
                                /// 변환 중인 레이어 증가
                                currentLayer++;
                                /// 진행률 업데이트
                                if(meshesToLoad > 0)
                                {
                                    double percent = Convert.ToDouble(((double)currentLayer / (double)meshesToLoad));
                                    LoadingProcess = 40 + Math.Round(percent * 50, 1);
                                }
                            }
                        }
                    }
                }
                /// 모든 BuildItem 순회 완료 후 상태를 완료로 변경하고, 진행률 100%로 설정
                LoadingProcess = 100;
            }
            finally
            {
                try
                {
                    /// CModel 객체 해제
                    if(cModel != null)
                    {
                        cModel.Dispose();
                        Debug.WriteLine("Disposed");
                    }
                }
                finally
                {
                    lock(loadLock)
                    {
                        State = ThreeMFState.Idle;
                    }
                }
            }

            return (modelMeshes, supportMeshes);
        }


        private void OnReadProgress(double progress, int stage)
        {
            if(double.IsNaN(progress) || double.IsInfinity(progress) || progress < 0)
            {
                return;
            }

            double mappedProgress = 10.0 + Math.Min(Math.Max(progress, 0.0), 1.0) * 30.0;

            double nextProgress = Math.Floor(mappedProgress);

            if(nextProgress > LoadingProcess)
            {
                LoadingProcess = nextProgress;
                Debug.WriteLine($"3MF Reading: {progress:P0} " +
                $"stage={stage}, overall={LoadingProcess:F0}%");
            }
        }

        /// <summary>
        /// lib3mf.dll 에서 쓰는 sTransform 데이터를 4x4 행렬로 변환하는 함수
        /// </summary>
        /// <param name="trans">변환할 sTransform 데이터</param>
        /// <returns>변환된 4x4 행렬</returns>
        private Matrix4x4 ConvertToMatrix(sTransform trans)
        {
            return new Matrix4x4(
               trans.Fields[0][0], trans.Fields[0][1], trans.Fields[0][2], 0f,
               trans.Fields[1][0], trans.Fields[1][1], trans.Fields[1][2], 0f,
               trans.Fields[2][0], trans.Fields[2][1], trans.Fields[2][2], 0f,
               trans.Fields[3][0], trans.Fields[3][1], trans.Fields[3][2], 1f
               );
        }

        /// <summary>
        /// CMeshObject 데이터를 MeshData로 변환하는 함수
        /// </summary>
        /// <param name="mesh">변환할 CMeshObject</param>
        /// <param name="transform">적용할 변환 행렬</param>
        /// <param name="sourceUnit">원본 단위</param>
        /// <returns>변환된 MeshData</returns>
        private MeshData ConvertToMeshData(CMeshObject mesh, Matrix4x4 transform, Unit sourceUnit)
        {
            /// CMeshObject에서 Vertex와 Triangle 데이터를 가져온다.
            var (positions, indices) = ReadMeshArrays(mesh);

            /// Renderer에 장비 크기를 알리지 않도록 모든 좌표를 millimeter 기준으로 통일한다.
            var unitScale = Matrix4x4.CreateScale(GetMillimeterScale(sourceUnit));
            var fullTransform = transform * unitScale;

            /// 기존 positions 배열의 각 좌표에 변환 행렬을 적용한다.
            for(int i = 0; i < positions.Length; i++)
            {
                positions[i] = Vector3.Transform(positions[i], fullTransform);
            }

            /// Triangle 데이터를 이용하여 각 Vertex의 Normal을 계산한다.
            var normals = CalculateNormals(positions, indices);

            /// 계산된 MeshData 반환
            return new MeshData
            {
                Positions = positions,
                Indices = indices,
                Normals = normals,
                Bounds = CalculateBounds(positions),
                MaterialIndex = -1
            };
        }

        /// <summary>
        /// Model3DData에 MeshData를 추가하는 함수
        /// </summary>
        /// <param name="modelData">추가할 대상 Model3DData</param>
        /// <param name="meshData">추가할 MeshData</param>
        /// <param name="layerIndex">추가할 레이어 인덱스</param>
        private static void AddToModel(Model3DData modelData, MeshData meshData, int layerIndex)
        {
            /// 현재 Mesh 수를 이용하여 MeshIndex를 계산하고, MeshData를 Model3DData에 추가한다.
            int meshIndex = modelData.Meshes.Count;
            modelData.Meshes.Add(meshData);

            modelData.Objects.Add(new ModelObjectData
            {
                Name = meshData.Name,
                MeshIndex = meshIndex,
                Transform = Matrix4x4.Identity,
                MaterialIndexOverride = meshData.MaterialIndex,
                LayerIndex = layerIndex
            });

            if(modelData.Meshes.Count == 1)
                modelData.Bounds = meshData.Bounds;
            else
                modelData.Bounds = MergeBounds(modelData.Bounds, meshData.Bounds);
        }

        private static Bounds3D MergeBounds(Bounds3D current, Bounds3D incoming)
        {
            if(current.IsEmpty)
                return incoming;
            if(incoming.IsEmpty)
                return current;

            var currentMin = current.Center - current.Size / 2f;
            var currentMax = current.Center + current.Size / 2f;
            var incomingMin = incoming.Center - incoming.Size / 2f;
            var incomingMax = incoming.Center + incoming.Size / 2f;

            var min = Vector3.Min(currentMin, incomingMin);
            var max = Vector3.Max(currentMax, incomingMax);
            return new Bounds3D(min, max);
        }

        private static Bounds3D CalculateBounds(Vector3[] positions)
        {
            if(positions.Length == 0)
            {
                return default;
            }

            var min = positions[0];
            var max = positions[0];

            foreach(var position in positions)
            {
                min = Vector3.Min(min, position);
                max = Vector3.Max(max, position);
            }

            return new Bounds3D(min, max);
        }

        //private static float GetMillimeterScale(Unit unit) => unit switch
        //{
        //    Unit.um => 0.001f,
        //    Unit.mm => 1f,
        //    Unit.cm => 10f,
        //    Unit.m => 1_000f,
        //    _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
        //};

        private static float GetMillimeterScale(Unit unit)
        {
            switch(unit)
            {
                case Unit.um : return 0.001f;
                case Unit.mm : return 1f;
                case Unit.cm : return 10f;
                case Unit.m : return 1_000f;
                default: throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }
        private Vector3[] CalculateNormals(Vector3[] positions, uint[] indices)
        {
            /// indices 크기가 3(혹은 3의 배수)인지 확인.
            if(indices.Length % 3 != 0)
            {
                throw new ArgumentException("Index count must be a multiple of 3");
            }

            var normals = new Vector3[positions.Length];

            for(int i = 0; i < indices.Length; i += 3)
            {
                uint i0 = indices[i];
                uint i1 = indices[i + 1];
                uint i2 = indices[i + 2];

                /// Vertex index가 범위 밖인지 검사
                if(i0 >= (uint)positions.Length ||
                   i1 >= (uint)positions.Length ||
                   i2 >= (uint)positions.Length)
                {
                    throw new InvalidOperationException("Mesh contains an out-of-range vertex index");
                }

                var edge1 = positions[i1] - positions[i0];
                var edge2 = positions[i2] - positions[i0];
                var faceNormal = Vector3.Cross(edge1, edge2);

                // 누적
                normals[i0] += faceNormal;
                normals[i1] += faceNormal;
                normals[i2] += faceNormal;
            }
            // 정규화
            for(int i = 0; i < normals.Length; i++)
            {
                normals[i] = normals[i] != Vector3.Zero ? Vector3.Normalize(normals[i]) : Vector3.UnitY;
            }
            return normals;
        }

        /// <summary>
        /// 네이티브 메쉬 데이터를 최종 Vector3[], uint[] 배열로 직접 복사한다.
        /// 중간 구조체 배열과 정점·삼각형별 작은 배열 생성을 생략하여
        /// 일시적인 메모리 사용량과 GC 부담을 줄인다.
        /// Copies native mesh data directly into the final arrays,
        /// avoiding intermediate arrays and reducing allocation pressure.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        private static unsafe (Vector3[] Positions, uint[] Indices) ReadMeshArrays(CMeshObject mesh)
        {
            IntPtr handle = mesh.GetHandle();

            if(handle == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(mesh));
            }
            /// Native vertex layout과 Vector3 구조체의 메모리 레이아웃이 일치하는지 확인
            if(sizeof(Vector3) != 3 * sizeof(float))
            {
                throw new NotSupportedException("Vector3 layout does not match the native vertex layout");
            }
            /// 필요한 Vertex 수와 Triangle 수만 조회
            CheckNativeResult(Lib3mfWrapper.MeshObject_GetVertices(handle, 0, out ulong vertexCount, IntPtr.Zero));

            CheckNativeResult(Lib3mfWrapper.MeshObject_GetTriangleIndices(handle, 0, out ulong triangleCount, IntPtr.Zero));
            /// 배열 길이 제한 확인. .NET에서 지원하는 최대 배열 길이를 초과하면 예외 발생
            const int maxMeshArrayLength = 0X7FEFFFFF;
            if(vertexCount > (ulong)maxMeshArrayLength || triangleCount > (ulong)maxMeshArrayLength / 3)
            {
                throw new InvalidOperationException("Mesh data exceeds the supported array length");
            }

            var positions = new Vector3[checked((int)vertexCount)];
            var indices = new uint[checked((int)(triangleCount * 3))];

            if(positions.Length > 0)
            {
                /// 호출 중에 GC가 배열을 이동시키지 못하도록 고정하는 키워드.
                fixed(Vector3* pointer = positions)
                {
                    CheckNativeResult(Lib3mfWrapper.MeshObject_GetVertices(handle, vertexCount, out ulong actualVertexCount, (IntPtr)pointer));

                    if(actualVertexCount != vertexCount)
                    {
                        throw new InvalidOperationException("Vertex count changed while reading the mesh.");
                    }
                }
            }

            if(indices.Length > 0)
            {
                /// 호출 중에 GC가 배열을 이동시키지 못하도록 고정하는 키워드.
                fixed(uint* pointer = indices)
                {
                    CheckNativeResult(Lib3mfWrapper.MeshObject_GetTriangleIndices(handle, triangleCount, out ulong actualTriangleCount, (IntPtr)pointer));

                    if(actualTriangleCount != triangleCount)
                    {
                        throw new InvalidOperationException("Triangle count changed while reading the mesh.");
                    }
                }
            }

            return (positions, indices);
        }

        private static void CheckNativeResult(int errorCode)
        {
            if(errorCode != 0)
            {
                throw new InvalidOperationException($"lib3mf mesh extraction failed. Error code:{errorCode}");
            }
        }
    }

    public enum ThreeMFState
    {
        Idle,
        Reading,
        ConvertingToMeshData        
    }
}
