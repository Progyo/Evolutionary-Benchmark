using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct EyeRadiusComponent : IComponentData
{
    /// <summary>
    /// The maximum distance that the entity can see
    /// </summary>
    public float value;
}
