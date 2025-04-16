using Unity.Entities;
using UnityEngine;

public class MobaPrefabsAuthoring : MonoBehaviour
{
    [Header("Entities")]
    public GameObject Champion;
    public GameObject Minion;
    public GameObject GameOverEntity;

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
                Champion = GetEntity(authoring.Champion, TransformUsageFlags.Dynamic),
                Minion = GetEntity(authoring.Minion, TransformUsageFlags.Dynamic),
                GameOverEntity = GetEntity(authoring.GameOverEntity, TransformUsageFlags.None)
            });
            
            AddComponentObject(prefabContainerEntity, new UIPrefabs
            {
                HealthBar = authoring.HealthBarPrefab,
                SkillShot = authoring.SkillShotAimPrefab
            });
        }
    }
}
