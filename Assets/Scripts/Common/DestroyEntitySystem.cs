using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
public partial struct DestroyEntitySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var networkTime = SystemAPI.GetSingleton<NetworkTime>();
        if (!networkTime.IsFirstTimeFullyPredictingTick) return;
        var currentTick = networkTime.ServerTick;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                     .WithAll<DestroyEntityTag, Simulate>().WithEntityAccess())
        {
            if (state.World.IsServer())
            {
                ecb.DestroyEntity(entity);
            }
            else
            {
                transform.ValueRW.Position = new float3(1000f, 1000f, 1000f);
            }
        }
    }
}