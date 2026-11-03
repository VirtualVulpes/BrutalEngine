using Brutal.Numerics;

namespace BrutalEngine;

public struct Transform
{
    public Transform()
    {
        
    }
    
    public float3 Position = float3.Zero;
    public float3 Scale = float3.One;
    public float3 Rotation = float3.Zero;

    public float4x4 ModelMatrix
    {
        get
        {
            float4x4 model = float4x4.Identity;
            model *= float4x4.CreateTranslation(Position);
            model *= float4x4.CreateRotationX(Rotation.X);
            model *= float4x4.CreateRotationY(Rotation.Y);
            model *= float4x4.CreateRotationZ(Rotation.Z);
            model *= float4x4.CreateScale(Scale);
            return model;
        }
    }
}