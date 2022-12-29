using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;


[UpdateAfter(typeof(EpochTimer))]
public partial class InputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        RequireForUpdate<SimStateComponent>();
        BufferLookup<SeeBufferComponent>  _seeBufferLookup = GetBufferLookup<SeeBufferComponent>(false);


        EntityQuery _SeeQuery = EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent));
            
        //new EntityQueryBuilder(Allocator.Temp)
        //.WithAllRW<LocalToWorldTransform>()
        //.AddAdditionalQuery()
        //.WithAllRW<EntityTypeComponent>()
        //.Build(this);

        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.running)
        {
            //EntityQuery query = state.WorldUnmanaged.EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent));



            NativeArray<LocalToWorldTransform> transforms = _SeeQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);
            NativeArray<EntityTypeComponent> types = _SeeQuery.ToComponentDataArray<EntityTypeComponent>(Allocator.TempJob);

            _seeBufferLookup.Update(this);
            JobHandle handle = new RadiusSeeJob { bufferLookup = _seeBufferLookup, transforms = transforms, types = types }.ScheduleParallel(this.Dependency);
            handle.Complete();

            transforms.Dispose();
            types.Dispose();
        }
    }


    [BurstCompile]
    private partial struct RadiusSeeJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<SeeBufferComponent> bufferLookup;

        [ReadOnly]
        public NativeArray<LocalToWorldTransform> transforms;

        [ReadOnly]
        public NativeArray<EntityTypeComponent> types;



        [BurstCompile]
        public void Execute(ref SeeRadiusAspect aspect, [EntityInQueryIndex] int index)
        {
            aspect.UpdateView(transforms, types, bufferLookup);
        }
    }
}

/*
[BurstCompile]
[UpdateAfter(typeof(EpochTimer))]
public partial struct InputSystem : ISystem
{

    private BufferLookup<SeeBufferComponent> _seeBufferLookup;

    private EntityQuery _SeeQuery;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();
        _seeBufferLookup = state.GetBufferLookup<SeeBufferComponent>(false);

        _SeeQuery = new EntityQueryBuilder(Allocator.Temp)
        .WithAllRW<LocalToWorldTransform>()
        .AddAdditionalQuery()
        .WithAllRW<EntityTypeComponent>()
        .Build(ref state);//(ref state)
        
         //var builder = new EntityQueryBuilder(Allocator.Temp)
         //.WithAllRW<LocalToWorldTransform>()
         //.AddAdditionalQuery()
         //.WithAllRW<EntityTypeComponent>();


         //_SeeQuery = state.EntityManager.CreateEntityQuery(in builder); 
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

        //_SeeQuery.Dispose();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if(success && simState.phase == Phase.running) 
        {
            //EntityQuery query = state.WorldUnmanaged.EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent));

           

            NativeArray<LocalToWorldTransform> transforms = _SeeQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);
            NativeArray<EntityTypeComponent> types = _SeeQuery.ToComponentDataArray<EntityTypeComponent>(Allocator.TempJob);

            _seeBufferLookup.Update(ref state);
            JobHandle handle = new RadiusSeeJob { bufferLookup = _seeBufferLookup, transforms =transforms, types =types}.ScheduleParallel(state.Dependency);
            handle.Complete();

            transforms.Dispose();
            types.Dispose();
        }

    }

    [BurstCompile]
    private partial struct RadiusSeeJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<SeeBufferComponent> bufferLookup;

        [ReadOnly]
        public NativeArray<LocalToWorldTransform> transforms;

        [ReadOnly]
        public NativeArray<EntityTypeComponent> types;



        [BurstCompile]
        public void Execute(ref SeeRadiusAspect aspect, [EntityInQueryIndex] int index) 
        {
            aspect.UpdateView(transforms, types , bufferLookup);
        }
    }
}
*/
