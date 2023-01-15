using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FieldComponent : IComponentData
{
    /// <summary>
    /// This is the field id
    /// </summary>
    public int value;
}
