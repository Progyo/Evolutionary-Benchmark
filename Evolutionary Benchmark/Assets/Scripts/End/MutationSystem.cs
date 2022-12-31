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


[assembly: RegisterGenericJobType(typeof(MutateJob<TestFloatMutationAlgorithm,TestIntMutationAlgorithm>))]

[BurstCompile]
[UpdateAfter(typeof(ControlSystem))]
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

    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        RefRW<SimStateComponent> simState = SystemAPI.GetSingletonRW<SimStateComponent>();

        if ( simState.ValueRO.phase == Phase.end)
        {
            RefRW<RandomComponent> random = SystemAPI.GetSingletonRW<RandomComponent>();
            
            _intBufferLookup.Update(ref state);
            _floatBufferLookup.Update(ref state);
            _entityTypeHandle.Update(ref state);

            //IMutateAlgorithm<int> test = new TestMutationAlgorithm { };

            JobHandle handle = new MutateJob<TestFloatMutationAlgorithm,TestIntMutationAlgorithm> { random = random,
                intBufferLookup = _intBufferLookup,
                floatBufferLookup = _floatBufferLookup,
                entityTypeHandle = _entityTypeHandle
            }.ScheduleParallel(_mutationQuery, state.Dependency);

            handle.Complete();


            simState.ValueRW.phase = Phase.evaluated;
        }
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
                    intAlgorithm.Mutate(ref intbuffer.ElementAt(j), random);
                }
            }

            successful = floatBufferLookup.TryGetBuffer(*chunkEntityPtr, out DynamicBuffer<TraitBufferComponent<float>> floatbuffer);
            if (successful)
            {
                for (int j = 0; j < floatbuffer.Length; j++)
                {
                    floatAlgorithm.Mutate(ref floatbuffer.ElementAt(j), random);
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


    public void Mutate(ref TraitBufferComponent<int> trait, RefRW<RandomComponent> random);
}

public interface IMutateFloatAlgorithm
{
    public RefRW<Entity> entity { get; set; }

    public RefRW<EntityTypeComponent> entityTypeTag { get; set; }


    public void Mutate(ref TraitBufferComponent<float> trait, RefRW<RandomComponent> random);
}