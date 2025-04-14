using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class ChampAuthoring : MonoBehaviour
{
    public float MoveSpeed;

    public class ChampBaker : Baker<ChampAuthoring>
    {
        public override void Bake(ChampAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<ChampTag>(entity);
            AddComponent<NewChampTag>(entity);
            AddComponent<MobaTeam>(entity);
            AddComponent<URPMaterialPropertyBaseColor>(entity);
            AddComponent<ChampMoveTargetPosition>(entity);
            AddComponent(entity, new CharacterMoveSpeed { Value = authoring.MoveSpeed });
            AddComponent<AbilityInput>(entity);
        }
    }
}