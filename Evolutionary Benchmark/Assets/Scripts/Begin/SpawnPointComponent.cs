using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public enum SpawnStrategy 
{
    /// <summary>
    /// A central filled in circle where the objects can spawn
    /// </summary>
    disk,

    /// <summary>
    /// A ring which all the entities spawn on 
    /// </summary>
    ring,
}

public struct SpawnPointComponent : IComponentData
{

    /// <summary>
    /// The standard entity that should be spawned
    /// </summary>
    public Entity prefab;

    public Random random;
    
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
