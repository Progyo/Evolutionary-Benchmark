using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using static BrainSystem;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

//[assembly: RegisterGenericJobType(typeof(BrainJob<BrainAspect>))]

[assembly: RegisterGenericJobType(typeof(BrainJob<BrainTest>))]


[BurstCompile]
[UpdateAfter(typeof(InputSystem))]
public partial struct BrainSystem : ISystem
{
    EntityQuery _targetQuery;
    EntityQuery _brainQuery;

    BufferLookup<SeeBufferComponent> _seeBufferLookup;


    //Brain stuff

    ComponentTypeHandle<LocalToWorldTransform> _transformTypeHandle;
    ComponentTypeHandle<TargetPositionComponent> _targetTypeHandle;
    ComponentTypeHandle<MaxDecisionSpeedComponent> _maxDecisionSpeedTypeHandle;
    ComponentTypeHandle<DecisionSpeedComponent> _decisionTimeLeftTypeHandle;
    ComponentTypeHandle<EnergyComponent> _energyTypeHandle;
    ComponentTypeHandle<HealthComponent> _healthTypeHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();

        _targetQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<TargetPositionComponent>()
        .Build(ref state);

        var builder = new EntityQueryBuilder(Allocator.Temp)
         .WithAllRW<LocalToWorldTransform, TargetPositionComponent>()
         .AddAdditionalQuery().
         WithAllRW<MaxDecisionSpeedComponent, DecisionSpeedComponent>()
         .AddAdditionalQuery()
         .WithAllRW<EnergyComponent, HealthComponent>();

        _brainQuery = state.GetEntityQuery(builder  )
        ;


        _seeBufferLookup = state.GetBufferLookup<SeeBufferComponent>(false);

        _transformTypeHandle = state.GetComponentTypeHandle<LocalToWorldTransform>();
        _targetTypeHandle = state.GetComponentTypeHandle<TargetPositionComponent>();
        _maxDecisionSpeedTypeHandle = state.GetComponentTypeHandle<MaxDecisionSpeedComponent>();
        _decisionTimeLeftTypeHandle = state.GetComponentTypeHandle<DecisionSpeedComponent>();
        _energyTypeHandle = state.GetComponentTypeHandle<EnergyComponent>();
        _healthTypeHandle = state.GetComponentTypeHandle<HealthComponent>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.running)
        {
            

            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();

            float deltaTime = SystemAPI.Time.DeltaTime;

            _seeBufferLookup.Update(ref state);

            _transformTypeHandle.Update(ref state);
            _targetTypeHandle.Update(ref state);
            _maxDecisionSpeedTypeHandle.Update(ref state);
            _decisionTimeLeftTypeHandle.Update(ref state);
            _energyTypeHandle.Update(ref state);
            _healthTypeHandle.Update(ref state);

            //NativeArray<LocalToWorldTransform> transforms = _brainQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

            //NativeArray<TargetPositionComponent> targets = _targetQuery.ToComponentDataArray<TargetPositionComponent>(Allocator.TempJob);
            JobHandle handle = new BrainJob<BrainTest> { bufferLookup = _seeBufferLookup,
                random = random,
                deltaTime = deltaTime,
                transformTypeHandle = _transformTypeHandle,
                targetTypeHandle = _targetTypeHandle,
                maxDecisionSpeedTypeHandle = _maxDecisionSpeedTypeHandle,
                decisionTimeLeftTypeHandle = _decisionTimeLeftTypeHandle,
                energyTypeHandle = _energyTypeHandle,
                healthTypeHandle = _healthTypeHandle
            }.ScheduleParallel(_brainQuery, state.Dependency);
            handle.Complete();

            //transforms.Dispose();
        }
    }
    

    
}

/*
[UpdateAfter(typeof(InputSystem))]
public partial class BrainSystem : SystemBase
{
    protected override void OnUpdate()
    {
        RequireForUpdate<SimStateComponent>();

        EntityQuery _targetQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<TargetPositionComponent>()
        .Build(this);

        ;

        EntityQuery _brainQuery = EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform),
            typeof(TargetPositionComponent), typeof(MaxDecisionSpeedComponent), typeof(DecisionSpeedComponent),
            typeof(EnergyComponent), typeof(HealthComponent));

        BufferLookup<SeeBufferComponent> _seeBufferLookup = GetBufferLookup<SeeBufferComponent>(false);


        ComponentTypeHandle<LocalToWorldTransform> _transformTypeHandle;
        ComponentTypeHandle<TargetPositionComponent> _targetTypeHandle;
        ComponentTypeHandle<MaxDecisionSpeedComponent> _maxDecisionSpeedTypeHandle;
        ComponentTypeHandle<DecisionSpeedComponent> _decisionTimeLeftTypeHandle;
        ComponentTypeHandle<EnergyComponent> _energyTypeHandle;
        ComponentTypeHandle<HealthComponent> _healthTypeHandle;

        _transformTypeHandle = GetComponentTypeHandle<LocalToWorldTransform>();
        _targetTypeHandle = GetComponentTypeHandle<TargetPositionComponent>();
        _maxDecisionSpeedTypeHandle = GetComponentTypeHandle<MaxDecisionSpeedComponent>();
        _decisionTimeLeftTypeHandle = GetComponentTypeHandle<DecisionSpeedComponent>();
        _energyTypeHandle = GetComponentTypeHandle<EnergyComponent>();
        _healthTypeHandle = GetComponentTypeHandle<HealthComponent>();


        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.running)
        {


            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();

            float deltaTime = SystemAPI.Time.DeltaTime;

            _seeBufferLookup.Update(this);

            _transformTypeHandle.Update(this);
            _targetTypeHandle.Update(this);
            _maxDecisionSpeedTypeHandle.Update(this);
            _decisionTimeLeftTypeHandle.Update(this);
            _energyTypeHandle.Update(this);
            _healthTypeHandle.Update(this);

            NativeArray<LocalToWorldTransform> transforms = _brainQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

            //NativeArray<TargetPositionComponent> targets = _targetQuery.ToComponentDataArray<TargetPositionComponent>(Allocator.TempJob);
            JobHandle handle = new BrainJob<BrainTest>
            {
                bufferLookup = _seeBufferLookup,
                random = random,
                deltaTime = deltaTime,
                transformTypeHandle = _transformTypeHandle,
                targetTypeHandle = _targetTypeHandle,
                maxDecisionSpeedTypeHandle = _maxDecisionSpeedTypeHandle,
                decisionTimeLeftTypeHandle = _decisionTimeLeftTypeHandle,
                energyTypeHandle = _energyTypeHandle,
                healthTypeHandle = _healthTypeHandle
            }.ScheduleParallel(_brainQuery, Dependency);
            handle.Complete();

            transforms.Dispose();
        }
    }
}
*/

