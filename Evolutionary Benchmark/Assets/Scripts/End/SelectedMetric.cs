using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SelectedMetric : IMetric
{
    public int selected;

    public string ToJsonString()
    {
        return "'selected' : " + selected;
    }
}
