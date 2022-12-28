using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct HealthComponent : IComponentData
{
    /// <summary>
    /// Contains the health value for the entity
    /// </summary>
    public float value;
}
