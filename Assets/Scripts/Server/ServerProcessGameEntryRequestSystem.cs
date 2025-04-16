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
        state.RequireForUpdate<NetworkTime>();
        state.RequireForUpdate<GameStartProperties>();
        state.RequireForUpdate<MobaPrefabs>();
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<MobaTeamRequest, ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var championPrefab = SystemAPI.GetSingleton<MobaPrefabs>().Champion;

        var gamePropertyEntity = SystemAPI.GetSingletonEntity<GameStartProperties>();
        var gameStartProperties = SystemAPI.GetComponent<GameStartProperties>(gamePropertyEntity);
        var teamPlayerCounter = SystemAPI.GetComponent<TeamPlayerCounter>(gamePropertyEntity);
        var spawnOffsets = SystemAPI.GetBuffer<SpawnOffset>(gamePropertyEntity);

        foreach (var (teamRequest, requestSource, requestEntity) in SystemAPI
                     .Query<RefRO<MobaTeamRequest>, RefRO<ReceiveRpcCommandRequest>>()
                     .WithEntityAccess())
        {
            ecb.DestroyEntity(requestEntity);
            ecb.AddComponent<NetworkStreamInGame>(requestSource.ValueRO.SourceConnection);

            var requestedTeamType = teamRequest.ValueRO.Value;

            if (requestedTeamType == TeamType.AutoAssign)
            {
                if (teamPlayerCounter.BlueTeamPlayers > teamPlayerCounter.RedTeamPlayers)
                {
                    requestedTeamType = TeamType.Red;
                }
                else if (teamPlayerCounter.BlueTeamPlayers <= teamPlayerCounter.RedTeamPlayers)
                {
                    requestedTeamType = TeamType.Blue;
                }
            }

            var clientId = SystemAPI.GetComponent<NetworkId>(requestSource.ValueRO.SourceConnection).Value;

            float3 spawnPosition;
            switch (requestedTeamType)
            {
                case TeamType.Blue:
                    if (teamPlayerCounter.BlueTeamPlayers >= gameStartProperties.MaxPlayersPerTeam)
                    {
                        Debug.Log($"Blue Team is full. Client ID: {clientId} is spectating the game.");
                        continue;
                    }
                    spawnPosition = new float3(-50f, 1f, -50f);
                    spawnPosition += spawnOffsets[teamPlayerCounter.BlueTeamPlayers].Value;
                    teamPlayerCounter.BlueTeamPlayers++;
                    break;
                case TeamType.Red:
                    if (teamPlayerCounter.RedTeamPlayers >= gameStartProperties.MaxPlayersPerTeam)
                    {
                        Debug.Log($"Red Team is full. Client ID: {clientId} is spectating the game.");
                        continue;
                    }
                    spawnPosition = new float3(50f, 1f, 50f);
                    spawnPosition += spawnOffsets[teamPlayerCounter.RedTeamPlayers].Value;
                    teamPlayerCounter.RedTeamPlayers++;
                    break;
                default:
                    continue;
            }

            Debug.Log($"Server is assigning client {clientId} to the {requestedTeamType.ToString()} team.");
            var newChamp = ecb.Instantiate(championPrefab);
            ecb.SetName(newChamp, "Champion");

            var newTransform = LocalTransform.FromPosition(spawnPosition);
            ecb.SetComponent(newChamp, newTransform);
            ecb.SetComponent(newChamp, new GhostOwner { NetworkId = clientId });
            ecb.SetComponent(newChamp, new MobaTeam { Value = requestedTeamType });
            ecb.AppendToBuffer(requestSource.ValueRO.SourceConnection, new LinkedEntityGroup() { Value = newChamp });

            ecb.SetComponent(newChamp, new NetworkEntityReference { Value = requestSource.ValueRO.SourceConnection });
            ecb.AddComponent(requestSource.ValueRO.SourceConnection, new PlayerSpawnInfo()
            {
                MobaTeam = requestedTeamType,
                SpawnPosition = spawnPosition
            });

            ecb.SetComponent(requestSource.ValueRO.SourceConnection, new CommandTarget { targetEntity = newChamp });

            var playersRemainingToStart = gameStartProperties.MinPlayersToStartGame - teamPlayerCounter.TotalPlayers;
            var gameStartRpc = ecb.CreateEntity();
            if (playersRemainingToStart <= 0 && !SystemAPI.HasSingleton<GameplayingTag>())
            {
                var simulationTickRate = NetCodeConfig.Global.ClientServerTickRate.SimulationTickRate;
                var ticksUntilStart = (uint)(gameStartProperties.CountdownTime * simulationTickRate);
                var gameStartTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
                gameStartTick.Add(ticksUntilStart);
                ecb.AddComponent(gameStartRpc, new GameStartTickRpc
                {
                    Value = gameStartTick
                });

                var gameStartEntity = ecb.CreateEntity();
                ecb.AddComponent(gameStartEntity, new GameStartTick
                {
                    Value = gameStartTick
                });
            }
            else
            {
                ecb.AddComponent(gameStartRpc, new PlayersRemainingToStart { Value = playersRemainingToStart });
            }
            ecb.AddComponent<SendRpcCommandRequest>(gameStartRpc);
        }
        ecb.Playback(state.EntityManager);
        SystemAPI.SetSingleton(teamPlayerCounter);
    }
}