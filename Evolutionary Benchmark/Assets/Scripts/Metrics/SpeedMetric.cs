using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SpeedMetric : IMetric
{
    public int totalCount;
    public float totalSpeed;
    public float topSpeed;
    public float worstSpeed;

    public string ToJsonString()
    {
        return "'speed' : {'average': " + totalSpeed / (float)totalCount + ", 'top': " + topSpeed+ ", 'worst': " + worstSpeed+ "}";
    }
}
