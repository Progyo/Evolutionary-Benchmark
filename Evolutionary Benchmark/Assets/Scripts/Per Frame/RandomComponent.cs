using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct RandomComponent : IComponentData
{
    /// <summary>
    /// Used to generate random values
    /// </summary>
    public Random value;
}
