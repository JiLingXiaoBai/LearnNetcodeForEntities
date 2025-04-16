using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct ChampMoveSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameplayingTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;

        foreach (var (transform, movePosition, moveSpeed) in SystemAPI
                     .Query<RefRW<LocalTransform>, RefRO<ChampMoveTargetPosition>, RefRO<CharacterMoveSpeed>>()
                     .WithAll<Simulate>())
        {
            var moveTarget = movePosition.ValueRO.Value;
            moveTarget.y = transform.ValueRO.Position.y;

            if (math.distancesq(transform.ValueRO.Position, moveTarget) < 0.001f) continue;
            var moveDirection = math.normalize(moveTarget - transform.ValueRO.Position);
            var moveVector = moveDirection * moveSpeed.ValueRO.Value * deltaTime;
            transform.ValueRW.Position += moveVector;
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(moveDirection, math.up());
        }
    }
}