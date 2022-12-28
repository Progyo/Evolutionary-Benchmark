using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct SizeComponent : IComponentData//IMutatableInt
{
    /// <summary>
    /// The size of the entity
    /// </summary>
    public int value;

    /*
    public void Mutate(IMutatableInt.PerformCalculation a, int val)
    {
        value = a.Invoke(val);
    }

    public void Mutate(object a)
    {
        Mutate(a as IMutatableInt.PerformCalculation, value);
    }*/
}
