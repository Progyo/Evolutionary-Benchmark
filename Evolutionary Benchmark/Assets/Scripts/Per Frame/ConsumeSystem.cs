using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;


[BurstCompile]
[UpdateAfter(typeof(ControlSystem))]
public partial struct ConsumeSystem : ISystem
{

    EntityQuery _eatQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<EatenByComponent>();

        _eatQuery = state.GetEntityQuery(builder);
        state.RequireForUpdate<SimStateComponent>();

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
            EntityCommandBuffer.ParallelWriter ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();


            JobHandle handle = new EatJob { ecb = ecb }.ScheduleParallel(state.Dependency);
            handle.Complete();

        }
    }
}

[BurstCompile]
public partial struct EatJob : IJobEntity 
{

    public EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void Execute(ref EatAspect aspect, [EntityInQueryIndex] int sortKey) 
    {
        aspect.Consume(ecb, sortKey);
    }
}