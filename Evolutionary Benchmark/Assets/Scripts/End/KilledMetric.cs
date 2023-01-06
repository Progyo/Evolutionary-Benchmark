using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct KilledMetric : IMetric
{
    public int killedThisGen;

    public string ToJsonString()
    {
        return "\"killed\" : "+ killedThisGen;
    }
}
