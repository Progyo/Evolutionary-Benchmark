using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[UpdateAfter(typeof(LoggerSystem))]
public partial class DestroyerSystem : SystemBase
{

    EntityQuery query;

    protected override void OnCreate()
    {
        query = SystemAPI.QueryBuilder().WithAll<DestroyComponent>().Build();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
        EntityCommandBuffer ecb2 = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (simState, ent) in SystemAPI.Query<RefRW<SimStateComponent>>().WithEntityAccess())
        {
            bool noChildren = true;
            BufferLookup<Child> lookup = GetBufferLookup<Child>();

            //https://docs.unity3d.com/Packages/com.unity.entities@0.0/manual/entity_iteration_foreach.html


            //Remove this later
            /*if(simState.ValueRO.phase == Phase.end) 
            {
                Entities.WithAll<EntityTypeComponent>().ForEach((Entity entity) =>
                {

                    ecb.AddComponent<DestroyComponent>(entity);
                    noChildren = false;

                }).Run();
                
            }
            */

            //Destroy all entities with 0 health
            int killed = 0;

            if(simState.ValueRO.phase == Phase.running) 
            {
                Entities.WithAll<HealthComponent>().WithNone<DestroyComponent>().ForEach((Entity entity, in HealthComponent health) =>
                {
                    if (health.value <= 0f)
                    {
                        ecb2.AddComponent<DestroyComponent>(entity);

                        killed++;
                    }
                }).Run();

                if (killed > 0)
                {
                    ecb.SetComponent<SimStateComponent>(ent, new SimStateComponent
                    {
                        currentEpoch = simState.ValueRO.currentEpoch,
                        maxEntities = simState.ValueRO.maxEntities,
                        maxEpochs = simState.ValueRO.maxEpochs,
                        entityPrefab = simState.ValueRO.entityPrefab,
                        epochDuration = simState.ValueRO.epochDuration,
                        phase = simState.ValueRO.phase,
                        timeElapsed = simState.ValueRO.timeElapsed,
                        fields = simState.ValueRO.fields,
                        killedThisGen = simState.ValueRO.killedThisGen + killed,
                        survivePercent = simState.ValueRO.survivePercent,
                        fieldEntityPrefab = simState.ValueRO.fieldEntityPrefab,
                        mode = simState.ValueRO.mode,
                    });
                }
            }

            if (simState.ValueRO.phase == Phase.end) 
            {
                
                Entities.WithAll<EntityTypeComponent>().WithNone<KeepComponent>().ForEach((Entity entity) =>
                {
                    ecb2.AddComponent<DestroyComponent>(entity);
                }).Run();

                /*Entities.WithAll<KeepComponent>().ForEach((Entity entity) =>
                {
                    ecb2.RemoveComponent<KeepComponent>(entity);
                }).Run();
                */

                ecb.SetComponent<SimStateComponent>(ent, new SimStateComponent
                {
                    currentEpoch = simState.ValueRO.currentEpoch,
                    maxEntities = simState.ValueRO.maxEntities,
                    maxEpochs = simState.ValueRO.maxEpochs,
                    entityPrefab = simState.ValueRO.entityPrefab,
                    epochDuration = simState.ValueRO.epochDuration,
                    phase = Phase.deleting,
                    timeElapsed = simState.ValueRO.timeElapsed,
                    fields = simState.ValueRO.fields,
                    killedThisGen = simState.ValueRO.killedThisGen,
                    survivePercent = simState.ValueRO.survivePercent,
                    fieldEntityPrefab = simState.ValueRO.fieldEntityPrefab,
                    mode = simState.ValueRO.mode,
                });

            }




            Entities.WithAll<DestroyComponent>().ForEach((Entity entity) =>
            {
                bool success = lookup.TryGetBuffer(entity, out DynamicBuffer<Child> buffer);

                if (success && buffer.Length > 0)
                {
                    noChildren = false;

                    for (int i = 0; i < buffer.Length; i++)
                    {
                        ecb.AddComponent<DestroyComponent>(buffer[i].Value);
                    }

                }

                ecb.DestroyEntity(entity);
            }).Run();

            if (simState.ValueRO.phase == Phase.deleting && noChildren)
            {
                ecb.SetComponent<SimStateComponent>(ent, new SimStateComponent
                {
                    currentEpoch = simState.ValueRO.currentEpoch,
                    maxEntities = simState.ValueRO.maxEntities,
                    maxEpochs = simState.ValueRO.maxEpochs,
                    entityPrefab = simState.ValueRO.entityPrefab,
                    epochDuration = simState.ValueRO.epochDuration,
                    phase = Phase.evaluated,
                    timeElapsed = simState.ValueRO.timeElapsed,
                    fields = simState.ValueRO.fields,
                    killedThisGen = simState.ValueRO.killedThisGen,
                    survivePercent = simState.ValueRO.survivePercent,
                    fieldEntityPrefab = simState.ValueRO.fieldEntityPrefab,
                    mode = simState.ValueRO.mode,
                });
            }

            break;
        }

        ecb2.Playback(EntityManager);
        ecb2.Dispose();
    }
}
