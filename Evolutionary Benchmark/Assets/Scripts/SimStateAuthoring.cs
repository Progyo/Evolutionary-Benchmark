using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

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


    /// <summary>
    /// The seed for the random generator
    /// </summary>
    public uint seed;

    public GameObject prefab;


    public GameObject fieldPrefab;

    /// <summary>
    /// The number of fields where entities can spawn on (Also the number of spawners)
    /// </summary>
    public int fields;

    /// <summary>
    /// The maximum amount to keep alive for the next generation. Range between 0 and 1
    /// </summary>
    public float survivePercent;
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
            entityPrefab = GetEntity(authoring.prefab),
            fields = authoring.fields,
            survivePercent = authoring.survivePercent,
            killedThisGen = authoring.maxEntities,
            fieldEntityPrefab = GetEntity(authoring.fieldPrefab)
        });

        AddComponent(new RandomComponent { value = new Unity.Mathematics.Random(authoring.seed) });
    }
}
