using Unity.Entities;
using UnityEngine;

public class MainCamera : IComponentData
{
    public Camera Value;
}



public struct MainCameraTag : IComponentData
{
}