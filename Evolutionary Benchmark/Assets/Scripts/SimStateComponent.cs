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
}
