using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

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
    ComponentTypeHandle<MaxEnergyComponent> _maxEnergyTypeHandle;
    ComponentTypeHandle<MaxHealthComponent> _maxHealthTypeHandle;
    EntityTypeHandle _entityTypeHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();

        var builder = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<TargetPositionComponent>();


        _targetQuery = state.GetEntityQuery(builder);


        builder.Dispose();

        builder = new EntityQueryBuilder(Allocator.Temp)
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
        _maxEnergyTypeHandle = state.GetComponentTypeHandle<MaxEnergyComponent>();
        _maxHealthTypeHandle = state.GetComponentTypeHandle<MaxHealthComponent>();
        _entityTypeHandle = state.GetEntityTypeHandle();


        builder.Dispose();
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
            _maxEnergyTypeHandle.Update(ref state);
            _maxHealthTypeHandle.Update(ref state);
            _entityTypeHandle.Update(ref state);

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);

            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

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
                healthTypeHandle = _healthTypeHandle,
                entityTypeHandle = _entityTypeHandle,
                maxEnergyTypeHandle = _maxEnergyTypeHandle,
                maxHealthTypeHandle = _maxHealthTypeHandle,
                ecb = ecbParallel
            }.ScheduleParallel(_brainQuery, state.Dependency);
            handle.Complete();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            //transforms.Dispose();
        }
    }
    

    
}


//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/81d262961b70ad285783d52a362b7ad75c85b549/ECSSamples/Assets/HelloCube/5.%20IJobChunk/RotationSpeedSystem.cs
//https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/EntitiesSamples/HelloCube/Assets/5.%20IJobChunk/RotationSpeedSystem.cs
//https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/iterating-data-ijobchunk.html

[BurstCompile]
public unsafe struct BrainJob<T> : IJobChunk where T : struct, IBrain
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
    public ComponentTypeHandle<MaxEnergyComponent> maxEnergyTypeHandle;
    public ComponentTypeHandle<MaxHealthComponent> maxHealthTypeHandle;
    public EntityTypeHandle entityTypeHandle;

    public EntityCommandBuffer.ParallelWriter ecb;

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
        var chunkMaxEnergy = chunk.GetNativeArray(maxEnergyTypeHandle);
        var chunkMaxHealth = chunk.GetNativeArray(maxHealthTypeHandle);

        //var chunkEntityPtr = chunk.GetEntityDataPtrRO(entityTypeHandle);
        //chunk.GetNativeArray


        //Workaround for RefRW
        NativeArray<Entity> entities = chunk.GetNativeArray(entityTypeHandle);//new NativeArray<Entity>(chunk.Count, Allocator.Temp);
        /*for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++, chunkEntityPtr++) 
        {
            entities[i] = *chunkEntityPtr;
        }*/

        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
        {

            RefRW<LocalToWorldTransform> transform = new RefRW<LocalToWorldTransform>(chunkTransforms, i);
            RefRW<TargetPositionComponent> target = new RefRW<TargetPositionComponent>(chunkTargets, i);
            RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed = new RefRW<MaxDecisionSpeedComponent>(chunkmaxDecision, i);
            RefRW<DecisionSpeedComponent> decisionTimeLeft = new RefRW<DecisionSpeedComponent>(chunkDecision, i);
            RefRW<EnergyComponent> energy = new RefRW<EnergyComponent>(chunkEnergy, i);
            RefRW<HealthComponent> health = new RefRW<HealthComponent>(chunkHealth, i);
            RefRW<MaxEnergyComponent> maxEnergy = new RefRW<MaxEnergyComponent>(chunkMaxEnergy, i);
            RefRW<MaxHealthComponent> maxHealth = new RefRW<MaxHealthComponent>(chunkMaxHealth, i);
            RefRW<Entity> entity = new RefRW<Entity>(entities,i);
            
            T brain = new T {transform = transform,
            target = target,
            maxDecisionSpeed = maxDecisionSpeed,
            decisionTimeLeft = decisionTimeLeft,
            energy = energy,
            health = health,
            entity = entity,
            };


            decisionTimeLeft.ValueRW.value -= deltaTime;

            if (decisionTimeLeft.ValueRO.value <= 0f)
            {
                target.ValueRW.value = transform.ValueRO.Value.Position;


                BrainAction action = brain.See(deltaTime, bufferLookup, random, out float3 moveTo, out Entity entityToConsume);
                //action = BrainAction.nothing;
                if (action == BrainAction.move)
                {

                    float3 bottomLeft = new float3(target.ValueRO.boundary.x, 0f, target.ValueRO.boundary.y);
                    float3 topRight = new float3(target.ValueRO.boundary.z, 0f, target.ValueRO.boundary.w);

                    //Restrict movement to boundaries
                    target.ValueRW.value = math.clamp(moveTo, bottomLeft, topRight);
                }
                else if (action == BrainAction.eat)
                {
                    var tempEntity = new RefStruct<Entity>(entities, i);
                    var tempEnergy = new RefStruct<EnergyComponent>(chunkEnergy, i);
                    var tempHealth = new RefStruct<HealthComponent>(chunkHealth, i);
                    var tempMaxEnergy = new RefStruct<MaxEnergyComponent>(chunkMaxEnergy, i);
                    var tempMaxHealth = new RefStruct<MaxHealthComponent>(chunkMaxHealth, i);

                    ecb.AddComponent<EatenByComponent>(i, entityToConsume, new EatenByComponent { eatenBy = tempEntity,
                        /*nurishment = 10f,*/ energy =tempEnergy,
                        health= tempHealth, maxEnergy = tempMaxEnergy,
                        maxHealth = tempMaxHealth});
                }
                else if (action == BrainAction.attack) 
                {

                }


                //Reset
                decisionTimeLeft.ValueRW.value = maxDecisionSpeed.ValueRO.value * random.ValueRW.value.NextFloat(0.01f, 1f);

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
    }
}


public enum BrainAction 
{
    /// <summary>
    /// Do nothing
    /// </summary>
    nothing,

    /// <summary>
    /// Move to the position
    /// </summary>
    move,

    /// <summary>
    /// Eat
    /// </summary>
    eat,

    /// <summary>
    /// Attack
    /// </summary>
    attack,
}

/// <summary>
/// Interface for all brain models
/// </summary>
public interface IBrain 
{

    public RefRW<Entity> entity { get; set; }
    public RefRW<LocalToWorldTransform> transform { get; set; }

    public RefRW<TargetPositionComponent> target { get; set; }
    public RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed { get; set; }
    public RefRW<DecisionSpeedComponent> decisionTimeLeft { get; set; }
    public RefRW<EnergyComponent> energy { get; set; }
    public RefRW<HealthComponent> health { get; set; }


    /// <summary>
    /// Process visual input
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <param name="bufferLookup"></param>
    /// <param name="random"></param>
    //[BurstCompile]
    public BrainAction See(float deltaTime, BufferLookup<SeeBufferComponent> bufferLookup, RefRW<RandomComponent> random, out float3 moveTo, out Entity entityToConsume);
}