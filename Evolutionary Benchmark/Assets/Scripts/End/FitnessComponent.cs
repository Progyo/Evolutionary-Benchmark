using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FitnessComponent : IComponentData
{
    /// <summary>
    /// The fitness of the entity
    /// </summary>
    public float value;
}
