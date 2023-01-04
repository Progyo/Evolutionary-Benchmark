using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


[UpdateAfter(typeof(EvaluationSystem))]
public partial class SelectionSystem : SystemBase
{

    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate<SimStateComponent>();
    }
    protected override void OnUpdate()
    {
        bool success = TryGetSingleton<SimStateComponent>(out SimStateComponent simState);
        

        if (success && simState.phase == Phase.end)
        {

            
            float totalFitness = 0f;
            


            //Sort fitness
            Dictionary<Entity, float> fitnessValues = new Dictionary<Entity, float>();
            foreach (var (fitness, entity) in SystemAPI.Query<FitnessComponent>().WithEntityAccess()) 
            {
                fitnessValues.Add(entity, fitness.value);
                totalFitness += fitness.value;
            }
            int count = fitnessValues.Count;
            
            
            //https://stackoverflow.com/questions/289/how-do-you-sort-a-dictionary-by-value
            List<KeyValuePair<Entity, float>> population = fitnessValues.ToList();

            population.Sort(
                delegate (KeyValuePair<Entity, float> pair1,
                KeyValuePair<Entity, float> pair2)
                {
                    //Highest fitness is first
                    return -pair1.Value.CompareTo(pair2.Value);
                }
            );

            int spawnerCount = simState.fields;

            /*Entities.WithAll<SpawnPointComponent>().ForEach((Entity entity, SpawnPointComponent spawn) =>
            {
                if(spawn.type == EntityType.blob) 
                {
                    spawnerCount++;
                }
                
            }).Run();*/


            int toKeep = math.max(0,simState.maxEntities / 2 - simState.killedThisGen);
            //SUS(sorted, totalFitness, toKeep);


            //SUS
            RefRW<RandomComponent> random = GetSingletonRW<RandomComponent>();

            float dist = totalFitness / toKeep;

            float start = random.ValueRW.value.NextFloat(0, dist);

            NativeArray<float> pointers = new NativeArray<float>(toKeep, Allocator.TempJob);

            for (int i = 0; i < toKeep; i++)
            {
                pointers[i] = start + i * dist;
            }

            //RWS
            NativeArray<Population> populationStruct = new NativeArray<Population>(population.Count, Allocator.TempJob);
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent);
            EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

            //Convert to native array
            for (int i = 0; i < population.Count; i++)
            {
                populationStruct[i] = new Population { entity = population[i].Key, fitness = population[i].Value };
            }

            JobHandle handle = new RWSJob { pointers = pointers, population = populationStruct, ecb = ecbParallel }.Schedule(pointers.Length, pointers.Length / 100, Dependency);

            handle.Complete();
            populationStruct.Dispose();

            ecb.Playback(EntityManager);
            ecb.Dispose();


            pointers.Dispose();

            if(population.Count> 0) 
            {
                //Log average fitness
                AverageAndMaxFitnessMetric fitnessMetric = new AverageAndMaxFitnessMetric { averageFitness = totalFitness / count, maxFitness = population[0].Value, metricCount = 0 };
                LoggerSystem.Logger.LogEpoch(simState.currentEpoch, fitnessMetric);
            }

            KilledMetric killedMetric = new KilledMetric { killedThisGen = simState.killedThisGen };
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, killedMetric);


            /*for (int i = 0; i < math.min(sorted.Count, 10); i++)
            {
                Debug.Log(GetComponent<LocalToWorldTransform>(sorted[i].Key).Value.Position);
            }*/

            int keepCount = 0;
            Entities.WithAll<KeepComponent>().ForEach((Entity entity) =>
            {
                keepCount++;
            }).Run();

            SelectedMetric selectedMetric = new SelectedMetric { selected = keepCount };
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, selectedMetric);

            //UnityEngine.Debug.Log(keepCount);
        }

    }

    //SUS Algorithm
    /*private void SUS(List<KeyValuePair<Entity, float>> population, float totalFitness, int toKeep) 
    {
        

        RefRW<RandomComponent> random = GetSingletonRW<RandomComponent>();

        float dist = totalFitness / toKeep;

        float start = random.ValueRW.value.NextFloat(0, dist);

        NativeArray<float> pointers = new NativeArray<float>(toKeep, Allocator.Temp);

        for (int i = 0; i < toKeep; i++)
        {
            pointers[i] = start + i * dist;
        }


        RWS(population, pointers);


        pointers.Dispose();
    }

    private void RWS(List<KeyValuePair<Entity, float>> population,  NativeArray<float> pointers) 
    {
        
        //NativeList<Entity> keep = new NativeList<Entity>();
        NativeArray<Population> populationStruct = new NativeArray<Population>(population.Count, Allocator.Temp);

        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        EntityCommandBuffer.ParallelWriter ecbParallel = ecb.AsParallelWriter();

        //Convert to native array
        for (int i = 0; i < population.Count; i++)
        {
            populationStruct[i] = new Population { entity = population[i].Key, fitness = population[i].Value };
        }

        JobHandle handle = new RWSJob { pointers = pointers, population = populationStruct, ecb = ecbParallel }.Schedule(pointers.Length, pointers.Length / 100, Dependency);

        handle.Complete();
        populationStruct.Dispose();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
   */
    private struct Population 
    {
        public Entity entity;
        public float fitness;
    }

    
    [BurstCompile]
    private struct RWSJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<float> pointers;

        [ReadOnly]
        public NativeArray<Population> population;

        public EntityCommandBuffer.ParallelWriter ecb;
        //public NativeList<Entity> toKeep;

        [BurstCompile]
        public void Execute(int index)
        {
            float P = pointers[index];
            int I = 0;

            while(Sum(I) < P) 
            {
                I++;
            }

            //Debug.Log(population[I].fitness);
            //toKeep.Add(population[I].entity);
            ecb.AddComponent<KeepComponent>(index, population[I].entity);
        }

        [BurstCompile]
        private float Sum(int I) 
        {
            float sum = 0f;
            for (int i = 0; i < I; i++)
            {
                sum += population[i].fitness;
            }
            return sum;
        }
    }
}
