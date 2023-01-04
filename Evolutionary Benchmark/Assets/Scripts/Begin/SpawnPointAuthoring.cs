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
    /// <summary>
    /// The standard entity that should be spawned
    /// </summary>
    public GameObject prefab;

    /// <summary>
    /// The type of entity this spawner spawns
    /// </summary>
    public EntityType type;

    /// <summary>
    /// The boundary of the spawn area (area that the entity is allowed to move in)
    /// </summary>
    public float4 boundary;

    /// <summary>
    /// The spawn radius
    /// </summary>
    public float radius;

    /// <summary>
    /// How the entites should be spawned
    /// </summary>
    public SpawnStrategy strategy;
}

public class SpawnPointBaker : Baker<SpawnPointAuthoring>
{
    public override void Bake(SpawnPointAuthoring authoring)
    {
        AddComponent<SpawnPointComponent>(new SpawnPointComponent { random = new Unity.Mathematics.Random(authoring.seed), 
        prefab = GetEntity(authoring.prefab),
        boundary = authoring.boundary,
        radius = authoring.radius,
        strategy = authoring.strategy,
        type= authoring.type});
    }
}
