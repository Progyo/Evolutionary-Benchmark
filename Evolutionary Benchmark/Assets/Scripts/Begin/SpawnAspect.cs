using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public readonly partial struct SpawnAspect : IAspect
{

    private readonly TransformAspect transformAspect;
    private readonly RefRW<SpawnPointComponent> spawnPoint;


    [BurstCompile]
    public void SpawnEntity(Entity entity, EntityCommandBuffer.ParallelWriter ecb, int sortKey, int count) 
    {

        
        float max = 100f;
        float min = -max;

        for (int i = 0; i < count; i++)
        {
            float3 pos =  transformAspect.Position + new float3(spawnPoint.ValueRW.random.NextFloat(min,max), 0f, spawnPoint.ValueRW.random.NextFloat(min, max));
            UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = 1f };
            Entity e = ecb.Instantiate(sortKey, entity);
            ecb.SetComponent<SpeedComponent>(sortKey, e, new SpeedComponent { value = spawnPoint.ValueRW.random.NextFloat(1f, 20f) });
            ecb.SetComponent<LocalToWorldTransform>(sortKey, e, new LocalToWorldTransform { Value = transform });
        }


    }

}
