using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

[BurstCompile]
[UpdateAfter(typeof(ConsumeSystem))]
public partial struct EvaluationSystem : ISystem
{
    EntityQuery _evaluateQuery;

    ComponentTypeHandle<EntityTypeComponent> _entityTypeTypeHandle;
    ComponentTypeHandle<LocalToWorldTransform> _transformTypeHandle;
    ComponentTypeHandle<TargetPositionComponent> _targetTypeHandle;
    ComponentTypeHandle<MaxDecisionSpeedComponent> _maxDecisionSpeedTypeHandle;
    ComponentTypeHandle<DecisionSpeedComponent> _decisionTimeLeftTypeHandle;
    ComponentTypeHandle<EnergyComponent> _energyTypeHandle;
    ComponentTypeHandle<HealthComponent> _healthTypeHandle;
    ComponentTypeHandle<MaxEnergyComponent> _maxEnergyTypeHandle;
    ComponentTypeHandle<MaxHealthComponent> _maxHealthTypeHandle;
    ComponentTypeHandle<FoodConsumedComponent> _foodConsumedTypeHandle;


    EntityTypeHandle _entityTypeHandle;

    BufferLookup<TraitBufferComponent<int>> _intBufferLookup;
    BufferLookup<TraitBufferComponent<float>> _floatBufferLookup;



    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();

        var builder = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<EntityTypeComponent, TargetPositionComponent>();

        _evaluateQuery = state.GetEntityQuery(builder);

        _entityTypeTypeHandle = state.GetComponentTypeHandle<EntityTypeComponent>(true);
        _entityTypeHandle = state.GetEntityTypeHandle();
        _transformTypeHandle = state.GetComponentTypeHandle<LocalToWorldTransform>(true);
        _targetTypeHandle = state.GetComponentTypeHandle<TargetPositionComponent>(true);
        _maxDecisionSpeedTypeHandle = state.GetComponentTypeHandle<MaxDecisionSpeedComponent>(true);
        _decisionTimeLeftTypeHandle = state.GetComponentTypeHandle<DecisionSpeedComponent>(true);
        _energyTypeHandle = state.GetComponentTypeHandle<EnergyComponent>(true);
        _healthTypeHandle = state.GetComponentTypeHandle<HealthComponent>(true);
        _maxEnergyTypeHandle = state.GetComponentTypeHandle<MaxEnergyComponent>(true);
        _maxHealthTypeHandle = state.GetComponentTypeHandle<MaxHealthComponent>(true);
        _foodConsumedTypeHandle = state.GetComponentTypeHandle<FoodConsumedComponent>(true);

        _intBufferLookup = state.GetBufferLookup<TraitBufferComponent<int>>(true);
        _floatBufferLookup = state.GetBufferLookup<TraitBufferComponent<float>>(true);


        builder.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        RefRW<SimStateComponent> simState = SystemAPI.GetSingletonRW<SimStateComponent>();
        if (simState.ValueRO.phase == Phase.end)
        {
            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();


            _entityTypeTypeHandle.Update(ref state);
            _entityTypeHandle.Update(ref state);
            _intBufferLookup.Update(ref state);
            _floatBufferLookup.Update(ref state);

            _transformTypeHandle.Update(ref state);
            _targetTypeHandle.Update(ref state);
            _maxDecisionSpeedTypeHandle.Update(ref state);
            _decisionTimeLeftTypeHandle.Update(ref state);
            _energyTypeHandle.Update(ref state);
            _healthTypeHandle.Update(ref state);
            _maxEnergyTypeHandle.Update(ref state);
            _maxHealthTypeHandle.Update(ref state);
            _foodConsumedTypeHandle.Update(ref state);


            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            JobHandle handle = new EvaluateJob<EvaluationFunction>
            {
                random = random,
                intBufferLookup = _intBufferLookup,
                floatBufferLookup = _floatBufferLookup,
                transformTypeHandle = _transformTypeHandle,
                targetTypeHandle = _targetTypeHandle,
                maxDecisionSpeedTypeHandle = _maxDecisionSpeedTypeHandle,
                decisionTimeLeftTypeHandle = _decisionTimeLeftTypeHandle,
                energyTypeHandle = _energyTypeHandle,
                healthTypeHandle = _healthTypeHandle,
                entityTypeHandle = _entityTypeHandle,
                maxEnergyTypeHandle = _maxEnergyTypeHandle,
                maxHealthTypeHandle = _maxHealthTypeHandle,
                foodConsumedTypeHandle = _foodConsumedTypeHandle,
                entityTypeTypeHandle = _entityTypeTypeHandle,
                ecb = ecbParallel,
                epoch = simState.ValueRO.currentEpoch,
                timeElapsed = simState.ValueRO.timeElapsed,
            }.ScheduleParallel(_evaluateQuery, state.Dependency);
            //simState.ValueRW.phase = Phase.evaluated;

            handle.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }  
}

[BurstCompile]
public unsafe struct EvaluateJob<EvalFunc> : IJobChunk where EvalFunc : struct, IEvaluate
{

    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> random;

    [ReadOnly]
    public ComponentTypeHandle<EntityTypeComponent> entityTypeTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<LocalToWorldTransform> transformTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<TargetPositionComponent> targetTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<MaxDecisionSpeedComponent> maxDecisionSpeedTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<DecisionSpeedComponent> decisionTimeLeftTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<EnergyComponent> energyTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<HealthComponent> healthTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<MaxEnergyComponent> maxEnergyTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<MaxHealthComponent> maxHealthTypeHandle;
    [ReadOnly]
    public ComponentTypeHandle<FoodConsumedComponent> foodConsumedTypeHandle;

