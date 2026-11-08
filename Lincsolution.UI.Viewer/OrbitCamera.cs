using System;
using System.Numerics;
using Lincsolution.Core.Rendering;

namespace Lincsolution.UI.Viewer;

/// <summary>
/// <para>객체 중심의 궤도 카메라(Orbit camera) 동작을 담당하는 내부 유틸리티입니다.</para>
/// <para>Handles orbit-style camera controls around a target point for model viewing.</para>
/// </summary>
internal sealed class OrbitCamera
{
    /// <summary>
    /// <para>기본 투영 시야각(rad).</para>
    /// <para>Default perspective field of view in radians.</para>
    /// </summary>
    private const float DefaultFieldOfView = MathF.PI / 4f;

    private const float AngleClampMin = 0.1f;
    private const float AngleClampMax = 179.9f;
    private const float NormalizationEpsilon = 1e-6f;
    private const float DefaultUpScale = 1f;

    private Vector3 upAxis = Vector3.UnitZ;

    /// <summary>
    /// <para>초기 반지름(거리) 복원용 값.</para>
    /// <para>Cached initial radius value used by Reset().</para>
    /// </summary>
    private readonly float initialRadius;

    /// <summary>
    /// <para>초기 위/아래 각도(세타) 복원용 값.</para>
    /// <para>Cached initial theta value used by Reset().</para>
    /// </summary>
    private readonly float initialTheta;

    /// <summary>
    /// <para>초기 좌/우 각도(파이) 복원용 값.</para>
    /// <para>Cached initial phi value used by Reset().</para>
    /// </summary>
    private readonly float initialPhi;

    /// <summary>
    /// <para>
    /// 카메라를 생성하고 기본 상태(거리, 세타, 파이)를 저장한 뒤 <see cref="Reset"/>을 호출합니다.
    /// </para>
    /// <para>
    /// Creates an orbit camera and stores initial state values, then calls <see cref="Reset"/>.
    /// </para>
    /// </summary>
    /// <param name="radius">초기 카메라 반경/거리. Initial camera radius.</param>
    /// <param name="theta">초기 세타(도), 0~180° 근방을 권장. Initial theta in degrees.</param>
    /// <param name="phi">초기 파이(도), 0~360° 자동 정규화. Initial phi in degrees.</param>
    /// <summary>
    /// <para>업 벡터의 기본값(로컬 Z-up)입니다.</para>
    /// <para>Default up axis (Z-up).</para>
    /// </summary>
    public static readonly Vector3 DefaultUpAxis = Vector3.UnitZ;

    /// <summary>
    /// <para>기본 줌 속도 배율입니다.</para>
    /// <para>Default zoom scale.</para>
    /// </summary>
    public float ZoomSpeed { get; set; } = DefaultUpScale;

    /// <summary>
    /// <para>기본 팬 속도 배율입니다.</para>
    /// <para>Default pan scale.</para>
    /// </summary>
    public float PanSpeed { get; set; } = DefaultUpScale;

    /// <summary>
    /// <para>회전 속도 배율입니다.</para>
    /// <para>Rotation scale.</para>
    /// </summary>
    public float RotateSpeed { get; set; } = 1f;

    /// <summary>
    /// <para>카메라 거리 최소값.</para>
    /// <para>Minimum allowed radius value.</para>
    /// </summary>
    public float MinRadius { get; set; } = 0.001f;

    /// <summary>
    /// <para>카메라 거리 최대값.</para>
    /// <para>Maximum allowed radius value.</para>
    /// </summary>
    public float MaxRadius { get; set; } = 1_000_000f;

    /// <summary>
    /// <para>카메라 업 벡터를 설정합니다.</para>
    /// <para>Sets up axis used by View matrix calculations.</para>
    /// </summary>
    public Vector3 UpAxis
    {
        get => upAxis;
        set => upAxis = NormalizeAxis(value);
    }

    /// <summary>
    /// <para>
    /// 카메라를 생성하고 기본 상태(거리, 세타, 파이)를 저장한 뒤 <see cref="Reset"/>을 호출합니다.
    /// </para>
    /// <para>
    /// Creates an orbit camera and stores initial state values, then calls <see cref="Reset"/>.
    /// </para>
    /// </summary>
    /// <param name="radius">초기 카메라 반경/거리. Initial camera radius.</param>
    /// <param name="theta">초기 세타(도), 0~180° 근방을 권장. Initial theta in degrees.</param>
    /// <param name="phi">초기 파이(도), 0~360° 자동 정규화. Initial phi in degrees.</param>
    /// <param name="upAxis">카메라 기준 업 벡터. 기본값은 Z-up입니다.</param>
    public OrbitCamera(float radius = 1f, float theta = 40f, float phi = 270f, Vector3? upAxis = null)
    {
        initialRadius = radius;
        initialTheta = theta;
        initialPhi = phi;
        UpAxis = upAxis ?? DefaultUpAxis;
        Reset();
    }

