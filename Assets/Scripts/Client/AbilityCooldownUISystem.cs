using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct AbilityCooldownUISystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkTime>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var currentTick = SystemAPI.GetSingleton<NetworkTime>().ServerTick;
        var abilityCooldownUIController = AbilityCooldownUIController.Instance;

        foreach (var (cooldownTargetTicks, abilityCooldownTicks) in
                 SystemAPI.Query<DynamicBuffer<AbilityCooldownTargetTicks>, RefRO<AbilityCooldownTicks>>())
        {
            if (!cooldownTargetTicks.GetDataAtTick(currentTick, out var curTargetTicks))
            {
                curTargetTicks.AoeAbility = NetworkTick.Invalid;
                curTargetTicks.SkillShotAbility = NetworkTick.Invalid;
            }

            if (curTargetTicks.AoeAbility == NetworkTick.Invalid ||
                currentTick.IsNewerThan(curTargetTicks.AoeAbility))
            {
                abilityCooldownUIController.UpdateAoeMask(0f);
            }
            else
            {
                var aoeRemainTickCount =
                    curTargetTicks.AoeAbility.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                var fillAmount = (float)aoeRemainTickCount / abilityCooldownTicks.ValueRO.AoeAbility;
                abilityCooldownUIController.UpdateAoeMask(fillAmount);
            }


            if (curTargetTicks.SkillShotAbility == NetworkTick.Invalid ||
                currentTick.IsNewerThan(curTargetTicks.SkillShotAbility))
            {
                abilityCooldownUIController.UpdateSkillShotMask(0f);
            }
            else
            {
                var skillShotRemainTickCount =
                    curTargetTicks.SkillShotAbility.TickIndexForValidTick - currentTick.TickIndexForValidTick;
                var fillAmount = (float)skillShotRemainTickCount / abilityCooldownTicks.ValueRO.SkillShotAbility;
                abilityCooldownUIController.UpdateSkillShotMask(fillAmount);
            }
        }
    }
}