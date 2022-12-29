using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(InputSystem))]
public partial struct BrainSystem : ISystem
{
    EntityQuery _targetQuery;

    BufferLookup<SeeBufferComponent> _seeBufferLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();
        _targetQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<TargetPositionComponent>()
        .Build(ref state);

        _seeBufferLookup = state.GetBufferLookup<SeeBufferComponent>(false);
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
            _seeBufferLookup.Update(ref state);

            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();

            float deltaTime = SystemAPI.Time.DeltaTime;

            //NativeArray<TargetPositionComponent> targets = _targetQuery.ToComponentDataArray<TargetPositionComponent>(Allocator.TempJob);
            JobHandle handle = new BrainJob { bufferLookup = _seeBufferLookup, random=random, deltaTime=deltaTime}.ScheduleParallel(state.Dependency);
            handle.Complete();
        }
    }

    [BurstCompile]
    private partial struct BrainJob : IJobEntity 
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
            aspect.MoveToClosest(deltaTime,bufferLookup, random);
        }
    }

}
