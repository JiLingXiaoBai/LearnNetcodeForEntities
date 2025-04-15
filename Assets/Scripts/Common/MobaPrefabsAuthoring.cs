using Unity.Entities;
using UnityEngine;

public class MobaPrefabsAuthoring : MonoBehaviour
{
    [Header("Entities")]
    public GameObject Champion;

    [Header("GameObjects")]
    public GameObject HealthBarPrefab;
    public GameObject SkillShotAimPrefab;

    public class MobaPrefabsBaker : Baker<MobaPrefabsAuthoring>
    {
        public override void Bake(MobaPrefabsAuthoring authoring)
        {
            var prefabContainerEntity = GetEntity(TransformUsageFlags.None);
            AddComponent(prefabContainerEntity, new MobaPrefabs()
            {
                Champion = GetEntity(authoring.Champion, TransformUsageFlags.Dynamic)
            });
            
            AddComponentObject(prefabContainerEntity, new UIPrefabs
            {
                HealthBar = authoring.HealthBarPrefab,
                SkillShot = authoring.SkillShotAimPrefab
            });
        }
    }
}
