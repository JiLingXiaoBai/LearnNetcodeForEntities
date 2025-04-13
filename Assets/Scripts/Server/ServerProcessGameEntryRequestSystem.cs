using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ServerProcessGameEntryRequestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MobaPrefabs>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var championPrefab = SystemAPI.GetSingleton<MobaPrefabs>().Champion;
        foreach (var (teamRequest, requestSource, requestEntity) in SystemAPI
                     .Query<RefRO<MobaTeamRequest>, RefRO<ReceiveRpcCommandRequest>>()
                     .WithEntityAccess())
        {
            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.ValueRO.SourceConnection);

            var requestedTeamType = teamRequest.ValueRO.Value;

            if (requestedTeamType == TeamType.AutoAssign)
            {
                requestedTeamType = TeamType.Blue;
            }

            var clientId = SystemAPI.GetComponent<NetworkId>(requestSource.ValueRO.SourceConnection).Value;

            Debug.Log($"Server is assigning client {clientId} to the {requestedTeamType.ToString()} team.");
            float3 spawnPosition;
            switch (requestedTeamType)
            {
                case TeamType.Blue:
                    spawnPosition = new float3(-50f, 1f, -50f);
                    break;
                case TeamType.Red:
                    spawnPosition = new float3(50f, 1f, 50f);
                    break;
                default:
                    continue;
            }

            var newChamp = ecb.Instantiate(championPrefab);
            ecb.SetName(newChamp, "Champion");

            var newTransform = LocalTransform.FromPosition(spawnPosition);
            ecb.SetComponent(newChamp, newTransform);
            ecb.SetComponent(newChamp, new GhostOwner { NetworkId = clientId });
            ecb.SetComponent(newChamp, new MobaTeam { Value = requestedTeamType });
            ecb.AppendToBuffer(requestSource.ValueRO.SourceConnection, new LinkedEntityGroup() { Value = newChamp });
        }
        ecb.Playback(state.EntityManager);
    }
}