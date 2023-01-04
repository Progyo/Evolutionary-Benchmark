using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


//[BurstCompile]
[UpdateAfter(typeof(EpochTimer))]
public partial class SpawnerSystem : SystemBase//ISystem
{

    BufferLookup<TraitBufferComponent<int>> _intBufferLookup;
    BufferLookup<TraitBufferComponent<float>> _floatBufferLookup;

    //[BurstCompile]
    protected override void OnCreate()//(ref SystemState state)
    {
        RequireForUpdate<SimStateComponent>();

        _intBufferLookup = GetBufferLookup<TraitBufferComponent<int>>(false);
        _floatBufferLookup = GetBufferLookup<TraitBufferComponent<float>>(false);
    }

    //[BurstCompile]
    /*public void OnDestroy()//(ref SystemState state)
    {

    }*/

   // [BurstCompile]
    protected override void OnUpdate()//(ref SystemState state)
    {
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.start)
        {

            var ecb = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged).AsParallelWriter();

            NativeList<Entity> toKeepList = new NativeList<Entity>(AllocatorManager.TempJob);
            NativeList<RefRef<TransformAspect>> toKeepTransformList = new NativeList<RefRef<TransformAspect>>(AllocatorManager.TempJob);
            NativeList<RefRef<HealthComponent>> toKeepHealthComponentList = new NativeList<RefRef<HealthComponent>>(AllocatorManager.TempJob);
            NativeList<RefRef<EnergyComponent>> toKeepEnergyComponentList = new NativeList<RefRef<EnergyComponent>>(AllocatorManager.TempJob);
            NativeList<RefRef<MaxHealthComponent>> toKeepMaxHealthComponentList = new NativeList<RefRef<MaxHealthComponent>>(AllocatorManager.TempJob);
            NativeList<RefRef<MaxEnergyComponent>> toKeepMaxEnergyComponentList = new NativeList<RefRef<MaxEnergyComponent>>(AllocatorManager.TempJob);
            //NativeList<Entity> toGenerate = new NativeList<Entity>();


            EntityCommandBuffer ecb2 = new EntityCommandBuffer(Allocator.TempJob);
            EntityCommandBuffer.ParallelWriter ecb2Parallel = ecb2.AsParallelWriter();
            
            JobHandle handle2 = Entities.WithAll<KeepComponent>().ForEach((Entity entity/*, ref TransformAspect transform, ref HealthComponent health,
                ref EnergyComponent energy*/, ref MaxHealthComponent maxHealth, ref MaxEnergyComponent maxEnergy) =>
            {
                toKeepList.Add(entity);
                
                /*toKeepTransformList.Add(new RefRef<TransformAspect>(ref transform));
                toKeepHealthComponentList.Add(new RefRef<HealthComponent>(ref health));
                toKeepEnergyComponentList.Add(new RefRef<EnergyComponent>(ref energy));*/
                toKeepMaxHealthComponentList.Add(new RefRef<MaxHealthComponent>(ref maxHealth));
                toKeepMaxEnergyComponentList.Add(new RefRef<MaxEnergyComponent>(ref maxEnergy));
                
                ecb2.RemoveComponent<KeepComponent>(entity);
            }).Schedule(Dependency);

            handle2.Complete();

            //Entities.WithAll<>

            var toKeep = toKeepList.ToArray(Allocator.TempJob);
            /*var toKeepHealth = toKeepHealthComponentList.ToArray(Allocator.Temp);
            var toKeepEnergy = toKeepEnergyComponentList.ToArray(Allocator.Temp);*/
            var toKeepMaxHealth = toKeepMaxHealthComponentList.ToArray(Allocator.TempJob);
            var toKeepMaxEnergy= toKeepMaxEnergyComponentList.ToArray(Allocator.TempJob);
            //Spawn = Create new entity
            //Generate = Create entity from set of traits
            //ReSpawn = Set entity stats back to max and reposition

            int toSpawnCount = simState.maxEntities - toKeepList.Length*2;

            //Remove this later
            toSpawnCount += toKeepList.Length;

            //Dispose of lists
            toKeepList.Dispose();
            /*toKeepTransformList.Dispose();
            toKeepHealthComponentList.Dispose();
            toKeepEnergyComponentList.Dispose();*/
            toKeepMaxHealthComponentList.Dispose();
            toKeepMaxEnergyComponentList.Dispose();

            //toGenerate.ToArray(Allocator.Temp);
            //toGenerate.Dispose();

            ecb2.Playback(EntityManager);
            
            ecb2.Dispose();

            //Per spawner
            int toSpawnSpawnCount = (int)(math.ceil(toSpawnCount / simState.fields));

            int toKeepSpawnCount = (int)(math.ceil(toKeep.Length / simState.fields));

            int foodCount = (int)(math.ceil(simState.maxEntities / simState.fields));


            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);

            JobHandle handle = new SpawnJob {ecb = ecb,
                spawnCount= toSpawnSpawnCount,
                toKeepSpawnCount=toKeepSpawnCount,
                foodSpawnCount = foodCount,
                toKeep = toKeep,
                toKeepMaxEnergy = toKeepMaxEnergy,
                toKeepMaxHealth = toKeepMaxHealth,
                intBufferLookup = _intBufferLookup,
                floatBufferLookup = _floatBufferLookup,
            }.ScheduleParallel(Dependency);
            handle.Complete();

            toKeep.Dispose();
            toKeepMaxEnergy.Dispose();
            toKeepMaxHealth.Dispose();

            RefRW<SimStateComponent> state = SystemAPI.GetSingletonRW<SimStateComponent>();
            state.ValueRW.killedThisGen = 0;

        }
    }

    [BurstCompile]
    private partial struct SpawnJob : IJobEntity 
    {

        public EntityCommandBuffer.ParallelWriter ecb;

        [ReadOnly]
        public int spawnCount;

        [ReadOnly]
        //How many entities from to KeepCount
        public int toKeepSpawnCount;

        [ReadOnly]
        public int foodSpawnCount;

        [NativeDisableParallelForRestriction]
        public NativeArray<Entity> toKeep;
        /*[NativeDisableParallelForRestriction]
        public NativeArray<RefRef<TransformAspect>> toKeepTransforms;
        [NativeDisableParallelForRestriction]
        public NativeArray<RefRef<HealthComponent>> toKeepHealth;
        [NativeDisableParallelForRestriction]
        public NativeArray<RefRef<EnergyComponent>> toKeepEnergy;*/
        [NativeDisableParallelForRestriction]
        public NativeArray<RefRef<MaxHealthComponent>> toKeepMaxHealth;
        [NativeDisableParallelForRestriction]
        public NativeArray<RefRef<MaxEnergyComponent>> toKeepMaxEnergy;


        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<int>> intBufferLookup;

        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<float>> floatBufferLookup;


        [BurstCompile]
        public void Execute(SpawnAspect aspect, [EntityInQueryIndex] int sortKey) 
        {
            //, toKeepTransforms, toKeepHealth, toKeepEnergy
            aspect.SpawnEntity(ecb, sortKey, spawnCount, toKeepSpawnCount, foodSpawnCount, toKeep, toKeepMaxHealth, toKeepMaxEnergy, intBufferLookup, floatBufferLookup);
        }
    }
}
