using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering;

[BurstCompile]
[UpdateAfter(typeof(EpochTimer))]
public partial struct SpawnerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
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

        if (success && simState.phase == Phase.start)
        {

            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            JobHandle handle = new SpawnJob {ecb = ecb, entityPrefab=simState.entityPrefab, spawnCount=simState.maxEntities}.ScheduleParallel(state.Dependency);
            handle.Complete();
        }
    }

    [BurstCompile]
    private partial struct SpawnJob : IJobEntity 
    {

        public EntityCommandBuffer.ParallelWriter ecb;

        [ReadOnly]
        public Entity entityPrefab;

        [ReadOnly]
        public int spawnCount;

        [BurstCompile]
        public void Execute(SpawnAspect aspect, [EntityInQueryIndex] int sortKey) 
        {
            aspect.SpawnEntity(entityPrefab, ecb, sortKey, spawnCount);
        }
    }
}
