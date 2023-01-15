using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct KeepComponent : IComponentData
{
    /// <summary>
    /// The number of children that this entity should have
    /// </summary>
    public int offspring;
}
