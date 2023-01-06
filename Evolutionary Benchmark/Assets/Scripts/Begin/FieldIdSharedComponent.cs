using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FieldIdSharedComponent : ISharedComponentData
{
    /// <summary>
    /// This stores the field ID
    /// </summary>
    public int value;
}
