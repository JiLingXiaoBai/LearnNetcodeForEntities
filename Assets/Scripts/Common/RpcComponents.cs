using Unity.NetCode;

public struct MobaTeamRequest : IRpcCommand
{
    public TeamType Value;
}