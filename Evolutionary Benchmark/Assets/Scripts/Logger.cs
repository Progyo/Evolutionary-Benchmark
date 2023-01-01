using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Collections;
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
[UpdateAfter(typeof(ConsumeSystem))]
public partial class LoggerSystem : SystemBase
{
    string json = "";
    protected override void OnUpdate()
    {
        //RequireForUpdate<SimStateComponent>();
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (simState, ent) in SystemAPI.Query<RefRW<SimStateComponent>>().WithEntityAccess()) 
        {
            
            if (simState.ValueRO.phase == Phase.end)
            {

                //Loop through all int metrics
                foreach (var (metric, entity) in SystemAPI.Query<RefRO<MetricComponent<int>>>().WithNone<DestroyComponent>().WithEntityAccess())
                {
                    json += string.Format("Time: {0} Epoch: {1} Value: {2}", metric.ValueRO.timeStamp, metric.ValueRO.epoch, metric.ValueRO.value) + "\n";
                    Debug.Log(json);
                    ecb.RemoveComponent(entity, typeof(MetricComponent<int>));
                    ecb.AddComponent<DestroyComponent>(entity);
                }


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










}