using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SimulationMode 
{
    /// <summary>
    /// Mutate and select the population
    /// </summary>
    evolve,

    /// <summary>
    /// Just run the simulation keeping the population the same over epochs
    /// </summary>
    run
}