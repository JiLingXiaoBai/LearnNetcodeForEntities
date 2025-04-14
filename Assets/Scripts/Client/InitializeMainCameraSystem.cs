using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InitializeMainCameraSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<MainCameraTag>();
    }
    protected override void OnUpdate()
    {
        Enabled = false;
        var mainCameraEntity = SystemAPI.GetSingletonEntity<MainCameraTag>();
        EntityManager.SetComponentData(mainCameraEntity, new MainCamera() { Value = Camera.main });
    }
}