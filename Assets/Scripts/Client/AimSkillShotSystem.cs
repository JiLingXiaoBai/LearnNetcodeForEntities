using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct AimSkillShotSystem : ISystem
{
    private CollisionFilter _selectionFilter;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MainCameraTag>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        _selectionFilter = new CollisionFilter
        {
            BelongsTo = 1 << 5, // Ray Casts
            CollidesWith = 1 << 0, // GroundPlane
        };
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (aimInput, transform, skillShotUIReference) in SystemAPI
                     .Query<RefRW<AimInput>, RefRO<LocalTransform>, SkillShotUIReference>()
                     .WithAll<AimSkillShotTag, OwnerChampTag>())
        {
            skillShotUIReference.Value.transform.position = transform.ValueRO.Position;
            
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var cameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
            var mainCamera = state.EntityManager.GetComponentObject<MainCamera>(cameraEntity).Value;

            var mousePosition = Input.mousePosition;
            mousePosition.z = 1000f;
            var worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);

            var selectionInput = new RaycastInput
            {
                Start = mainCamera.transform.position,
                End = worldPosition,
                Filter = _selectionFilter,
            };

            if (collisionWorld.CastRay(selectionInput, out var closestHit))
            {
                var directionToTarget = closestHit.Position - transform.ValueRO.Position;
                directionToTarget.y = transform.ValueRO.Position.y;
                directionToTarget = math.normalize(directionToTarget);
                aimInput.ValueRW.Value = directionToTarget;
                
                var angleRag = math.atan2(directionToTarget.z, directionToTarget.x);
                var angleDeg = math.degrees(angleRag);
                skillShotUIReference.Value.transform.rotation = Quaternion.Euler(0, -angleDeg, 0);
            }
        }
    }
}