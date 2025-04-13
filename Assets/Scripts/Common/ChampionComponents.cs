using Unity.Entities;
using Unity.NetCode;

public struct ChampTag : IComponentData
{
}

public struct NewChampTag : IComponentData
{
}

public struct MobaTeam : IComponentData
{
    [GhostField] public TeamType Value;
}