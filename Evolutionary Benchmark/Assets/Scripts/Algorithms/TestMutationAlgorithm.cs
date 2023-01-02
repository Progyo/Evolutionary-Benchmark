using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct TestFloatMutationAlgorithm : IMutateFloatAlgorithm
{

    private RefRW<Entity> _entity;
    private RefRW<EntityTypeComponent> _entityTypeTag;

    public RefRW<Entity> entity { get => _entity; set => _entity=value; }
    public RefRW<EntityTypeComponent> entityTypeTag { get => _entityTypeTag; set => _entityTypeTag=value; }

    [BurstCompile]
    public void Mutate(ref TraitBufferComponent<float> trait, RefRW<RandomComponent> random)
    {
        trait.value = math.min(math.max(trait.value + random.ValueRW.value.NextFloat(-5f, 5f),trait.minValue),trait.maxValue);
    }

}


public struct TestIntMutationAlgorithm : IMutateIntAlgorithm
{

    private RefRW<Entity> _entity;
    private RefRW<EntityTypeComponent> _entityTypeTag;

    public RefRW<Entity> entity { get => _entity; set => _entity = value; }
    public RefRW<EntityTypeComponent> entityTypeTag { get => _entityTypeTag; set => _entityTypeTag = value; }

    [BurstCompile]
    public void Mutate(ref TraitBufferComponent<int> trait, RefRW<RandomComponent> random)
    {
        trait.value = math.min(math.max(trait.value + random.ValueRW.value.NextInt(-5, 5),trait.minValue),trait.maxValue);
    }

}
