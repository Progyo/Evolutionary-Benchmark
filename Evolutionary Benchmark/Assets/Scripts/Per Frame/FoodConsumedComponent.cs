using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FoodConsumedComponent : IComponentData
{
    /// <summary>
    /// The amound of food consumed by the entity
    /// </summary>
    public int value;
}
