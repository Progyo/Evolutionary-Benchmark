using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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

        foreach (var (simState, ent) in SystemAPI.Query<RefRW<SimStateComponent>>().WithEntityAccess())
        {
            bool noChildren = true;
            BufferLookup<Child> lookup = GetBufferLookup<Child>();

            //https://docs.unity3d.com/Packages/com.unity.entities@0.0/manual/entity_iteration_foreach.html

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

            if (simState.ValueRO.phase == Phase.end && noChildren)
            {
                ecb.SetComponent<SimStateComponent>(ent, new SimStateComponent
                {
                    currentEpoch = simState.ValueRO.currentEpoch,
                    maxEntities = simState.ValueRO.maxEntities,
                    maxEpochs = simState.ValueRO.maxEpochs,
                    entityPrefab = simState.ValueRO.entityPrefab,
                    epochDuration = simState.ValueRO.epochDuration,
                    phase = Phase.evaluated,
                    timeElapsed = simState.ValueRO.timeElapsed
                });
            }

            break;
        }
    }
}
