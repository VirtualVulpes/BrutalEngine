using Brutal.Numerics;

namespace HelloBrutal;

public class Camera
{
    public float3 Position = new(0.0f, 0.0f, 3.0f);
    private float _yaw = float.DegreesToRadians(-90.0f);
    private float _pitch = 0.0f;
    public float AspectRatio = (float)800 / 600;
    public float Fov = float.DegreesToRadians(60.0f);

    private float3 _forward = new(0.0f, 0.0f, -1.0f);
    private float3 _right = float3.UnitX;

    private float _zNear = 0.1f;
    private float _zFar = 100.0f;

    public float4x4 GetView()
    {
        return float4x4.CreateLookAt(Position, Position + _forward, float3.UnitY);
    }

    public float4x4 GetProjection()
    {
        return float4x4.CreatePerspectiveFieldOfView(Fov, AspectRatio, _zNear, _zFar);
    }

    public void MoveRelative(float3 localDelta)
    {
        Position += _right * localDelta.X + float3.UnitY * localDelta.Y + _forward * localDelta.Z;
    }

    public void Rotate(float yawDelta, float pitchDelta)
    {
        _yaw += yawDelta;
        _pitch = float.Clamp(_pitch + pitchDelta, float.DegreesToRadians(-89.0f), float.DegreesToRadians(89.0f));
        UpdateVectors();
    }

    private void UpdateVectors()
    {
        _forward = float3.Normalize(new float3(float.Cos(_yaw) * float.Cos(_pitch), float.Sin(_pitch),
            float.Sin(_yaw) * float.Cos(_pitch)));
        _right = float3.Normalize(float3.Cross(_forward, float3.UnitY));
    }
}