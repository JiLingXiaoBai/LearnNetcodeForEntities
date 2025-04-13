using System.Drawing;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public partial struct InitializeCharacterSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (physicsMass, mobaTeam, newCharacterEntity) in SystemAPI
                     .Query<RefRW<PhysicsMass>, RefRO<MobaTeam>>().WithAny<NewChampTag>().WithEntityAccess())
        {
            physicsMass.ValueRW.InverseInertia[0] = 0;
            physicsMass.ValueRW.InverseInertia[1] = 0;
            physicsMass.ValueRW.InverseInertia[2] = 0;

            var teamColor = mobaTeam.ValueRO.Value switch
            {
                TeamType.Blue => new float4(0, 0, 1, 1),
                TeamType.Red => new float4(1, 0, 0, 1),
                _ => new float4(1)
            };
            
            ecb.SetComponent(newCharacterEntity, new URPMaterialPropertyBaseColor {Value = teamColor});
            ecb.RemoveComponent<NewChampTag>(newCharacterEntity);
        }
        
        ecb.Playback(state.EntityManager);
    }
}