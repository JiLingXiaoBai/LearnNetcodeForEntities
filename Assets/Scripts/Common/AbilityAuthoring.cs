using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class AbilityAuthoring : MonoBehaviour
{
    public GameObject AoeAbility;
    public float AoeAbilityCooldown;
    public NetCodeConfig NetCodeConfig;
    private int SimulationTickRate => NetCodeConfig.ClientServerTickRate.SimulationTickRate;

    public class AbilityBaker : Baker<AbilityAuthoring>
    {
        public override void Bake(AbilityAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AbilityPrefabs
            {
                AoeAbility = GetEntity(authoring.AoeAbility, TransformUsageFlags.Dynamic)
            });
            AddComponent(entity, new AbilityCooldownTicks
            {
                AoeAbility = (uint)(authoring.AoeAbilityCooldown * authoring.SimulationTickRate)
            });
            AddBuffer<AbilityCooldownTargetTicks>(entity);
        }
    }
}