using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


[assembly: RegisterGenericComponentType(typeof(SimStateComponent))]
[assembly: RegisterGenericComponentType(typeof(SimStateAuthoring))]
public class SimStateAuthoring : MonoBehaviour
{
    /// <summary>
    /// The current phase
    /// </summary>
    public Phase phase = Phase.start;
    /// <summary>
    /// Time elapsed in the current epoch
    /// </summary>
    public float timeElapsed = 0f;
    /// <summary>
    /// The duration of an epoch in seconds
    /// </summary>
    public float epochDuration;
    /// <summary>
    /// The current epoch
    /// </summary>
    public int currentEpoch = 0;
    /// <summary>
    /// Maximum epochs to simulate
    /// </summary>
    public int maxEpochs;

    /// <summary>
    /// The maximum entities to simulate
    /// </summary>
    public int maxEntities;
}


public class SimStateBaker : Baker<SimStateAuthoring>
{
    public override void Bake(SimStateAuthoring authoring)
    {
        AddComponent(new SimStateComponent{ phase=authoring.phase,
            timeElapsed = authoring.timeElapsed,
            epochDuration=authoring.epochDuration,
            currentEpoch = authoring.currentEpoch,
            maxEpochs = authoring.maxEpochs,
            maxEntities = authoring.maxEntities,
        });
    }
}
