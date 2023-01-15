using Unity.Entities;

/// <summary>
/// This component stores global state information
/// </summary>
public struct SimStateComponent : IComponentData
{
    
    /// <summary>
    /// The current phase
    /// </summary>
    public Phase phase;
    /// <summary>
    /// Time elapsed in the current epoch
    /// </summary>
    public float timeElapsed;
    /// <summary>
    /// The duration of an epoch in seconds
    /// </summary>
    public float epochDuration;
    /// <summary>
    /// The current epoch
    /// </summary>
    public int currentEpoch;
    /// <summary>
    /// Maximum epochs to simulate
    /// </summary>
    public int maxEpochs;

    /// <summary>
    /// The maximum entities to simulate
    /// </summary>
    public int maxEntities;

    /// <summary>
    /// The entity prefab
    /// </summary>
    public Entity entityPrefab;


    /// <summary>
    /// The field entity prefab
    /// </summary>
    public Entity fieldEntityPrefab;

    /// <summary>
    /// The number of fields where entities can spawn on (Also the number of spawners)
    /// </summary>
    public int fields;


    /// <summary>
    /// The maximum amount to keep alive for the next generation. Range between 0 and 1
    /// </summary>
    public float survivePercent;

    /// <summary>
    /// The number of (blob) entities killed this generation
    /// </summary>
    public int killedThisGen;

}
