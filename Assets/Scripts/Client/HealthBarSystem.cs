using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(TransformSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct HealthBarSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        state.RequireForUpdate<UIPrefabs>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, healthBarOffset, maxHitPoints, entity) in SystemAPI
                     .Query<RefRO<LocalTransform>, RefRO<HealthBarOffset>, RefRO<MaxHitPoints>>()
                     .WithNone<HealthBarUIReference>().WithEntityAccess())
        {
            var healthBarPrefab = SystemAPI.ManagedAPI.GetSingleton<UIPrefabs>().HealthBar;
            var spawnPosition = transform.ValueRO.Position + healthBarOffset.ValueRO.Value;
            var newHealthBar = Object.Instantiate(healthBarPrefab, spawnPosition, Quaternion.identity);
            SetHealthBar(newHealthBar, maxHitPoints.ValueRO.Value, maxHitPoints.ValueRO.Value);
            ecb.AddComponent(entity, new HealthBarUIReference { Value = newHealthBar });
        }

        //Update position and values of health bar
        foreach (var (transform, healthBarOffset, currentHitPoints, maxHitPoints, healthBarUI) in
                 SystemAPI
                     .Query<RefRO<LocalTransform>, RefRO<HealthBarOffset>, RefRO<CurrentHitPoints>,
                         RefRO<MaxHitPoints>, HealthBarUIReference>())
        {
            var healthBarPosition = transform.ValueRO.Position + healthBarOffset.ValueRO.Value;
            healthBarUI.Value.transform.position = healthBarPosition;
            SetHealthBar(healthBarUI.Value, currentHitPoints.ValueRO.Value, maxHitPoints.ValueRO.Value);
        }

        //Cleanup health bar once associated entity is destroyed
        foreach (var (healthBarUI, entity) in SystemAPI.Query<HealthBarUIReference>().WithNone<LocalTransform>().WithEntityAccess())
        {
            Object.Destroy(healthBarUI.Value);
            ecb.RemoveComponent<HealthBarUIReference>(entity);
        }
    }

    private void SetHealthBar(GameObject healthBarCanvasObject, int curHitPoints, int maxHitPoints)
    {
        var healthBarSlider = healthBarCanvasObject.GetComponentInChildren<UnityEngine.UI.Slider>();
        healthBarSlider.minValue = 0;
        healthBarSlider.maxValue = maxHitPoints;
        healthBarSlider.value = curHitPoints;
    }
}