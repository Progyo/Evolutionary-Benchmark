using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(EpochTimer))]
public partial struct InputSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        //Dispose of see item arrays
        /*
        foreach (SeeComponent sees in SystemAPI.Query<SeeComponent>())
        {
            if (sees.value.IsCreated) 
            {
                sees.value.Dispose();
            }
            
        }*/
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if(success && simState.phase == Phase.running) 
        {

            /*
            EntityQuery query = new EntityQueryBuilder(Allocator.Temp)
            .WithAllRW<TransformAspect>()
            .AddAdditionalQuery()
            .WithAllRW<EntityTypeComponent>()
            .Build(ref state);

            NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob);
            */

            JobHandle handle = new RadiusSeeJob { }.ScheduleParallel(state.Dependency);
            handle.Complete();

            //entities.Dispose();
        }

    }

    [BurstCompile]
    private partial struct RadiusSeeJob : IJobEntity
    {
        
        [ReadOnly] 
        public BufferLookup<SeeBufferComponent> bufferLookup;

        //[ReadOnly]
        //public NativeArray<Entity> entities;

        [BurstCompile]
        public void Execute(ref SeeRadiusAspect aspect, [EntityInQueryIndex] int index) 
        {

        }
    }
}

