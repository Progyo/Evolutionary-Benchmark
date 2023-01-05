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


        _intBufferLookup = GetBufferLookup<TraitBufferComponent<int>>(false);
        _floatBufferLookup = GetBufferLookup<TraitBufferComponent<float>>(false);
    }

    protected override void OnUpdate()
    {
        RefRW<SimStateComponent> simState = GetSingletonRW<SimStateComponent>();


        if (simState.ValueRO.phase == Phase.start) 
        {

            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);

            EntityCommandBuffer ecb = GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            JobHandle handle = new ApplyJob
            {
                intLookup = _intBufferLookup,
                floatLookup = _floatBufferLookup,
                ecb = ecbParallel,
            }.ScheduleParallel(Dependency);

            handle.Complete();

            simState = GetSingletonRW<SimStateComponent>();
            simState.ValueRW.phase = Phase.running;
        }

    }

    [BurstCompile]
    private partial struct ApplyJob: IJobEntity 
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<int>> intLookup;
        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<float>> floatLookup;

        public EntityCommandBuffer.ParallelWriter ecb;

        [BurstCompile]
        public void Execute(ApplyAspect aspect, [EntityInQueryIndex] int sortKey) 
        {
            aspect.SetTraits(intLookup, floatLookup, ecb, sortKey);;
        }
    }
}
