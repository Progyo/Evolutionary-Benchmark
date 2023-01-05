using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Collections;
using Unity.Burst;

[UpdateAfter(typeof(MutationSystem))]
public partial class ApplySystem : SystemBase
{
    BufferLookup<TraitBufferComponent<int>> _intBufferLookup;
    BufferLookup<TraitBufferComponent<float>> _floatBufferLookup;

    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<SimStateComponent>();


        _intBufferLookup = GetBufferLookup<TraitBufferComponent<int>>(true);
        _floatBufferLookup = GetBufferLookup<TraitBufferComponent<float>>(true);
    }

    protected override void OnUpdate()
    {
        RefRW<SimStateComponent> simState = GetSingletonRW<SimStateComponent>();


        if (simState.ValueRO.phase == Phase.start) 
        {

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent);

            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);

            JobHandle handle = new ApplyJob
            {
                ecb = ecbParallel,
                intLookup = _intBufferLookup,
                floatLookup = _floatBufferLookup,
            }.ScheduleParallel(Dependency);

            handle.Complete();

            ecb.Playback(EntityManager);
            ecb.Dispose();

            simState = GetSingletonRW<SimStateComponent>();
            simState.ValueRW.phase = Phase.running;
        }

    }

    [BurstCompile]
    private partial struct ApplyJob: IJobEntity 
    {
        public EntityCommandBuffer.ParallelWriter ecb;

        [ReadOnly]
        public BufferLookup<TraitBufferComponent<int>> intLookup;
        [ReadOnly]
        public BufferLookup<TraitBufferComponent<float>> floatLookup;

        [BurstCompile]
        public void Execute() 
        {

        }
    }
}
