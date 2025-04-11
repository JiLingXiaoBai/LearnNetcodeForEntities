using Unity.Entities;

public struct ClientTeamRequest : IComponentData
{
    public TeamType Value;
}