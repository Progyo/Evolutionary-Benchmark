using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct DecisionSpeedComponent : IComponentData
{
    /// <summary>
    /// The time left until a new decision is made
    /// </summary>
    public float value;
}
