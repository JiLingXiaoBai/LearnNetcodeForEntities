using System;
using Unity.Entities;

public partial class GameOverSystem : SystemBase
{
    public Action<TeamType> OnGameOver;

    protected override void OnCreate()
    {
        RequireForUpdate<GameOverTag>();
        RequireForUpdate<GameplayingTag>();
    }

    protected override void OnUpdate()
    {
        var gameOverEntity = SystemAPI.GetSingletonEntity<GameOverTag>();
        var winningTeam = SystemAPI.GetComponent<WinningTeam>(gameOverEntity).Value;
        OnGameOver?.Invoke(winningTeam);
        
        var gamePlayingEntity = SystemAPI.GetSingletonEntity<GameplayingTag>();
        EntityManager.DestroyEntity(gamePlayingEntity);
        Enabled = false;
    }
}