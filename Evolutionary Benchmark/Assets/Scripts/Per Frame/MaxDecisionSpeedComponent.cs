using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct MaxDecisionSpeedComponent : IComponentData
{
    /// <summary>
    /// The maximum decision speed for the entity.
    /// </summary>
    public float value;
}
