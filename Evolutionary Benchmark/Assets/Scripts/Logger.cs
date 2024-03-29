using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.IO;
/*
[BurstCompile]
public static class Logger
{
//[BurstCompile]
public static void Log() 
{
Debug.Log("Test");
}

[BurstCompile]
public static void LogOnce(Metric metricJson) 
{
Debug.Log(metricJson);
}

[BurstCompile]
public static void LogTimeAndEpoch(float time, int epoch, string metricJson) 
{
Debug.Log(metricJson);//string.Format("Time: {0} Epoch: {1} Metric: {2}", time, epoch, metricJson)
}

[BurstCompile]
public static void LogEpoch(int epoch, string metricJson) 
{
//  Debug.Log(string.Format("Epoch: {0} Metric: {1}", epoch, metricJson));
}

[BurstCompile]
public static string SaveJson() 
{
return "";
}
}*/
[UpdateAfter(typeof(CombinationSystem))]
public partial class LoggerSystem : SystemBase
{
    //string json = "";
    protected override void OnUpdate()
    {
        //RequireForUpdate<SimStateComponent>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (simState, ent) in SystemAPI.Query<RefRW<SimStateComponent>>().WithEntityAccess()) 
        {
            bool fitness = false;
            //AverageAndMaxFitnessMetric fitnessMetric = new FitnessMetric {averageFitness = 0f, metricCount =0, maxFitness =0f };

            if (simState.ValueRO.phase == Phase.end)
            {
                int mutations = 0;
                //Loop through all int metrics
                foreach (var (metric, entity) in SystemAPI.Query<RefRO<MetricComponent<int>>>().WithNone<DestroyComponent>().WithEntityAccess())
                {

                    if(metric.ValueRO.type == MetricType.mutation) 
                    {
                        mutations++;
                    }

                    //json += string.Format("Time: {0} Epoch: {1} Value: {2}", metric.ValueRO.timeStamp, metric.ValueRO.epoch, metric.ValueRO.value) + "\n";
                    //Debug.Log(json);
                    ecb.RemoveComponent(entity, typeof(MetricComponent<int>));
                    ecb.AddComponent<DestroyComponent>(entity);
                }

                foreach (var (metric, entity) in SystemAPI.Query<RefRO<MetricComponent<float>>>().WithNone<DestroyComponent>().WithEntityAccess())
                {
                    if (metric.ValueRO.type == MetricType.fitness)
                    {
                        fitness = true;
                        /*if (metric.ValueRO.value > fitnessMetric.maxFitness) 
                        {
                            fitnessMetric.maxFitness = metric.ValueRO.value;
                        }

                        fitnessMetric.averageFitness += metric.ValueRO.value;
                        fitnessMetric.metricCount++;*/
                    }
                    else 
                    {
                        //json += string.Format("Time: {0} Epoch: {1} Value: {2}", metric.ValueRO.timeStamp, metric.ValueRO.epoch, metric.ValueRO.value) + "\n";
                    }
                    
                    
                    ecb.RemoveComponent(entity, typeof(MetricComponent<float>));
                    ecb.AddComponent<DestroyComponent>(entity);
                }
                //fitnessMetric.averageFitness /= math.max(fitnessMetric.metricCount, 1);


                if (fitness) 
                {
                    //Logger.LogTimeAndEpoch(simState.ValueRO.timeElapsed, simState.ValueRO.currentEpoch, fitnessMetric);
                    //Logger.LogEpoch(simState.ValueRO.currentEpoch, fitnessMetric);
                }
                //Debug.Log(mutations);
                //Debug.Log(Logger.SaveJson());
                Logger.SaveJsonIncrement();
                //json += string.Format("Time: {0} Epoch: {1} Average Fitness: {2} Max Fitness: {3}", simState.ValueRO.timeElapsed, simState.ValueRO.currentEpoch, averageFitness, highestFitness) + "\n";

                //Debug.Log(json);
                //json = "";
                
                //Replace this later

                //simState.ValueRW.phase = Phase.evaluated;
                /*ecb.SetComponent<SimStateComponent>(ent, new SimStateComponent
                {
                    currentEpoch = simState.ValueRO.currentEpoch,
                    maxEntities = simState.ValueRO.maxEntities,
                    maxEpochs = simState.ValueRO.maxEpochs,
                    entityPrefab = simState.ValueRO.entityPrefab,
                    epochDuration = simState.ValueRO.epochDuration,
                    phase = Phase.evaluated,
                    timeElapsed = simState.ValueRO.timeElapsed
                });*/

            }
            break;
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();


    }




    public static class Logger
    {
        //Internally the time -1 is used for all other metrics that are on the same level in the heirarchy as time
        //Internally the time -2 is used for all entity metrics that are on the same level in the heirarchy as time


        /// <summary>
        /// Epoch: {Time: {json}, "Other Metrics": }
        /// </summary>
        private static Dictionary<int, Dictionary<float, List<string>>> cache = new Dictionary<int, Dictionary<float, List<string>>>();

        /// <summary>
        /// The last epoch that was saved
        /// </summary>
        private static int lastEpoch = 0;

        public static void LogOnce(IMetric metric)
        {
            
        }

        public static void LogTimeAndEpoch(float time, int epoch, IMetric metric)
        {
            if (!cache.ContainsKey(epoch)) 
            {
                cache.Add(epoch, new Dictionary<float, List<string>>());
            }

            if (!cache[epoch].ContainsKey(time)) 
            {
                cache[epoch].Add(time, new List<string>());
            }

            cache[epoch][time].Add(metric.ToJsonString());
        }

        public static void LogEpoch(int epoch, IMetric metric)
        {
            LogTimeAndEpoch(-1f, epoch, metric);
        }

        public static void LogEntityEpoch(int epoch, IMetric metric)
        {
            LogTimeAndEpoch(-2f, epoch, metric);
        }


        private static string SaveJson() 
        {

            string json = "";

            int outsideCount = 0;

            foreach (KeyValuePair<int, Dictionary<float, List<string>>> pair in cache)
            {
                //Skip initial
                if (pair.Key <= lastEpoch)
                {
                    continue;
                }

                json += "\"" + pair.Key + "\": {\n";

                int insideCount = 0;
                if (pair.Value.ContainsKey(-1f))
                {
                    foreach (string str in pair.Value[-1f])
                    {
                        insideCount++;
                        if (insideCount < pair.Value[-1f].Count || pair.Value.ContainsKey(-2f))
                        {
                            json += str + ",\n";
                        }
                        else
                        {
                            json += str + "\n";
                        }

                    }
                }
                insideCount = 0;
                if (pair.Value.ContainsKey(-2f))
                {

                    json += "\"epoch\": [";
                    foreach (string str in pair.Value[-2f])
                    {

                        insideCount++;
                        if (insideCount < pair.Value[-2f].Count)
                        {
                            json += str + ",\n";
                        }
                        else
                        {
                            json += str + "\n";
                        }

                    }

                    //This might need to be changed to have a ], in some cases
                    json += "]";
                }

                insideCount = 0;
                foreach (KeyValuePair<float, List<string>> pair2 in pair.Value)
                {
                    if (pair2.Key != -1f && pair2.Key != -2f)
                    {
                        json += pair2.Key + ": [\n";

                        foreach (string str in pair2.Value)
                        {
                            json += str + "\n";
                        }
                        insideCount++;

                        if (insideCount < pair.Value.Count)
                        {
                            json += "],\n";
                        }
                        else
                        {
                            json += "]\n";
                        }

                    }

                }
                outsideCount++;

                if (outsideCount < cache.Count)
                {
                    json += "},\n";
                }
                else
                {
                    json += "}\n";
                }

                lastEpoch = pair.Key;

            }

            

            //File.WriteAllText("evolution_benchmark_data.json", json);

            return json;
        }

        public static string SaveJsonWhole()
        {
            string json = "{\n";

            lastEpoch = 0;

            json += SaveJson();

            json += "}\n";


            File.WriteAllText("evolution_benchmark_data.json", json);

            return json;
        }


        public static void SaveJsonIncrement() 
        {
            string newJson = SaveJson();


            if (!File.Exists("evolution_benchmark_data.json")) 
            {
                File.WriteAllText("evolution_benchmark_data.json", "{\n");
            }

            File.AppendAllText("evolution_benchmark_data.json", newJson);

        }

    }




}