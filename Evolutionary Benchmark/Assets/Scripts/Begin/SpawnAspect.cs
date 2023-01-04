using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public readonly partial struct SpawnAspect : IAspect
{

    private readonly TransformAspect transformAspect;
    private readonly RefRW<SpawnPointComponent> spawnPoint;


    [BurstCompile]
    public void SpawnEntity(EntityCommandBuffer.ParallelWriter ecb, int sortKey, int newSpawnCount, int toKeepSpawn, int foodSpawnCount,NativeArray<Entity> toKeep, /*NativeArray<RefRef<TransformAspect>> toKeepTransforms,
        NativeArray<RefRef<HealthComponent>> toKeepHealth, NativeArray<RefRef<EnergyComponent>> toKeepEnergy, */NativeArray<RefRef<MaxHealthComponent>> toKeepMaxHealth, NativeArray<RefRef<MaxEnergyComponent>> toKeepMaxEnergy,
        BufferLookup<TraitBufferComponent<int>> intBufferLookup, BufferLookup<TraitBufferComponent<float>> floatBufferLookup) 
    {

        float maxX = spawnPoint.ValueRO.boundary.z;
        float maxY = spawnPoint.ValueRO.boundary.w;
        float minX = spawnPoint.ValueRO.boundary.x;
        float minY = spawnPoint.ValueRO.boundary.y;



        if (spawnPoint.ValueRO.type == EntityType.blob)
        {



            for (int i = 0; i < newSpawnCount; i++)
            {
                float3 pos =  transformAspect.Position + new float3(spawnPoint.ValueRW.random.NextFloat(minX,maxX), 0f, spawnPoint.ValueRW.random.NextFloat(minY, maxY));

                Entity e = ecb.Instantiate(sortKey, spawnPoint.ValueRO.prefab);


                /*bool success = intBufferLookup.TryGetBuffer(e, out DynamicBuffer<TraitBufferComponent<int>> intBuffer);

                bool success2 = floatBufferLookup.TryGetBuffer(e, out DynamicBuffer<TraitBufferComponent<float>> floatBuffer);

                //Destroy entity if not trait buffer
                if (!success || !success2) 
                {
                    ecb.AddComponent<DestroyComponent>((sortKey + 1) * toKeepSpawn + i, e);
                    continue;
                }

                int size = 1;
                for (int j = 0; j < intBuffer.Length; j++)
                {
                    if(intBuffer.ElementAt(j).traitType == TraitType.size) 
                    {
                        size = intBuffer.ElementAt(j).value;
                        break;
                    }
                }*/
                int size = 10;


                UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = size/10f };
                //ecb.SetComponent<SpeedComponent>(sortKey, e, new SpeedComponent { value = spawnPoint.ValueRW.random.NextFloat(1f, 20f) });
                ecb.SetComponent<LocalToWorldTransform>(sortKey, e, new LocalToWorldTransform { Value = transform });
            

                float4 boundary = new float4(transformAspect.Position.x, transformAspect.Position.z, transformAspect.Position.x, transformAspect.Position.z) + spawnPoint.ValueRO.boundary;
                ecb.SetComponent<TargetPositionComponent>(sortKey, e, new TargetPositionComponent { value = pos, boundary = boundary });
            
            
            }

            for (int i = 0; i < toKeepSpawn; i++)
            {
                int index = math.min(sortKey * toKeepSpawn + i, toKeep.Length-1);
                Entity e = toKeep[index];
                float3 pos = transformAspect.Position + new float3(spawnPoint.ValueRW.random.NextFloat(minX, maxX), 0f, spawnPoint.ValueRW.random.NextFloat(minY, maxY));


                bool success = intBufferLookup.TryGetBuffer(e, out DynamicBuffer<TraitBufferComponent<int>> intBuffer);

                bool success2 = floatBufferLookup.TryGetBuffer(e, out DynamicBuffer<TraitBufferComponent<float>> floatBuffer);

                //Destroy entity if not trait buffer
                if (!success || !success2)
                {
                    ecb.AddComponent<DestroyComponent>((sortKey + 1) * toKeepSpawn + i, e);
                    //throw new NotImplementedException();
                    continue;
                }

                int size = 1;
                for (int j = 0; j < intBuffer.Length; j++)
                {
                    if (intBuffer.ElementAt(j).traitType == TraitType.size)
                    {
                        size = intBuffer.ElementAt(j).value;
                        break;
                    }
                }



                UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = size/10f };
                ecb.SetComponent<LocalToWorldTransform>(index, e, new LocalToWorldTransform { Value = transform });
                ecb.SetComponent<HealthComponent>(index, e, new HealthComponent { value = toKeepMaxHealth[index].ValueRO.value });
                ecb.SetComponent<EnergyComponent>(index, e, new EnergyComponent { value = toKeepMaxEnergy[index].ValueRO.value });


                float4 boundary = new float4(transformAspect.Position.x, transformAspect.Position.z, transformAspect.Position.x, transformAspect.Position.z) + spawnPoint.ValueRO.boundary;
                ecb.SetComponent<TargetPositionComponent>(sortKey, e, new TargetPositionComponent { value = pos, boundary = boundary });
                
            }
        }
        else if (spawnPoint.ValueRO.type == EntityType.food)
        {
            for (int i = 0; i < foodSpawnCount; i++)
            {
                float3 pos = transformAspect.Position + new float3(spawnPoint.ValueRW.random.NextFloat(minX, maxX), 0f, spawnPoint.ValueRW.random.NextFloat(minY, maxY));
                UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = 1f };
                Entity e = ecb.Instantiate(sortKey, spawnPoint.ValueRO.prefab);
                ecb.SetComponent<LocalToWorldTransform>(sortKey, e, new LocalToWorldTransform { Value = transform });
            }
        }


    }

}
