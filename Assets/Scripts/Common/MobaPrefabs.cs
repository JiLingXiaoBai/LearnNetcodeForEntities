using Unity.Entities;
using UnityEngine;

public struct MobaPrefabs : IComponentData
{
    public Entity Champion;
    public Entity Minion;
}

public class UIPrefabs : IComponentData
{
    public GameObject HealthBar;
    public GameObject SkillShot;
}