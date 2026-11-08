using System;
using System.Collections.Generic;
using System.Numerics;

namespace Lincsolution.Core.Rendering
{
    public sealed class Model3DData
    {
        public List<MeshData> Meshes { get; } = new List<MeshData>();
        public List<ModelObjectData> Objects { get; } = new List<ModelObjectData>();
        public List<MaterialData> Materials { get; } = new List<MaterialData>();
        public List<TextureData> Textures { get; } = new List<TextureData>();
        public Unit Unit { get; set; } = Unit.mm;
        public Bounds3D Bounds { get; set; }
    }

    public sealed class MeshData
    {
        public string Name { get; set; } = string.Empty;
        public Vector3[] Positions { get; set; } = Array.Empty<Vector3>();
        public uint[] Indices { get; set; } = Array.Empty<uint>();
        public Vector3[] Normals { get; set; } = Array.Empty<Vector3>();
        public Vector2[] TextureCoordinates { get; set; } = Array.Empty<Vector2>();
        public Bounds3D Bounds { get; set; }
        public int MaterialIndex { get; set; } = -1;
        public int? LayerIndex { get; set; }
    }

    public sealed class ModelObjectData
    {
        public string Name { get; set; } = string.Empty;
        public int MeshIndex { get; set; } = -1;
        public Matrix4x4 Transform { get; set; } = Matrix4x4.Identity;
        public int? MaterialIndexOverride { get; set; }
        public int? LayerIndex { get; set; }
    }

    /// <summary>표면 조명 방식. Surface lighting selection.</summary>
    public enum SurfaceShading
    {
        /// <summary>기본 파트의 플라스틱 조명. Plastic lighting.</summary>
        Plastic,
        /// <summary>반사광 없는 단순 조명. Diffuse lighting without highlights.</summary>
        Diffuse
    }

    public sealed class MaterialData
    {
        /// <summary>표시 또는 진단 시 사용할 재질 이름. Material label.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>RGBA 기본색. 각 성분 0~1, W는 불투명도. Base color and opacity.</summary>
        public Vector4 DiffuseColor { get; set; } = Vector4.One;
        public int? TextureIndex { get; set; }
        /// <summary>표면 조명 종류. 파일 형식과 독립적인 선택. File-independent shading mode.</summary>
        public SurfaceShading Shading { get; set; } = SurfaceShading.Plastic;
        /// <summary>흰색 반사광 강도, 0~1. White specular strength.</summary>
        public float SpecularStrength { get; set; } = 0.3f;
        /// <summary>반사광 집중 지수, 1~256. 값이 클수록 좁은 광택. Specular exponent.</summary>
        public float Shininess { get; set; } = 48f;
    }

    public sealed class TextureData
    {
        public string Name { get; set; } = string.Empty;
        public string Uri { get; set; }
    }

    public readonly struct Bounds3D
    {
        public Vector3 Center { get; }
        public Vector3 Size { get; }
        public bool IsEmpty { get; }

        public Bounds3D(Vector3 Minimum, Vector3 Maximum)
        {
            Center = (Minimum + Maximum) * 0.5f;
            Size = Maximum - Minimum;
            IsEmpty = (Minimum == Maximum);
        }
    }

    public enum Unit
    {
        um,
        mm,
        cm,
        m
    }
}
