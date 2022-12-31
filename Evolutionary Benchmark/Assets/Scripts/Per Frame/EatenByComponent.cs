using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct EatenByComponent : IComponentData
{
    /// <summary>
    /// The amount the food item restores energy/health
    /// </summary>
    public float nurishment;

    /// <summary>
    /// Reference to the entity that is eating this item
    /// </summary>
    public RefRW<Entity> eatenBy;

    public RefRW<HealthComponent> health;
    public RefRW<MaxHealthComponent> maxHealth;
    public RefRW<EnergyComponent> energy;
    public RefRW<MaxEnergyComponent> maxEnergy;
}