//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/81d262961b70ad285783d52a362b7ad75c85b549/ECSSamples/Assets/HelloCube/5.%20IJobChunk/RotationSpeedSystem.cs
//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/EntitiesSamples/HelloCube/Assets/5.%20IJobChunk/RotationSpeedSystem.cs
//https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/iterating-data-ijobchunk.html
/*[BurstCompile]
public partial struct BrainJob: IJobEntity//<T> : IJobEntity where T :  IAspect, IBrain
{
    [NativeDisableParallelForRestriction]
    public BufferLookup<SeeBufferComponent> bufferLookup;

    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> random;

    [ReadOnly]
    public float deltaTime;

    [BurstCompile]
    public void Execute(ref BrainAspect aspect)
    {
        aspect.MoveToClosest(deltaTime, bufferLookup, random);
    }
}*/


[BurstCompile]
public struct BrainJob<T> : IJobChunk where T : struct, IBrain
{
    [NativeDisableParallelForRestriction]
    public BufferLookup<SeeBufferComponent> bufferLookup;

    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> random;

    [ReadOnly]
    public float deltaTime;


    public ComponentTypeHandle< LocalToWorldTransform> transformTypeHandle;
    public ComponentTypeHandle<TargetPositionComponent> targetTypeHandle;
    public ComponentTypeHandle<MaxDecisionSpeedComponent> maxDecisionSpeedTypeHandle;
    public ComponentTypeHandle<DecisionSpeedComponent> decisionTimeLeftTypeHandle;
    public ComponentTypeHandle<EnergyComponent> energyTypeHandle;
    public ComponentTypeHandle<HealthComponent> healthTypeHandle;

    [BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        Assert.IsFalse(useEnabledMask);
        

        var chunkTransforms = chunk.GetNativeArray(transformTypeHandle);
        var chunkTargets = chunk.GetNativeArray(targetTypeHandle);
        var chunkmaxDecision = chunk.GetNativeArray(maxDecisionSpeedTypeHandle);
        var chunkDecision = chunk.GetNativeArray(decisionTimeLeftTypeHandle);
        var chunkEnergy = chunk.GetNativeArray(energyTypeHandle);
        var chunkHealth = chunk.GetNativeArray(healthTypeHandle);


        //AtomicSafetyHandle handle = AtomicSafetyHandle.Create();
        //TargetPositionComponent* target = (TargetPositionComponent*)chunk.GetRequiredComponentDataPtrRW<TargetPositionComponent>(ref targetTypeHandle);
        
        
        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)//, target++
        {
            RefRW<TargetPositionComponent> target = new RefRW<TargetPositionComponent>(chunkTargets, i);//new RefRW<TargetPositionComponent>((byte*) target , handle);

            //target.ValueRW.value = new Unity.Mathematics.float3(i, 0f, 0f);

            TargetTest test = new TargetTest {};

            test.SetTarget(target);

            test.Move();

            /*
            T brain = new T {transform = chunkTransforms[i],
            target = comp,
            maxDecisionSpeed = chunkmaxDecision[i],
            decisionTimeLeft = chunkDecision[i],
            energy = chunkEnergy[i],
            health = chunkHealth[i]
            };

            brain.See(deltaTime, bufferLookup, random);

            //This is slow please figure out how to fix this!!!
            chunkTransforms[i] = new LocalToWorldTransform { Value = brain.transform.Value};
            //chunkTargets[i] = new TargetPositionComponent { value = new Unity.Mathematics.float3(i,0,i)}; //brain.target.value
            chunkTargets[i] = brain.target.ValueRO;
            chunkmaxDecision[i] = new MaxDecisionSpeedComponent { value = brain.maxDecisionSpeed.value};
            chunkDecision[i] = new DecisionSpeedComponent { value = brain.decisionTimeLeft.value };
            chunkEnergy[i] = new EnergyComponent { value = brain.energy.value};
            chunkHealth[i] = new HealthComponent { value = brain.health.value };*/

        }
        //AtomicSafetyHandle.Release(handle);
    }
}

/// <summary>
/// Interface for all brain models
/// </summary>
public interface IBrain 
{
    public LocalToWorldTransform transform { get; set; }

    public RefRW<TargetPositionComponent> target { get; set; }
    public MaxDecisionSpeedComponent maxDecisionSpeed { get; set; }
    public DecisionSpeedComponent decisionTimeLeft { get; set; }
    public EnergyComponent energy { get; set; }
    public HealthComponent health { get; set; }


    /// <summary>
    /// Process visual input
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="bufferLookup"></param>
    /// <param name="random"></param>
    [BurstCompile]
    public void See(float deltaTime, BufferLookup<SeeBufferComponent> bufferLookup, RefRW<RandomComponent> random);
}