    public EntityTypeHandle entityTypeHandle;


    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public BufferLookup<TraitBufferComponent<int>> intBufferLookup;

    [NativeDisableParallelForRestriction]
    [ReadOnly]
    public BufferLookup<TraitBufferComponent<float>> floatBufferLookup;


    public EntityCommandBuffer.ParallelWriter ecb;


    [ReadOnly]
    public int epoch;

    [ReadOnly]
    public float timeElapsed;

    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        Assert.IsFalse(useEnabledMask);


        var chunkTransforms = chunk.GetNativeArray(transformTypeHandle);
        var chunkTargets = chunk.GetNativeArray(targetTypeHandle);
        var chunkmaxDecision = chunk.GetNativeArray(maxDecisionSpeedTypeHandle);
        var chunkDecision = chunk.GetNativeArray(decisionTimeLeftTypeHandle);
        var chunkEnergy = chunk.GetNativeArray(energyTypeHandle);
        var chunkHealth = chunk.GetNativeArray(healthTypeHandle);
        var chunkMaxEnergy = chunk.GetNativeArray(maxEnergyTypeHandle);
        var chunkMaxHealth = chunk.GetNativeArray(maxHealthTypeHandle);
        var chunkFoodConsumed = chunk.GetNativeArray(foodConsumedTypeHandle);

        var chunkEntityType = chunk.GetNativeArray(entityTypeTypeHandle);
        


        NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);

        EvalFunc evalFunc = new EvalFunc
        {
            floatLookup = floatBufferLookup,
            intLookup = intBufferLookup,
        };

        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
        {
            if (IsEntity(chunkEntityType[i].value)) 
            {
                RefRW<LocalToWorldTransform> transform = new RefRW<LocalToWorldTransform>(chunkTransforms, i);
                RefRW<TargetPositionComponent> target = new RefRW<TargetPositionComponent>(chunkTargets, i);
                RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed = new RefRW<MaxDecisionSpeedComponent>(chunkmaxDecision, i);
                RefRW<DecisionSpeedComponent> decisionTimeLeft = new RefRW<DecisionSpeedComponent>(chunkDecision, i);
                RefRW<EnergyComponent> energy = new RefRW<EnergyComponent>(chunkEnergy, i);
                RefRW<HealthComponent> health = new RefRW<HealthComponent>(chunkHealth, i);
                RefRW<MaxEnergyComponent> maxEnergy = new RefRW<MaxEnergyComponent>(chunkMaxEnergy, i);
                RefRW<MaxHealthComponent> maxHealth = new RefRW<MaxHealthComponent>(chunkMaxHealth, i);
                RefRW<FoodConsumedComponent> foodConsumed = new RefRW<FoodConsumedComponent>(chunkFoodConsumed, i);
                RefRW<Entity> entity = new RefRW<Entity>(entities, i);

                evalFunc.transform = transform;
                evalFunc.target = target;
                evalFunc.maxDecisionSpeed = maxDecisionSpeed;
                evalFunc.decisionTimeLeft = decisionTimeLeft;
                evalFunc.energy = energy;
                evalFunc.maxEnergy = maxEnergy;
                evalFunc.health = health;
                evalFunc.maxHealth = maxHealth;
                evalFunc.entity = entity;
                evalFunc.foodEaten = foodConsumed;

                float fitness = evalFunc.Evaluate();

                ecb.SetComponent<FitnessComponent>(i, entity.ValueRO, new FitnessComponent { value = fitness });

                /*
                if(fitness == 0) 
                {
                    ecb.AddComponent<DestroyComponent>(i, entity.ValueRO);
                }*/
                

                Entity e  = ecb.CreateEntity(i + chunkEntityCount);

                ecb.AddComponent<MetricComponent<float>>(i + 2 * chunkEntityCount, e,
                    new MetricComponent<float> {epoch = epoch,
                        timeStamp = timeElapsed,
                        type = MetricType.fitness,
                        value = fitness});
            }
        }

        entities.Dispose();

        chunkTransforms.Dispose();
        chunkTargets.Dispose();
        chunkDecision.Dispose();
        chunkmaxDecision.Dispose();
        chunkEnergy.Dispose();
        chunkHealth.Dispose();
        chunkMaxEnergy.Dispose();
        chunkMaxHealth.Dispose();
        chunkFoodConsumed.Dispose();
        chunkEntityType.Dispose();
    }


    [BurstCompile]
    private bool IsEntity(EntityType type) 
    {
        return type == EntityType.blob;
    }
}

public interface IEvaluate 
{
    public RefRW<Entity> entity { get; set; }
    public RefRW<LocalToWorldTransform> transform { get; set; }

    public RefRW<TargetPositionComponent> target { get; set; }
    public RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed { get; set; }
    public RefRW<DecisionSpeedComponent> decisionTimeLeft { get; set; }
    public RefRW<EnergyComponent> energy { get; set; }
    public RefRW<HealthComponent> health { get; set; }
    public RefRW<MaxEnergyComponent> maxEnergy { get; set; }
    public RefRW<MaxHealthComponent> maxHealth { get; set; }
    public BufferLookup<TraitBufferComponent<int>> intLookup { get; set; }
    public BufferLookup<TraitBufferComponent<float>> floatLookup { get; set; }

    public RefRW<FoodConsumedComponent> foodEaten { get; set; }

    public float Evaluate();
}