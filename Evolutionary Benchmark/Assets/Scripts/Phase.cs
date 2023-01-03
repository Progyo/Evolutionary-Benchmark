
/// <summary>
/// Enum storing the different states that the simulator can be in
/// </summary>
[System.Serializable]
public enum Phase 
{
    /// <summary>
    /// The beginning of an epoch (Spawn entities etc)
    /// </summary>
    start,
    /// <summary>
    /// State while epoch is running
    /// </summary>
    running,
    /// <summary>
    /// End of epoch 
    /// </summary>
    end,
    /// <summary>
    /// The simulation is over
    /// </summary>
    stop,
    /// <summary>
    /// End of epoch evaluated
    /// </summary>
    evaluated,

    /// <summary>
    /// Objects being deleted at the end of epoch 
    /// </summary>
    deleting,
}