using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

[assembly: RegisterGenericJobType(typeof(CombineJob<TestCombinationAlgorithm>))]

[UpdateAfter(typeof(SelectionSystem))]
public partial class CombinationSystem : SystemBase
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


        if(simState.ValueRO.phase == Phase.end) 
        {
            RefRW<RandomComponent> random = GetSingletonRW<RandomComponent>();
            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            NativeList<Entity> selectedList = new NativeList<Entity>(AllocatorManager.TempJob);
            NativeList<FitnessComponent> fitnessList = new NativeList<FitnessComponent>(AllocatorManager.TempJob);
            NativeList<KeepComponent> keepList = new NativeList<KeepComponent>(AllocatorManager.TempJob);

            Entities.WithAll<KeepComponent>().ForEach((Entity entity, in FitnessComponent fitnessComponent, in KeepComponent keep) =>
            {
                selectedList.Add(entity);
                fitnessList.Add(fitnessComponent);
                keepList.Add(keep);

            }).Schedule(Dependency).Complete();


            NativeArray<Entity> selected =selectedList.ToArray(Allocator.TempJob);
            NativeArray<FitnessComponent> fitness = fitnessList.ToArray(Allocator.TempJob);
            NativeArray<KeepComponent> toKeep = keepList.ToArray(Allocator.TempJob);
            selectedList.Dispose();
            fitnessList.Dispose();
            keepList.Dispose();


            JobHandle handle = new CombineJob<TestCombinationAlgorithm>
            {
                random = random,
                floatBufferLookup = _floatBufferLookup,
                intBufferLookup = _intBufferLookup,
                selectedEntities = selected,
                ecb = ecbParallel,
                fitness = fitness,
                toKeep=toKeep,
            }.Schedule((int)math.floor(selected.Length / 2f), 100, Dependency);

            handle.Complete();

            selected.Dispose();
            fitness.Dispose();
            toKeep.Dispose();
            ecb.Playback(World.Unmanaged.EntityManager);
            ecb.Dispose();

        }

    }

}

[BurstCompile]
public struct CombineJob<Alg> : IJobParallelFor where Alg : struct, ICombinationAlgorithn
{
    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> random;

    [NativeDisableParallelForRestriction]
    public BufferLookup<TraitBufferComponent<int>> intBufferLookup;

    [NativeDisableParallelForRestriction]
    public BufferLookup<TraitBufferComponent<float>> floatBufferLookup;

    [NativeDisableParallelForRestriction]
    public NativeArray<Entity> selectedEntities;

    [NativeDisableParallelForRestriction]
    public NativeArray<FitnessComponent> fitness;

    [NativeDisableParallelForRestriction]
    public NativeArray<KeepComponent> toKeep;

    public EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void Execute(int index)
    {
        Alg alg = new Alg { };

        alg.SelectAndCreate(random, selectedEntities, fitness, toKeep, index, ecb);
    }
}


public interface ICombinationAlgorithn 
{
    public void SelectAndCreate(RefRW<RandomComponent> random, NativeArray<Entity> entities, NativeArray<FitnessComponent> fitness, NativeArray<KeepComponent> toKeep, int index, EntityCommandBuffer.ParallelWriter ecb);
}