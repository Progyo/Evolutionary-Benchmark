using System.Collections;
using System.Collections.Generic;
using Unity.Entities;

public struct EnergyComponent : IComponentData
{
    /// <summary>
    /// Contains the energy value for the entity
    /// </summary>
    public float value;
}
