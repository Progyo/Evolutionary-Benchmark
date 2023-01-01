using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public readonly partial struct EatAspect : IAspect
{
    public readonly Entity entity;
    private readonly RefRW<EatenByComponent> eaten;


    [BurstCompile]
    public void Consume(EntityCommandBuffer.ParallelWriter ecb, int sortKey) 
    {
        float energyToAdd = 0f;
        float healthToAdd = 0f;
        //Prioritize energy regen

        float maxEnergy = 10f; //eaten.ValueRW.maxEnergy.ValueRW.value
        float energy = 5f;//eaten.ValueRW.energy.ValueRO.value;
        float nurishment = 0f;//eaten.ValueRW.nurishment;
        float health = eaten.ValueRO.health.ValueRO.value;


        if ( maxEnergy < energy + nurishment)
        {
            energyToAdd = maxEnergy - energy;
            healthToAdd =  nurishment - energyToAdd;
        }
        else if (energy < maxEnergy)
        {
            energyToAdd = nurishment;
        }
        
        eaten.ValueRW.energy.ValueRW.value = energy + energyToAdd;
        eaten.ValueRW.health.ValueRW.value = health + healthToAdd;
        
        ecb.DestroyEntity(sortKey, entity);

    }
}
