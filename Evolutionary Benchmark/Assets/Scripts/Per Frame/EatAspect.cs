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
        
        /*
        if (eaten.ValueRW.maxEnergy.ValueRW.value < eaten.ValueRW.energy.ValueRW.value + eaten.ValueRW.nurishment)
        {
            energyToAdd = eaten.ValueRW.maxEnergy.ValueRW.value - eaten.ValueRW.energy.ValueRW.value;
            healthToAdd = eaten.ValueRW.nurishment - energyToAdd;
        }
        else if (eaten.ValueRW.energy.ValueRW.value < eaten.ValueRW.maxEnergy.ValueRW.value)
        {
            energyToAdd = eaten.ValueRW.nurishment;
        }
        
        eaten.ValueRW.energy.ValueRW.value += energyToAdd;
        eaten.ValueRW.health.ValueRW.value += healthToAdd;
        */
        ecb.DestroyEntity(sortKey, entity);

    }
}
