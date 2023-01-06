using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// All metrics which should be logged must implement this interface
/// </summary>
/*public interface IMetric
{
    public FunctionPointer<Process2StringDelegate> ToJsonString { get; set; }
    //public char[] ToJsonString { get; }
}

[System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.Cdecl)]
public delegate int Process2StringDelegate();
*/

[assembly: RegisterGenericComponentTypeAttribute(typeof(MetricComponent<int>))]
[assembly: RegisterGenericComponentTypeAttribute(typeof(MetricComponent<float>))]

public enum MetricType 
{

    mutation,
    
    /// <summary>
    /// Fitness metric for a single entity
    /// </summary>
    fitness,

    /// <summary>
    /// The average and max fitness of an epoch
    /// </summary>
    averageMaxFitness,
}

/// <summary>
/// Struct that defines how fitness metric is serialized to json
/// </summary>
public struct AverageAndMaxFitnessMetric : IMetric
{
    public float averageFitness;
    public float maxFitness;
    public float metricCount;

    public string ToJsonString()
    {
        return "\"fitness\" : {\"average\": "+averageFitness+", \"max\": "+maxFitness+"}";
    }
}


public interface IMetric
{
    public string ToJsonString();
}

public struct MetricComponent<T> : IComponentData where T: struct 
{
    /// <summary>
    /// The value which is to be logged
    /// </summary>
    public T value;

    /// <summary>
    /// The origin of the metric
    /// </summary>
    public MetricType type;

    /// <summary>
    /// The time at which the metric was taken
    /// </summary>
    public float timeStamp;

    /// <summary>
    /// The epoch at which the metric was taken
    /// </summary>
    public float epoch;
}


/*
public struct Metric
{
    //FunctionPointer<Process2StringDelegate> _ToJsonString;
    //public FunctionPointer<Process2StringDelegate> ToJsonString { get { return _ToJsonString; } set { _ToJsonString = value; } }
    //public char[] ToJsonString => ("{\n'value'" + value + "\n}").ToCharArray();
    public FunctionPointer<Process2StringDelegate> ToJsonString;
}*/