using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


/// <summary>
/// System that is responsible for instantiating, re-positioning and reseting stats of entities
/// </summary>
[UpdateAfter(typeof(EpochTimer))]
public partial class SpawnerSystem : SystemBase//ISystem
{

    BufferLookup<TraitBufferComponent<int>> _intBufferLookup;
    BufferLookup<TraitBufferComponent<float>> _floatBufferLookup;

    protected override void OnCreate()//(ref SystemState state)
    {
        RequireForUpdate<SimStateComponent>();

        //Intialize buffer lookup components
        _intBufferLookup = GetBufferLookup<TraitBufferComponent<int>>(false);
        _floatBufferLookup = GetBufferLookup<TraitBufferComponent<float>>(false);
    }

    protected override void OnUpdate()//(ref SystemState state)
    {
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);

        if (success && simState.phase == Phase.start)
        {
            //Get entity command buffer
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
            
            //Fetch components from entities marked as keep from previous generation and remove keep tag
            JobHandle handle2 = Entities.WithAll<KeepComponent>().ForEach((Entity entity/*, ref TransformAspect transform, ref HealthComponent health,
                ref EnergyComponent energy*/, ref MaxHealthComponent maxHealth, ref MaxEnergyComponent maxEnergy) =>
            {
                toKeepList.Add(entity);
                toKeepMaxHealthComponentList.Add(new RefRef<MaxHealthComponent>(ref maxHealth));
                toKeepMaxEnergyComponentList.Add(new RefRef<MaxEnergyComponent>(ref maxEnergy));
                
                ecb2.RemoveComponent<KeepComponent>(entity);
            }).Schedule(Dependency);


            handle2.Complete();
            Entity fittest= new Entity { Version = -100};
            Entities.WithAll<BestInGenComponent>().ForEach((Entity entity) =>
            {
                fittest = entity;
                ecb2.RemoveComponent<BestInGenComponent>(entity);
            }).Run();

            //Fetch components that need to be generated

            

            //Apply changes made in command buffer
            ecb2.Playback(EntityManager);
            ecb2.Dispose();

            var toKeep = toKeepList.ToArray(Allocator.TempJob);
            var toKeepMaxHealth = toKeepMaxHealthComponentList.ToArray(Allocator.TempJob);
            var toKeepMaxEnergy= toKeepMaxEnergyComponentList.ToArray(Allocator.TempJob);
            //Spawn = Create new entity
            //Generate = Create entity from set of traits
            //ReSpawn = Set entity stats back to max and reposition

            int toSpawnCount = simState.maxEntities - toKeepList.Length;

            //Remove this later
            //toSpawnCount += toKeepList.Length;

            //Dispose of lists
            toKeepList.Dispose();
            toKeepMaxHealthComponentList.Dispose();
            toKeepMaxEnergyComponentList.Dispose();



            float fields = simState.fields;

            //Per spawner
            var temp = math.ceil(toSpawnCount / fields);
            //The number of entities to spawn new per spawner
            int toSpawnSpawnCount = (int)(temp);

            //The number of entities to regenerate per spawner
            int toKeepSpawnCount = (int)(math.ceil(toKeep.Length / fields));

            //The number of food to spawn per spawner
            int foodCount = (int)(math.ceil(simState.maxEntities / fields * math.max(simState.currentEpoch/10, 1f))) ;

            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);

            ecb2 = new EntityCommandBuffer(Allocator.TempJob);
            ecb2Parallel = ecb2.AsParallelWriter();

            RefRW<SimStateComponent> state = SystemAPI.GetSingletonRW<SimStateComponent>();
            JobHandle handle = new SpawnJob { ecb = ecb,
                spawnCount = toSpawnSpawnCount,
                maxToSpawnCount = toSpawnCount,
                toKeepSpawnCount = toKeepSpawnCount,
                toKeep = toKeep,
                toKeepMaxEnergy = toKeepMaxEnergy,
                toKeepMaxHealth = toKeepMaxHealth,
                intBufferLookup = _intBufferLookup,
                floatBufferLookup = _floatBufferLookup,
                ecb2 = ecb2Parallel,
                epoch = state.ValueRO.currentEpoch,
                fittestEntity = fittest,
            }.ScheduleParallel(Dependency);


            new FoodSpawnJob { ecb = ecb, foodSpawnCount = foodCount, maxToSpawnCount=toSpawnCount}.ScheduleParallel(handle).Complete();

            toKeep.Dispose();
            toKeepMaxEnergy.Dispose();
            toKeepMaxHealth.Dispose();
            ecb2.Playback(EntityManager);
            ecb2.Dispose();

            //Reset killed stats because they have been spawned new
            state = SystemAPI.GetSingletonRW<SimStateComponent>();
            state.ValueRW.killedThisGen = 0;

        }
    }

    [BurstCompile]
    private partial struct SpawnJob : IJobEntity 
    {

        public Entity fittestEntity;

        public EntityCommandBuffer.ParallelWriter ecb;

        public EntityCommandBuffer.ParallelWriter ecb2;

        [ReadOnly]
        public int spawnCount;

        [ReadOnly]
        public int maxToSpawnCount;


        [ReadOnly]
        //How many entities from to KeepCount
        public int toKeepSpawnCount;

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

        [ReadOnly]
        public int epoch;

        [BurstCompile]
        public void Execute(SpawnAspect aspect, [EntityInQueryIndex] int sortKey) 
        {
            aspect.SpawnEntity(ecb, ecb2, sortKey, spawnCount, maxToSpawnCount, toKeepSpawnCount, toKeep, toKeepMaxHealth, toKeepMaxEnergy, intBufferLookup, floatBufferLookup, epoch, fittestEntity);
        }
    }

    [BurstCompile]
    private partial struct FoodSpawnJob : IJobEntity
    {

        public EntityCommandBuffer.ParallelWriter ecb;

        [ReadOnly]
        public int foodSpawnCount;

        [ReadOnly]
        public int maxToSpawnCount;

        [BurstCompile]
        public void Execute(FoodSpawnAspect aspect, [EntityInQueryIndex] int sortKey)
        {
            aspect.SpawnEntity(ecb, sortKey + maxToSpawnCount * 6, foodSpawnCount);
        }
    }
}
