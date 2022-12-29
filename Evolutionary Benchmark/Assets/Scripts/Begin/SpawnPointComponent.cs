using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct SpawnPointComponent : IComponentData
{
    public Random random;
}
