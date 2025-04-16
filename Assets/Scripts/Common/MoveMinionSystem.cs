using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct MoveMinionSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, pathPositions, pathIndex, moveSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, DynamicBuffer<MinionPathPosition>, RefRW<MinionPathIndex>,
                         RefRO<CharacterMoveSpeed>>().WithAll<Simulate>())
        {
            var curTargetPosition = pathPositions[pathIndex.ValueRO.Value].Value;
            if (math.distance(curTargetPosition, transform.ValueRO.Position) <= 1.5f)
            {
                if (pathIndex.ValueRO.Value >= pathPositions.Length - 1) continue;
                pathIndex.ValueRW.Value++;
                curTargetPosition = pathPositions[pathIndex.ValueRO.Value].Value;
            }
            curTargetPosition.y = transform.ValueRO.Position.y;
            var curHeading = math.normalizesafe(curTargetPosition - transform.ValueRO.Position);
            transform.ValueRW.Position += curHeading * moveSpeed.ValueRO.Value * deltaTime;
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(curHeading, math.up());
        }
    }
}