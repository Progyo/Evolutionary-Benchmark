using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct SpeedComponent : IComponentData//IMutatableFloat
{
    /// <summary>
    /// The speed of the entity
    /// </summary>
    public float value;

    /*
    public void Mutate(IMutatableFloat.PerformCalculation a, float val)
    {
        value = a.Invoke(val);
    }

    public void Mutate(object a)
    {
        Mutate(a as IMutatableFloat.PerformCalculation, value);
    }*/
}
