#if UNITY_EDITOR
using Unity.Entities;
using UnityEngine.SceneManagement;

public partial class LoadConnectionSceneSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Enabled = false;
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0)) return;
        SceneManager.LoadScene(0);
    }
}
#endif