using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(SpawnPointAuthoring))]
[assembly: RegisterGenericComponentType(typeof(SpawnPointComponent))]
public class SpawnPointAuthoring : MonoBehaviour
{
    public uint seed;
}

public class SpawnPointBaker : Baker<SpawnPointAuthoring>
{
    public override void Bake(SpawnPointAuthoring authoring)
    {
        AddComponent<SpawnPointComponent>(new SpawnPointComponent { random = new Unity.Mathematics.Random(authoring.seed) });
    }
}
