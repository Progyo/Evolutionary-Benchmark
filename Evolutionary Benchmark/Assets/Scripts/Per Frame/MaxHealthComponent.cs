using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct MaxHealthComponent : IComponentData
{
    /// <summary>
    /// Contains the max health value for the entity
    /// </summary>
    public float value;
}
