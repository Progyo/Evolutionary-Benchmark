using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;


[BurstCompile]
public struct TestCombinationAlgorithm : ICombinationAlgorithn
{
    [BurstCompile]
    public void SelectAndCreate(RefRW<RandomComponent> random, NativeArray<Entity> entities, NativeArray<FitnessComponent> fitness, int index, EntityCommandBuffer.ParallelWriter ecb)
    {
        int entity1Index = index * 2;
        int entity2Index = index * 2 + 1;

        Entity entity1 = entities[entity1Index];
        Entity entity2 = entities[entity2Index];

        Entity toCreate = entity2;
        int sortKey = entity2Index;

        if (fitness[entity1Index].value > fitness[entity2Index].value) 
        {
            toCreate = entity1;
            sortKey = entity1Index;
        }


        Entity e = ecb.Instantiate(sortKey, toCreate);

        ecb.AddComponent<KeepComponent>(sortKey, e);
    }
}
