using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// A test mutation algorithm that mutates float values with a probability of mutationChance/100 and a variance of mutationVariance
/// </summary>
[BurstCompile]
public struct TestFloatMutationAlgorithm : IMutateFloatAlgorithm
{

    private RefRW<Entity> _entity;
    private RefRW<EntityTypeComponent> _entityTypeTag;

    public RefRW<Entity> entity { get => _entity; set => _entity=value; }
    public RefRW<EntityTypeComponent> entityTypeTag { get => _entityTypeTag; set => _entityTypeTag=value; }

    [BurstCompile]
    public bool Mutate(ref TraitBufferComponent<float> trait, RefRW<RandomComponent> random, float mutationChance, float mutationVariance)
    {
        bool mutated = false;

        if(random.ValueRW.value.NextFloat(0,100f) < mutationChance) 
        {
            trait.value = math.min(math.max(trait.value + random.ValueRW.value.NextFloat(-mutationVariance, mutationVariance), trait.minValue), trait.maxValue);
            mutated = true;
        }
        return mutated;
    }

}

/// <summary>
/// A test mutation algorithm that mutates int values with a probability of mutationChance/100 and a variance of mutationVariance
/// </summary>
[BurstCompile]
public struct TestIntMutationAlgorithm : IMutateIntAlgorithm
{

    private RefRW<Entity> _entity;
    private RefRW<EntityTypeComponent> _entityTypeTag;

    public RefRW<Entity> entity { get => _entity; set => _entity = value; }
    public RefRW<EntityTypeComponent> entityTypeTag { get => _entityTypeTag; set => _entityTypeTag = value; }

    [BurstCompile]
    public bool Mutate(ref TraitBufferComponent<int> trait, RefRW<RandomComponent> random, float mutationChance, int mutationVariance)
    {
        bool mutated = false;

        if (random.ValueRW.value.NextFloat(0, 100f) < mutationChance)
        {
            trait.value = math.min(math.max(trait.value + random.ValueRW.value.NextInt(-mutationVariance, mutationVariance), trait.minValue), trait.maxValue);
            mutated = true;
        }
        return mutated;
    }

}
