using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

[UpdateAfter(typeof(ApplySystem))]
public partial class InputSystem : SystemBase
{
    BufferLookup<SeeBufferComponent> _seeBufferLookup;

    EntityQuery _SeeQuery;

    EntityQuery _SeeEntityQuery;

    List<NativeArraysToDispose> _toDispose;


    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<SimStateComponent>();
        _seeBufferLookup = GetBufferLookup<SeeBufferComponent>(false);
        _SeeQuery = EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent), typeof(FieldIdSharedComponent)) ;
        _SeeEntityQuery = EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent), typeof(EyeRadiusComponent), typeof(FieldIdSharedComponent));
        _toDispose = new List<NativeArraysToDispose>();
    }

    protected override void OnUpdate()
    {


         
            
        //new EntityQueryBuilder(Allocator.Temp)
        //.WithAllRW<LocalToWorldTransform>()
        //.AddAdditionalQuery()
        //.WithAllRW<EntityTypeComponent>()
        //.Build(this);

        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.running)
        {
            JobHandle handle = Dependency;

            //EntityQuery query = state.WorldUnmanaged.EntityManager.CreateEntityQuery(typeof(LocalToWorldTransform), typeof(EntityTypeComponent));
            for (int i = 0; i < simState.fields; i++)
            {
                _SeeEntityQuery.SetSharedComponentFilter<FieldIdSharedComponent>(new FieldIdSharedComponent { value = i });
                _SeeQuery.SetSharedComponentFilter<FieldIdSharedComponent>(new FieldIdSharedComponent { value = i });

                NativeArray<LocalToWorldTransform> transforms = _SeeQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);
                NativeArray<EntityTypeComponent> types = _SeeQuery.ToComponentDataArray<EntityTypeComponent>(Allocator.TempJob);

                NativeArray<Entity> entities = _SeeQuery.ToEntityArray(Allocator.TempJob);


                NativeArray<EyeRadiusComponent> lookRadi = _SeeEntityQuery.ToComponentDataArray<EyeRadiusComponent>(Allocator.TempJob);

                NativeArray<LocalToWorldTransform> lookTransform = _SeeEntityQuery.ToComponentDataArray<LocalToWorldTransform>(Allocator.TempJob);

                NativeArray<Entity> lookEntities = _SeeEntityQuery.ToEntityArray(Allocator.TempJob);

                _toDispose.Add(new NativeArraysToDispose { seeTransforms = transforms, seeTypes = types, seeEntities = entities, lookEntities = lookEntities,
                lookRadi = lookRadi, lookTransform = lookTransform});

                _seeBufferLookup.Update(this);
                //handle = new RadiusSeeJob { bufferLookup = _seeBufferLookup, transforms = transforms, types = types, entities = entities }.ScheduleParallel(_SeeEntityQuery, handle);
                handle = new RadiusSeeJob
                {
                    bufferLookup = _seeBufferLookup,
                    seeTransforms = transforms,
                    seeTypes = types,
                    seeEntities = entities,
                    lookEntities = lookEntities,
                    lookRadi = lookRadi,
                    lookTransform = lookTransform,
                }.Schedule(lookEntities.Length, lookEntities.Length/100, handle);
            }

            handle.Complete();

            foreach(NativeArraysToDispose toDipose in _toDispose) 
            {
                toDipose.seeTransforms.Dispose();
                toDipose.seeTypes.Dispose();
                toDipose.seeEntities.Dispose();

                toDipose.lookEntities.Dispose();
                toDipose.lookRadi.Dispose();
                toDipose.lookTransform.Dispose();
            }
            _toDispose.Clear();

        }
    }


    private struct NativeArraysToDispose
    {
        public NativeArray<LocalToWorldTransform> seeTransforms;
        public NativeArray<EntityTypeComponent> seeTypes;
        public NativeArray<Entity> seeEntities;
        
        
        public NativeArray<EyeRadiusComponent> lookRadi;
        public NativeArray<LocalToWorldTransform> lookTransform;
        public NativeArray<Entity> lookEntities;
    }

    [BurstCompile]
    private struct RadiusSeeJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<SeeBufferComponent> bufferLookup;


        //See items

        [ReadOnly]
        public NativeArray<LocalToWorldTransform> seeTransforms;

        [ReadOnly]
        public NativeArray<EntityTypeComponent> seeTypes;

        [ReadOnly]
        public NativeArray<Entity> seeEntities;


        //Entity stuff

        [ReadOnly]
        public NativeArray<EyeRadiusComponent> lookRadi;

        [ReadOnly]
        public NativeArray<LocalToWorldTransform> lookTransform;


        [ReadOnly]
        public NativeArray<Entity> lookEntities;

        [BurstCompile]
        public void Execute(int index)
        {
            UpdateView(lookEntities[index], lookTransform[index], lookRadi[index]);
        }

        [BurstCompile]
        public void UpdateView(Entity entity, LocalToWorldTransform transformAspect, EyeRadiusComponent seeRadius)
        {

        bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);

            if (!successful)
            {
                return;
            }

            buffer.Clear();

            //Super slow
            for (int i = 0; i < seeTransforms.Length; i++)
            {
                float3 pos = seeTransforms[i].Value.Position;//SystemAPI.GetComponent<LocalToWorldTransform>(entities[i]).Value.Position;
                float distance = math.distance(transformAspect.Value.Position, pos);

                if (distance == 0)
                {
                    continue;
                }

                if (distance < seeRadius.value)
                {
                    EntityType type = seeTypes[i].value;
                    ItemType itemType = getType(type);

                    //RefRW<Entity> ent = new RefRW<Entity>(entities, i);

                    buffer.Add(new SeeBufferComponent { distance = distance, itemType = itemType, position = pos, entity = seeEntities[i] });
                }



            }
        }

        [BurstCompile]
        private ItemType getType(EntityType entityType)
        {
            if (entityType == EntityType.blob)
            {
                return ItemType.enemy;
            }
            else if (entityType == EntityType.food)
            {
                return ItemType.food;
            }


            return ItemType.enemy;
        }

    }
    /*
    [BurstCompile]
    private partial struct RadiusSeeJob : IJobEntity
    {
        [NativeDisableParallelForRestriction]
        public BufferLookup<SeeBufferComponent> bufferLookup;

        [ReadOnly]
        public NativeArray<LocalToWorldTransform> transforms;

        [ReadOnly]
        public NativeArray<EntityTypeComponent> types;

        [ReadOnly]
        public NativeArray<Entity> entities;

        [BurstCompile]
        public void Execute(ref SeeRadiusAspect aspect, [EntityInQueryIndex] int index)
        {
            aspect.UpdateView(transforms, types, bufferLookup, entities);
        }
    } */
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