    /// <summary>
    /// <para>카메라 중심(타깃)로부터의 거리(반지름)입니다.</para>
    /// <para>Distance from target center to camera position.</para>
    /// </summary>
    public float Radius { get; private set; }

    /// <summary>
    /// <para>수직 회전 각도(세타, 도 단위). 위/아래 방향 제어에 사용됩니다.</para>
    /// <para>Vertical orbit angle in degrees used for elevation.</para>
    /// </summary>
    public float Theta { get; private set; }

    /// <summary>
    /// <para>수평 회전 각도(파이, 도 단위). 좌우 회전 방향 제어에 사용됩니다.</para>
    /// <para>Horizontal orbit angle in degrees used for azimuth.</para>
    /// </summary>
    public float Phi { get; private set; }

    /// <summary>
    /// <para>현재 관찰 대상의 중심점입니다.</para>
    /// <para>Current target center point.</para>
    /// </summary>
    public Vector3 Center { get; private set; }

    /// <summary>
    /// <para>타깃 중심에서의 이동 오프셋입니다.</para>
    /// <para>Pan offset applied to the target center.</para>
    /// </summary>
    public Vector3 Offset { get; private set; }

    /// <summary>
    /// <para>
    /// 현재 카메라 월드 좌표입니다. <see cref="Radius"/>, <see cref="Theta"/>, <see cref="Phi"/>로 계산합니다.
    /// </para>
    /// <para>
    /// Current world-space camera position calculated from radius, theta, and phi.
    /// </para>
    /// </summary>
    public Vector3 Position
    {
        get
        {
            float theta = MathF.PI * Theta / 180f;
            float phi = MathF.PI * Phi / 180f;

            return Target + new Vector3(
                Radius * MathF.Sin(theta) * MathF.Cos(phi),
                Radius * MathF.Sin(theta) * MathF.Sin(phi),
                Radius * MathF.Cos(theta));
        }
    }

    /// <summary>
    /// <para>현재 타깃(중심 + 오프셋)입니다.</para>
    /// <para>Current target point.</para>
    /// </summary>
    public Vector3 Target => Center + Offset;

