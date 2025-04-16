using Unity.Entities;
using UnityEngine;

public class GameOverOnDestroyAuthoring : MonoBehaviour
{
    public class GameOverOnDestroyBaker : Baker<GameOverOnDestroyAuthoring>
    {
        public override void Bake(GameOverOnDestroyAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<GameOverOnDestroyTag>(entity);
        }
    }
}