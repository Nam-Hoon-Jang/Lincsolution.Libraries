using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Numerics;

namespace Lincsolution.Core.Rendering.STL
{
    /// <summary>
    /// .stl 파일 처리를 위한 클래스.
    /// </summary>
    public sealed class Stl
    {
        /// <summary>Binary STL 헤더 크기. byte 단위</summary>
        private const int headerSize = 80;

        /// <summary>STL 파일 내부 Triangle 개수를 저장하는 uint 크기. byte 단위</summary>
        private const int triangleCountSize = sizeof(uint);

        /// <summary>Normal, Vertex 3개, Property를 포함한 Triangle Record의 크기.</summary>
        private const int triangleRecordSize = 50;

        /// <summary>
        /// Binary STL 파일을 열어 Triangle 개수와 파일 길이 검증.
        /// </summary>
        /// <param name="filePath">검사할 STL 파일 경로.</param>
        /// <returns>파일에 기록된 Triangle 개수.</returns>
        public uint ReadBinaryTriangleCount(string filePath)
        {
            /// 파일 경로 검사
            if(string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException($"There is no such .stl file:{nameof(filePath)}");

            /// ReadOnly로 파일 열기
            using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                /// 기본 자료형을 읽을 Reader
                using(var reader = new BinaryReader(stream))
                {
                    return ReadBinaryTriangleCount(reader);
                }
            }

        }


        public Model3DData LoadBinary(string filePath)
        {
            /// 파일 경로 검사
            if(string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException($"There is no such .stl file:{nameof(filePath)}");

            /// ReadOnly로 파일 열기
            using(var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                /// Binary STL 읽을 Reader 생성
                using(var reader = new BinaryReader(stream))
                {
                    /// stl 파일 검증하고, 첫 Triangle 위치로 reader 읽는 위치 이동.
                    uint triangleCount = ReadBinaryTriangleCount(reader);

                    /// Triangle마다 독립된 Vertex 3개 필요하므로 3 곱하고, overflow 방지를 위해 long으로 생성.
                    long requiredVertices = (long)triangleCount * 3;
                    /// 필요한 Vertices 갯수가 배열 최대크기보다 큰 경우, 메모리 부족 가능성 있어서 예외 처리.
                    const int maxMeshArrayLength = 0X7FEFFFFF;
                    if(requiredVertices > maxMeshArrayLength)
                        throw new NotSupportedException("The .stl file contains too many vertices for a single mesh.");

                    /// 배열 생성에 쓸 int 길이로 데이터 변환.
                    int vertexCount = (int)requiredVertices;

                    /// 최종 배열 생성.
                    var positions = new Vector3[vertexCount];
                    var normals = new Vector3[vertexCount];
                    var indices = new uint[vertexCount];

                    /// 모델 전체 경계값 저장용 변수.
                    var minimum = new Vector3(float.PositiveInfinity);
                    var maximum = new Vector3(float.NegativeInfinity);

                    /// 모든 Triangle에 대하여 계산.
                    for(int baseIndex = 0; baseIndex < vertexCount; baseIndex += 3)
                    {
                        /// 파일에 저장된 4byte Normal을 XYZ 좌표만큼 읽고 무시.
                        /// 잘못된 길이나 0으로 저장된 Normal값 버리고 Vertex에서 다시 계산하기 위함.
                        reader.ReadSingle();
                        reader.ReadSingle();
                        reader.ReadSingle();

                        /// Triangle Vertex 3개를 reader로 순서대로 읽기.
                        Vector3 p0 = ReadPosition(reader);
                        Vector3 p1 = ReadPosition(reader);
                        Vector3 p2 = ReadPosition(reader);

                        /// Triangle 데이터 끝에 들어가는 2byte 크기의 속성 데이터 읽기.
                        /// 지금은 일단 읽고 무시.
                        reader.ReadUInt16();

                        /// Vertex 순서대로 Face Normal 계산.
                        Vector3 normal = CalculateFaceNormal(p0, p1, p2);

                        /// 최종 Vertex Array에 p0, p1, p2 좌표 저장.
                        positions[baseIndex] = p0;
                        positions[baseIndex + 1] = p1;
                        positions[baseIndex + 2] = p2;

                        /// 같은 Triangle에 속한 Vertex들은 같은 Face Normal 사용.
                        normals[baseIndex] =
                        normals[baseIndex + 1] =
                        normals[baseIndex + 2] = normal;

                        /// 독립된 Vertex Array로 만드는 중이므로, index도 순서대로 연결.
                        indices[baseIndex] = (uint)baseIndex;
                        indices[baseIndex + 1] = (uint)(baseIndex + 1);
                        indices[baseIndex + 2] = (uint)(baseIndex + 2);

                        /// 계산된 Triangle 까지는 모델의 경계에 포함되어야 하므로, 
                        /// 최소, 최대 좌표값 갱신.
                        minimum = Vector3.Min(minimum, Vector3.Min(p0, Vector3.Min(p1, p2)));
                        maximum = Vector3.Max(maximum, Vector3.Max(p0, Vector3.Max(p1, p2)));
                    }

                    /// for루프 돌면서 계산된 최종 min/max 값으로 Bound 생성.
                    var bounds = new Bounds3D(minimum, maximum);

                    /// 단일 Mesh 정보 담을 모델 생성.
                    /// STL은 단위 정보 안 들어가므로, mm를 기본으로 설정.
                    var model = new Model3DData
                    {
                        Unit = Unit.mm,
                        Bounds = bounds
                    };

                    /// 생성한 Array 그대로 연결.
                    model.Meshes.Add(new MeshData
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        Positions = positions,
                        Normals = normals,
                        Indices = indices,
                        Bounds = bounds,
                        /// 기본 재질 사용
                        MaterialIndex = -1
                    });

                    /// 모델 반환
                    return model;
                }
            }
        }

