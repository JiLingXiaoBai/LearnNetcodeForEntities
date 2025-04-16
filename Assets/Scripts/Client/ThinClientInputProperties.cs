using Unity.Entities;
using Unity.Mathematics;

public struct ThinClientInputProperties : IComponentData
{
    public Random Random;
    public float Timer;
    public float MinTimer;
    public float MaxTimer;
    public float3 MinPosition;
    public float3 MaxPosition;
}