using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public struct SizeMetric : IMetric
{
    public int totalCount;
    public float totalSize;
    public float topSize;
    public float worstSize;

    public string ToJsonString()
    {
        return "'size' : {'average': " + totalSize/(float)totalCount + ", 'top': " + topSize + ", 'worst': " + worstSize + "}";
    }
}
