using AOT;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.VisualScripting;

//using System.Runtime.InteropServices;


[assembly: RegisterGenericJobType(typeof(MutateJob<TestFloatMutationAlgorithm,TestIntMutationAlgorithm>))]

[BurstCompile]
[UpdateAfter(typeof(SpawnerSystem))]
public partial struct MutationSystem : ISystem
{

    EntityQuery _mutationQuery;
    BufferLookup<TraitBufferComponent<int>> _intBufferLookup;
    BufferLookup<TraitBufferComponent<float>> _floatBufferLookup;

    EntityTypeHandle _entityTypeHandle;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();

        var builder = new EntityQueryBuilder(Allocator.Temp)
         .WithAllRW<EntityTypeComponent>();

        _mutationQuery = state.GetEntityQuery(builder);


        _intBufferLookup = state.GetBufferLookup<TraitBufferComponent<int>>(false);
        _floatBufferLookup = state.GetBufferLookup<TraitBufferComponent<float>>(false);

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
        RefRW<SimStateComponent> simState = SystemAPI.GetSingletonRW<SimStateComponent>();

        if ( simState.ValueRO.phase == Phase.start)
        {
            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();
            
            _intBufferLookup.Update(ref state);
            _floatBufferLookup.Update(ref state);
            _entityTypeHandle.Update(ref state);

            //IMutateAlgorithm<int> test = new TestMutationAlgorithm { };

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            JobHandle handle = new MutateJob<TestFloatMutationAlgorithm, TestIntMutationAlgorithm> { random = random,
                intBufferLookup = _intBufferLookup,
                floatBufferLookup = _floatBufferLookup,
                entityTypeHandle = _entityTypeHandle,
                mutationChance = 100f,
                floatMutationVariance = 2.5f,
                intMutationVariance = 5,
                ecb = ecbParallel,
                epoch = simState.ValueRO.currentEpoch,
            }.ScheduleParallel(_mutationQuery, state.Dependency);

            handle.Complete();
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            //simState.ValueRW.phase = Phase.running;

            //LogMetric(simState.ValueRO.timeElapsed, simState.ValueRO.currentEpoch, ref state);
        }
    }
    [BurstCompile]
    private void LogMetric(float timeStamp, int epoch, ref SystemState state) 
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        Entity entity = ecb.CreateEntity();
        ecb.AddComponent(entity, new MetricComponent<int> { value = 420, timeStamp = timeStamp, epoch = epoch, type= MetricType.mutation });
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}




[BurstCompile]
public unsafe struct MutateJob<floatAlg,intAlg> : IJobChunk where floatAlg : struct, IMutateFloatAlgorithm where intAlg: struct, IMutateIntAlgorithm
{

    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> random;

    //Lookups (later change to single lookup)
    [NativeDisableParallelForRestriction]
    public BufferLookup<TraitBufferComponent<int>> intBufferLookup;

    [NativeDisableParallelForRestriction]
    public BufferLookup<TraitBufferComponent<float>> floatBufferLookup;

    public EntityTypeHandle entityTypeHandle;

    public EntityCommandBuffer.ParallelWriter ecb;

    [ReadOnly]
    public float mutationChance;

    [ReadOnly]
    public int intMutationVariance;

    [ReadOnly]
    public float floatMutationVariance;

    [ReadOnly]
    public int epoch;


    [BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {

        intAlg intAlgorithm = new intAlg { };
        floatAlg floatAlgorithm = new floatAlg { };

        //algorithm = new TestMutationAlgorithm { };

        var chunkEntityPtr = chunk.GetEntityDataPtrRO(entityTypeHandle);

        //Loop through all entities in chunk
        for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++, chunkEntityPtr++)
        {
            bool successful = intBufferLookup.TryGetBuffer(*chunkEntityPtr, out DynamicBuffer<TraitBufferComponent<int>> intbuffer);
            if (successful)
            {
                for (int j = 0; j < intbuffer.Length; j++)
                {
                    bool mutated = intAlgorithm.Mutate(ref intbuffer.ElementAt(j), random, mutationChance, intMutationVariance);

                    if (mutated) 
                    {
                        int sort = i * intbuffer.Length + j;
                        //Entity entity = ecb.CreateEntity(sort);
                        //ecb.AddComponent(sort, entity, new MetricComponent<int> { value = 1, timeStamp = 0f, epoch = epoch, type = MetricType.mutation });
                    }
                }
            }

            successful = floatBufferLookup.TryGetBuffer(*chunkEntityPtr, out DynamicBuffer<TraitBufferComponent<float>> floatbuffer);
            if (successful)
            {
                for (int j = 0; j < floatbuffer.Length; j++)
                {
                    bool mutated = floatAlgorithm.Mutate(ref floatbuffer.ElementAt(j), random, mutationChance, floatMutationVariance);
            
                    if (mutated)
                    {
                        int sort = chunkEntityCount * intbuffer.Length + i * floatbuffer.Length + j;
                        //Entity entity = ecb.CreateEntity(sort);
                        //ecb.AddComponent(sort, entity, new MetricComponent<int> { value = 1, timeStamp = 0f, epoch = epoch, type = MetricType.mutation });
                    }
                }
            }

        }

    }

    //https://stackoverflow.com/questions/17156179/pointers-of-generic-type
}

public interface IMutateIntAlgorithm
{
    public RefRW<Entity> entity { get; set; }

    public RefRW<EntityTypeComponent> entityTypeTag { get; set; }


    public bool Mutate(ref TraitBufferComponent<int> trait, RefRW<RandomComponent> random, float mutationChance, int mutationVariance);
}

public interface IMutateFloatAlgorithm
{
    public RefRW<Entity> entity { get; set; }

    public RefRW<EntityTypeComponent> entityTypeTag { get; set; }


    public bool Mutate(ref TraitBufferComponent<float> trait, RefRW<RandomComponent> random, float mutationChance, float mutationVariance);
}