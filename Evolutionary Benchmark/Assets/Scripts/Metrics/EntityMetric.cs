using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EntityMetric : IMetric
{

    public int size;

    public float speed;

    public float health;

    public float energy;

    public float fitness;

    public string ToJsonString()
    {
        return "{\"size\": " + size + ", \"speed\": " + speed + ", \"health\": " + health + ", \"energy\": "+ energy+ ", \"fitness\": " + fitness+"}";
    }
}
