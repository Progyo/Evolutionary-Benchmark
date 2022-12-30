using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// The items types that can be seen by the entity
/// </summary>
public enum ItemType
{
    food, enemy, ally
}


/// <summary>
/// Buffer element which stores items that the entity can see
/// </summary>
public struct SeeBufferComponent : IBufferElementData
{
    /// <summary>
    /// The item type of the item seen
    /// </summary>
    public ItemType itemType;
    /// <summary>
    /// The distance to the item
    /// </summary>
    public float distance;
    /// <summary>
    /// The position of the item
    /// </summary>
    public float3 position;

    /// <summary>
    /// The entity
    /// </summary>
    public Entity entity;
}