        /// <summary>
        /// 열린 파일에서 Binary STL 헤더와 Triangle 개수 검증.
        /// 정상 반환 시 Reader가 다음에 읽는 위치는, 첫 Triangle이 시작되는 84바이트 지점.
        /// </summary>
        /// <param name="reader">처음부터 탐색 가능한 STL 파일 Reader. 이 함수에서는 해제하지 않음.</param>
        /// <returns>파일에 있는 Triangle 갯수</returns>
        /// <exception cref="InvalidDataException">파일 길이 또는 Triangle 개수가 잘못된 경우.</exception>
        private static uint ReadBinaryTriangleCount(BinaryReader reader)
        {
            /// reader에서 사용하는 파일스트림 조회.
            Stream stream = reader.BaseStream;

            /// 헤더와 Triangle 갯수를 읽는 데 필요한 최소 길이.
            const int mimimumSize = headerSize + triangleCountSize;

            /// 최소 파일 크기보다 작은 파일(깨졌거나 잘못된 파일) 검사
            if(stream.Length < mimimumSize)
                throw new InvalidDataException("The binary .stl file is too short to read.");
            
            /// 설명용 헤더 80바이트 건너뛰고, Triangle 갯수 위치로 이동.
            stream.Position = headerSize;

            /// Binary STL의 little-endian uint 읽어서 Triangle 갯수 확인.
            uint triangleCount = reader.ReadUInt32();

            /// Triangle 갯수가 0인지 검사.
            if(triangleCount == 0)
                throw new InvalidDataException("There is no Triangles in the file.");

            /// 예상되는 실제 파일 길이값 계산. uint보다 큰 값이라 long타입으로.
            long expectedSize = mimimumSize + (long)triangleCount * triangleRecordSize;

            /// 예상값과 실제 파일 길이비교.
            if(stream.Length != expectedSize)
                throw new InvalidDataException($"The length of Binary .stl file not matches." +
                                               $"Expected : {expectedSize}, Confirmed : {stream.Length}");

            return triangleCount;
        }

        /// <summary>
        /// xyz 좌표 3개를 연속으로 읽어서 유효한 Vertex 좌표 생성하는 함수.
        /// </summary>
        /// <param name="reader">현재 위치에서 X,Y,Z 순서로 읽을 BinaryReader</param>
        /// <returns>유효한 XYZ Vector3</returns>
        /// <exception cref="InvalidDataException">NaN 또는 무한대 좌표가 포함된 경우</exception>
        private static Vector3 ReadPosition(BinaryReader reader)
        {
            /// Binary .stl의 float 좌표값 xyz 순서대로 read
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();

            /// 카메라에 표시할 모델 경계 계산과 GPU Rendering 깨트리는 비정상 좌표 차단.
            if(float.IsNaN(x) || float.IsInfinity(x) ||
               float.IsNaN(y) || float.IsInfinity(y) ||
               float.IsNaN(z) || float.IsInfinity(z))
                throw new InvalidDataException("The STL contains a non-finite vertex.");
            
            /// 검사한 좌표 반환
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Vertex 순서에 따른 외적으로 단위 면의 Normal 계산.
        /// 
        /// </summary>
        /// <param name="p0">Triangle의 첫 번째 Vertex</param>
        /// <param name="p1">Triangle의 두 번째 Vertex</param>
        /// <param name="p2">Triangle의 세 번째 Vertex</param>
        /// <returns>Unit Normal. Triangle 면적이 0이면(일직선이거나 한 점인 경우), Vector3.Zero 반환</returns>
        private static Vector3 CalculateFaceNormal(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            /// float 그대로 썼다가 overflow 발생할 수 있으니, double로 변환해서 계산.
            /// 첫 번째 변 p0→p1 계산. 뺄셈 전 double로 변환.
            double ax = (double)p1.X - p0.X;
            double ay = (double)p1.Y - p0.Y;
            double az = (double)p1.Z - p0.Z;

            /// 두 번째 변 p0→p2 계산.
            double bx = (double)p2.X - p0.X;
            double by = (double)p2.Y - p0.Y;
            double bz = (double)p2.Z - p0.Z;

            /// 두 변의 외적으로 면에 수직인 방향 계산.
            double nx = ay * bz - az * by;
            double ny = az * bx - ax * bz;
            double nz = ax * by - ay * bx;

            /// 외적 벡터 길이 계산.
            double length = Math.Sqrt((nx * nx) + (ny * ny) + (nz * nz));
            
            /// 일직선이나 같은 Vertex로 이루어진 Triangle이 0으로 나눠지는 것 방지.
            /// 0으로 잘못 나누면 SyntaxError 발생.
            if(length == 0)
                return Vector3.Zero;

            /// 단위 길이로 Normalize 한 뒤 float형식으로 변환.
            /// GPU 입력 형식이 float이므로, float로 반환해야 함.
            return new Vector3((float)(nx / length), (float)(ny / length), (float)(nz / length));
        }

        /// <summary></summary>
    }
}
