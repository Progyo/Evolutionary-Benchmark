using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct MaxEnergyComponent : IComponentData
{
    /// <summary>
    /// Contains the maximum energy value for the entity
    /// </summary>
    public float value;
}
