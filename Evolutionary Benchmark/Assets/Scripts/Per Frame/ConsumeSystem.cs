using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
[UpdateAfter(typeof(ControlSystem))]
public partial struct ConsumeSystem : ISystem
{

    EntityQuery _eatQuery;

    ComponentTypeHandle<EatenByComponent> _eatenByTypeHandle;
    ComponentTypeHandle<FoodComponent> _foodTypeHandle;
    EntityTypeHandle _entityTypeHandle;
    //BufferLookup<Child> childLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<EatenByComponent,FoodComponent>().AddAdditionalQuery().WithNone<DestroyComponent>();

        _eatQuery = state.GetEntityQuery(builder);
        state.RequireForUpdate<SimStateComponent>();
        builder.Dispose();


        _eatenByTypeHandle = state.GetComponentTypeHandle<EatenByComponent>(false);
        _foodTypeHandle = state.GetComponentTypeHandle<FoodComponent>(true);
        _entityTypeHandle = state.GetEntityTypeHandle();
        //childLookup = state.GetBufferLookup<Child>();
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

            _eatenByTypeHandle.Update(ref state);
            _entityTypeHandle.Update(ref state);
            _foodTypeHandle.Update(ref state);
            //childLookup.Update(ref state);

            JobHandle handle = new EatJob { ecb = ecb, eatenByTypeHandle = _eatenByTypeHandle, entityTypeHandle =_entityTypeHandle/*, children = childLookup*/, foodTypeHandle = _foodTypeHandle}.ScheduleParallel(_eatQuery, state.Dependency);
            handle.Complete();

        }
    }
}

/*[BurstCompile]
public partial struct EatJob : IJobEntity 
{

    public EntityCommandBuffer.ParallelWriter ecb;

    [BurstCompile]
    public void Execute(ref EatAspect aspect, [EntityInQueryIndex] int sortKey) 
    {
        aspect.Consume(ecb, sortKey);
    }
}
*/

[BurstCompile]
public struct EatJob : IJobChunk
{

    public ComponentTypeHandle<EatenByComponent> eatenByTypeHandle;

    [ReadOnly]
    public ComponentTypeHandle<FoodComponent> foodTypeHandle;

    public EntityTypeHandle entityTypeHandle;

    public EntityCommandBuffer.ParallelWriter ecb;

    //public BufferLookup<Child> children;

    [BurstCompile]
    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
    {
        //offset = 0;
        Assert.IsFalse(useEnabledMask);

        var chunkEatenBy = chunk.GetNativeArray(eatenByTypeHandle);
        var entities = chunk.GetNativeArray(entityTypeHandle);
        var chunkFood = chunk.GetNativeArray(foodTypeHandle);

        if(entities.Length > 0 && chunkEatenBy.Length > 0 && chunkFood.Length > 0) 
        {
            for (int i = 0, chunkEntityCount = chunk.Count; i < chunkEntityCount; i++)
            {
                Consume(chunkEatenBy, entities, chunkFood, i);
            }

            chunkEatenBy.Dispose();
            entities.Dispose();
            chunkFood.Dispose();
        }


        




    }


    [BurstCompile]
    private void Consume(NativeArray<EatenByComponent> eaten, NativeArray<Entity> entities, NativeArray<FoodComponent> food, int index)
    {
        float energyToAdd = 0f;
        float healthToAdd = 0f;
        //Prioritize energy regen

        var test = eaten[index].maxEnergy;

        float maxEnergy = test.ValueRO.value;
        float maxHealth = test.ValueRO.value;
        float energy = eaten[index].energy.ValueRO.value;
        float nurishment = food[index].value;
        float health = eaten[index].health.ValueRO.value;


        if (maxEnergy <= energy + nurishment)
        {
            energyToAdd = maxEnergy - energy;
            healthToAdd = nurishment - energyToAdd;
        }
        else if (energy < maxEnergy)
        {
            energyToAdd = nurishment;
        }

        eaten[index].energy.ValueRW.value = energy + energyToAdd;
        eaten[index].health.ValueRW.value = math.min(health + healthToAdd, maxHealth);

        /*bool success = children.TryGetBuffer(entities[index], out DynamicBuffer<Child> buffer);

        if (success) 
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                //ecb.AddComponent<DestroyComponent>()
                ecb.DestroyEntity(chunkSize + offset, buffer[i].Value);
                //ecb.
                offset++;
            }
        }*/

        ecb.RemoveComponent<EatenByComponent>(index, entities[index]);
        ecb.RemoveComponent<EntityTypeComponent>(index, entities[index]);
        ecb.AddComponent<DestroyComponent>(index, entities[index]);
    }

}