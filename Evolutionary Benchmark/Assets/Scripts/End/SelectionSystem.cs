using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;


[UpdateAfter(typeof(EvaluationSystem))]
public partial class SelectionSystem : SystemBase
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


            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent);

            if (simState.mode == SimulationMode.evolve)
            {
                int spawnerCount = simState.fields;

                int toKeep = (int)math.ceil(simState.maxEntities / 2f);

                if (toKeep > simState.maxEntities - simState.killedThisGen)
                {
                    toKeep = math.max(0, simState.maxEntities - simState.killedThisGen);
                }

                //SUS(sorted, totalFitness, toKeep);


                //SUS
                RefRW<RandomComponent> random = GetSingletonRW<RandomComponent>();

                double dist = totalFitness / (toKeep * 2);

                double start = random.ValueRW.value.NextDouble(0, dist);

                double ptr = start;

                

                int totalOffspring = 0;

                NativeArray<int> offSpring = new NativeArray<int>(toKeep, Allocator.Temp);

                //NativeArray<float> pointers = new NativeArray<float>(toKeep, Allocator.TempJob);
                double sum = 0f;
                for (int i = 0; i < toKeep; i++)
                {
                    int keepEntity = 0;
                    double entityFitness = population[i].Value;
                    for (sum += entityFitness; sum > ptr && totalOffspring < toKeep; ptr += dist)
                    {
                        keepEntity++;
                        totalOffspring++;
                    }

                    ecb.AddComponent<KeepComponent>(population[i].Key, new KeepComponent { offspring = keepEntity });
                }


                ecb.Playback(World.Unmanaged.EntityManager);
                ecb.Dispose();

                offSpring.Dispose();




            }
            else if(simState.mode == SimulationMode.run) 
            {
                foreach (var (fitness, entity) in SystemAPI.Query<FitnessComponent>().WithEntityAccess())
                {
                    //ecb.AddComponent<KeepComponent>(entity, new KeepComponent { offspring = 0 });
                    ecb.AddComponent<DestroyComponent>(entity, new DestroyComponent{ });
                }

                ecb.Playback(World.Unmanaged.EntityManager);
                ecb.Dispose();
            }

            //Metric stuff
            _intBufferLookup.Update(this);
            _floatBufferLookup.Update(this);


            SizeMetric sizeMetric = new SizeMetric { topSize = 0f, totalCount = 0, totalSize = 0f, worstSize = 0f };
            SpeedMetric speedMetric = new SpeedMetric{ topSpeed = 0f, totalCount = 0, totalSpeed = 0f, worstSpeed = 0f };



            //Convert to native array and log some metrics
            for (int i = 0; i < population.Count; i++)
            {
                //Size Metric

                EntityMetric entityMetric = new EntityMetric { };

                bool successBuffer = _intBufferLookup.TryGetBuffer(population[i].Key, out DynamicBuffer<TraitBufferComponent<int>> intBuffer);
                if (successBuffer) 
                {
                    for (int j = 0; j < intBuffer.Length; j++)
                    {
                        if (intBuffer[j].traitType == TraitType.size) 
                        {
                            sizeMetric.totalSize += intBuffer[j].value;
                            sizeMetric.totalCount++;

                            entityMetric.size = intBuffer[j].value;

                            if (i == 0) 
                            {
                                sizeMetric.topSize = intBuffer[j].value;
                            }
                            else if (i == population.Count - 1) 
                            {
                                sizeMetric.worstSize = intBuffer[j].value;
                            }
                            break;
                        }
                    }
                }

                //Speed Metric

                successBuffer = _floatBufferLookup.TryGetBuffer(population[i].Key, out DynamicBuffer<TraitBufferComponent<float>> floatBuffer);
                if (successBuffer)
                {
                    for (int j = 0; j < floatBuffer.Length; j++)
                    {
                        if (floatBuffer[j].traitType == TraitType.speed)
                        {
                            speedMetric.totalSpeed += floatBuffer[j].value;
                            speedMetric.totalCount++;

                            entityMetric.speed = floatBuffer[j].value;

                            if (i == 0)
                            {
                                speedMetric.topSpeed = floatBuffer[j].value;
                            }
                            else if (i == population.Count - 1)
                            {
                                speedMetric.worstSpeed = floatBuffer[j].value;
                            }
                            break;
                        }
                    }
                }


                //Other metrics

                entityMetric.health = GetComponent<HealthComponent>(population[i].Key).value;
                entityMetric.energy = GetComponent<EnergyComponent>(population[i].Key).value;
                entityMetric.fitness = population[i].Value;

                LoggerSystem.Logger.LogEntityEpoch(simState.currentEpoch, entityMetric);
            }

            /*JobHandle handle = new RWSJob { pointers = pointers, population = populationStruct, ecb = ecbParallel }.Schedule(pointers.Length, pointers.Length / 100, Dependency);

            handle.Complete();


            //Mark the highest fitness
            if(populationStruct.Length > 0)
            {
                ecb.AddComponent<BestInGenComponent>(populationStruct[0].entity, new BestInGenComponent());
            }
            

            populationStruct.Dispose();

            ecb.Playback(EntityManager);
            ecb.Dispose();


            pointers.Dispose();*/
            if (ecb.IsCreated) 
            {
                ecb.Dispose();
            }
            ecb = new EntityCommandBuffer(Allocator.Persistent);

            if(population.Count> 0 ) 
            {
                if(simState.mode == SimulationMode.evolve)
                {
                    ecb.AddComponent<BestInGenComponent>(population[0].Key, new BestInGenComponent());
                }
                
                //Log average fitness
                AverageAndMaxFitnessMetric fitnessMetric = new AverageAndMaxFitnessMetric { averageFitness = totalFitness / count, maxFitness = population[0].Value, metricCount = 0 };
                LoggerSystem.Logger.LogEpoch(simState.currentEpoch, fitnessMetric);
            }

            KilledMetric killedMetric = new KilledMetric { killedThisGen = simState.killedThisGen };
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, killedMetric);
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, sizeMetric);
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, speedMetric);


            ecb.Playback(World.Unmanaged.EntityManager);
            ecb.Dispose();



            int keepCount = 0;
            Entities.WithAll<KeepComponent>().ForEach((Entity entity) =>
            {
                keepCount++;
            }).WithBurst().Run();
            
            SelectedMetric selectedMetric = new SelectedMetric { selected = keepCount };
            LoggerSystem.Logger.LogEpoch(simState.currentEpoch, selectedMetric);


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

            while (Sum(I) < P)
            {
                I++;
            }

            //Debug.Log(population[I].fitness);
            //toKeep.Add(population[I].entity);
            if (I < population.Length) 
            {
                ecb.AddComponent<KeepComponent>(index, population[I].entity);
            }
            else 
            {
                ecb.AddComponent<KeepComponent>(index, population[population.Length-1].entity);
            }
            
        }

        [BurstCompile]
        private float Sum(int I) 
        {
            float sum = 0f;
            for (int i = 0; i <= I; i++)
            {
                sum += population[i].fitness;
            }
            return sum;
        }
    }
}