    /// <summary>
    /// <para>현재 뷰 행렬(월드에서 뷰 공간으로)입니다.</para>
    /// <para>Current view matrix from world to camera space.</para>
    /// </summary>
    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Target, GetUpDirection());

    /// <summary>
    /// <para>현재 종횡비 기준의 투영 행렬을 반환합니다.</para>
    /// <para>Returns projection matrix using the current aspect ratio.</para>
    /// </summary>
    /// <param name="aspectRatio">뷰포트 너비/높이 비율. Viewport width / height.</param>
    /// <returns>원근 투영 행렬.</returns>
    public Matrix4x4 GetProjectionMatrix(float aspectRatio)
    {
        float nearPlane = Math.Max(Radius / 1_000f, MinRadius);
        float farPlane = Math.Max(Radius * 10f, nearPlane + 1f);

        return Matrix4x4.CreatePerspectiveFieldOfView(
            DefaultFieldOfView,
            Math.Max(aspectRatio, 0.001f),
            nearPlane,
            farPlane);
    }

    /// <summary>
    /// <para>세타/파이 각도를 증분값만큼 회전시킵니다.</para>
    /// <para>Rotates orbit angles by deltas.</para>
    /// </summary>
    /// <param name="deltaTheta">세타 증분(도). Delta theta in degrees.</param>
    /// <param name="deltaPhi">파이 증분(도). Delta phi in degrees.</param>
    public void Rotate(float deltaTheta, float deltaPhi)
    {
        Theta = Math.Clamp(Theta + (deltaTheta * RotateSpeed), AngleClampMin, AngleClampMax);
        Phi = NormalizeAngle(Phi + deltaPhi * RotateSpeed);
    }

    /// <summary>
    /// <para>카메라가 바라보는 평면 기준으로 대상 중심을 이동시킵니다.</para>
    /// <para>Pans the target position on the camera's local right/up plane.</para>
    /// </summary>
    /// <param name="deltaX">로컬 오른쪽 방향 이동량.</param>
    /// <param name="deltaY">로컬 위쪽 방향 이동량.</param>
    public void Pan(float deltaX, float deltaY)
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, upAxis));
        Vector3 up = Vector3.Normalize(Vector3.Cross(right, forward));

        Offset += (right * (deltaX * PanSpeed)) + (up * (deltaY * PanSpeed));
    }

    /// <summary>
    /// <para>뷰포트 픽셀 기반으로 판 이동량을 보정해 적용합니다.</para>
    /// <para>Pans using viewport-corrected movement amounts.</para>
    /// </summary>
    /// <param name="deltaX">스크롤/드래그 가로 픽셀 이동량.</param>
    /// <param name="deltaY">스크롤/드래그 세로 픽셀 이동량.</param>
    /// <param name="viewportWidth">뷰포트 너비.</param>
    /// <param name="viewportHeight">뷰포트 높이.</param>
    /// <param name="sensitivity">추가 배율.</param>
    public void Pan(float deltaX, float deltaY, float viewportWidth, float viewportHeight, float sensitivity = 1f)
    {
        if (viewportWidth <= 0f || viewportHeight <= 0f)
        {
            return;
        }

        float heightScale = 2f * Radius * MathF.Tan(DefaultFieldOfView * 0.5f) / viewportHeight;
        float widthScale = heightScale * MathF.Max(viewportWidth / viewportHeight, NormalizationEpsilon);
        Pan(deltaX * (-widthScale) * sensitivity, deltaY * heightScale * sensitivity);
    }

    /// <summary>
    /// <para>거리(반지름)만 변경해 줌 동작을 수행합니다.</para>
    /// <para>Adjusts only the radius to implement zoom.</para>
    /// </summary>
    /// <param name="delta">거리 증분(양수 확대, 음수 축소).</param>
    public void Zoom(float delta)
    {
        Radius = Math.Clamp(Radius + delta * ZoomSpeed, MinRadius, MaxRadius);
    }

    /// <summary>
    /// <para>지수 스케일 줌(휠 입력에서 권장)입니다.</para>
    /// <para>Relative/ exponential zoom commonly used with mouse-wheel input.</para>
    /// </summary>
    /// <param name="delta">휠 델타 같은 입력 값.</param>
    /// <param name="speed">감도, 기본 0.1.</param>
    public void ZoomRelative(float delta, float speed = 0.1f)
    {
        if (MathF.Abs(speed) <= NormalizationEpsilon)
        {
            return;
        }

        float factor = MathF.Exp(-delta * speed);
        Radius = Math.Clamp(Radius * factor, MinRadius, MaxRadius);
    }

    /// <summary>
    /// <para>주어진 바운딩 박스에 맞춰 타깃 중심과 거리를 재설정합니다.</para>
    /// <para>Fits the camera to a target bounds by updating center and radius.</para>
    /// </summary>
    /// <param name="bounds">맞출 바운딩 데이터.</param>
    /// <param name="margin">여백 비율(0~1). Default 0.1.</param>
    public void Fit(Bounds3D bounds, float margin = 0.1f)
    {
        if (bounds.IsEmpty)
        {
            return;
        }

        Center = bounds.Center;
        Offset = Vector3.Zero;
        float fitRadius = MathF.Max(bounds.Size.Length() * 0.5f, MinRadius);
        Radius = Math.Clamp(fitRadius / MathF.Max(MathF.Sin(DefaultFieldOfView * 0.5f), NormalizationEpsilon), MinRadius, MaxRadius) * (1f + Math.Clamp(margin, 0f, 0.95f));
    }

    /// <summary>
    /// <para>카메라 상태를 생성 시 저장한 기본 값으로 되돌립니다.</para>
    /// <para>Resets camera state to construction defaults.</para>
    /// </summary>
    /// <param name="center">필요 시 새 타깃 중심으로 초기화.</param>
    public void Reset(Vector3? center = null)
    {
        Radius = Math.Clamp(initialRadius, MinRadius, MaxRadius);
        Theta = Math.Clamp(initialTheta, AngleClampMin, AngleClampMax);
        Phi = NormalizeAngle(initialPhi);
        Center = center ?? Vector3.Zero;
        Offset = Vector3.Zero;
    }

    /// <summary>
    /// <para>카메라 뷰 계산에 사용할 업 벡터를 산출합니다.</para>
    /// <para>Builds a stable up vector for view matrix calculation.</para>
    /// </summary>
    private Vector3 GetUpDirection()
    {
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 right = Vector3.Cross(forward, upAxis);
        if (right.LengthSquared() <= NormalizationEpsilon)
        {
            right = Vector3.Cross(forward, Vector3.UnitY);
        }

        right = Vector3.Normalize(right);
        return Vector3.Normalize(Vector3.Cross(right, forward));
    }

    private static Vector3 NormalizeAxis(Vector3 axis)
    {
        if (axis.LengthSquared() <= NormalizationEpsilon)
        {
            return DefaultUpAxis;
        }

        return Vector3.Normalize(axis);
    }

    /// <summary>
    /// <para>각도를 0~360° 범위로 정규화합니다.</para>
    /// <para>Normalizes angle to a 0~360 degree range.</para>
    /// </summary>
    private static float NormalizeAngle(float angle) => (angle % 360f + 360f) % 360f;
}
