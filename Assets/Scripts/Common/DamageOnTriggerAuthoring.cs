using Unity.Entities;
using UnityEngine;

public class DamageOnTriggerAuthoring : MonoBehaviour
{
    public int DamageOnTrigger;

    public class DamageOnTriggerBaker : Baker<DamageOnTriggerAuthoring>
    {
        public override void Bake(DamageOnTriggerAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new DamageOnTrigger { Value = authoring.DamageOnTrigger });
            AddBuffer<AlreadyDamagedEntity>(entity);
        }
    }
}