using Unity.Entities;

public partial class AbilityInputSystem : SystemBase
{
    private MobaInputActions _inputActions;

    protected override void OnCreate()
    {
        _inputActions = new MobaInputActions();
    }

    protected override void OnStartRunning()
    {
        _inputActions.Enable();
    }

    protected override void OnStopRunning()
    {
        _inputActions.Disable();
    }

    protected override void OnUpdate()
    {
        var newAbilityInput = new AbilityInput();
        if (_inputActions.GameplayMap.AoeAblility.WasPressedThisFrame())
        {
            newAbilityInput.AoeAbility.Set();
        }

        foreach (var abilityInput in SystemAPI.Query<RefRW<AbilityInput>>())
        {
            abilityInput.ValueRW = newAbilityInput;
        }
    }
}