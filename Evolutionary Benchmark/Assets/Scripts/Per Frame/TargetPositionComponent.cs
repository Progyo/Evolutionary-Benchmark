using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;

public struct TargetPositionComponent : IComponentData
{
    /// <summary>
    /// The position the entity should move to
    /// </summary>
    public float3 value;

    /// <summary>
    /// The boundary which the entity is allowed to move in
    /// </summary>
    public float4 boundary;
}
