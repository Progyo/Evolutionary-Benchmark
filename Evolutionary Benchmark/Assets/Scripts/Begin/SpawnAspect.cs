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

/// <summary>
/// Spawn aspect that allows the IEntityJob to loop through all the spawners.
/// </summary>
[BurstCompile]
public readonly partial struct SpawnAspect : IAspect
{

    private readonly RefRW<LocalToWorldTransform> transformRef;
    private readonly RefRW<SpawnPointComponent> spawnPoint;


    /// <summary>
    /// This function spawns entities at the spawner location within the given boundaries
    /// </summary>
    /// <param name="ecb"> The entity command buffer which allows the entities to be spawned </param>
    /// /// <param name="ecb"> The entity command buffer which changes entity components</param>
    /// <param name="sortKey"> The sort key (needs to be unique to this thread) </param>
    /// <param name="newSpawnCount"> The number of entities that need to be spawned new for this spawner</param>
    /// <param name="toKeepSpawn"> The number of entities that need to be generated (respawned) for this spawner</param>
    /// <param name="foodSpawnCount"> The number of food entities to spawn for this spawner</param>
    /// <param name="toKeep"> Entity array of all the entities from the previous generation to keep (Only the values need to be reset and a new position assigned)</param>
    /// <param name="toKeepMaxHealth"> Reference to the entities max health</param>
    /// <param name="toKeepMaxEnergy"> Reference to the entities max energy</param>
    /// <param name="intBufferLookup"> Integer trait lookup </param>
    /// <param name="floatBufferLookup"> Float trait lookup </param>
    [BurstCompile]
    public void SpawnEntity(EntityCommandBuffer.ParallelWriter ecb, EntityCommandBuffer.ParallelWriter ecb2, int sortKey, int newSpawnCount, int maxToSpawnCount, int toKeepSpawn,NativeArray<Entity> toKeep, /*NativeArray<RefRef<TransformAspect>> toKeepTransforms,
        NativeArray<RefRef<HealthComponent>> toKeepHealth, NativeArray<RefRef<EnergyComponent>> toKeepEnergy, */NativeArray<RefRef<MaxHealthComponent>> toKeepMaxHealth, NativeArray<RefRef<MaxEnergyComponent>> toKeepMaxEnergy,
        BufferLookup<TraitBufferComponent<int>> intBufferLookup, BufferLookup<TraitBufferComponent<float>> floatBufferLookup, int epoch) 
    {
        //Define boundaries as min and max points
        float maxX = spawnPoint.ValueRO.boundary.z;
        float maxY = spawnPoint.ValueRO.boundary.w;
        float minX = spawnPoint.ValueRO.boundary.x;
        float minY = spawnPoint.ValueRO.boundary.y;

        //The way this is currently done, it only allows for one type of moving entity

        //If the type is blob then iterrate
        if (spawnPoint.ValueRO.type == EntityType.blob)
        {



            for (int i = 0; i < newSpawnCount; i++)
            {
                //Don't accidentally spawn too many
                if(sortKey * newSpawnCount + i >= maxToSpawnCount) 
                { break; }
                //Set position to some random position in boundaries
                float3 pos =  transformRef.ValueRO.Value.Position + new float3(spawnPoint.ValueRW.random.NextFloat(minX,maxX), 0f, spawnPoint.ValueRW.random.NextFloat(minY, maxY));

                //Create the entity creation command
                Entity e = ecb.Instantiate(sortKey, spawnPoint.ValueRO.prefab);


                //int defaultSize = 10;// +epoch*5;

                //Set the transform
                UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = transformRef.ValueRO.Value.Scale};
                ecb.SetComponent<LocalToWorldTransform>(sortKey, e, new LocalToWorldTransform { Value = transform });
            
                //Set the boundary and target position 
                float4 boundary = new float4(transformRef.ValueRO.Value.Position.x, transformRef.ValueRO.Value.Position.z, transformRef.ValueRO.Value.Position.x, transformRef.ValueRO.Value.Position.z) + spawnPoint.ValueRO.boundary;
                ecb.SetComponent<TargetPositionComponent>(sortKey, e, new TargetPositionComponent { value = pos, boundary = boundary });
            
            
            }

            //Loop through all the ones to keep
            for (int i = 0; i < toKeepSpawn; i++)
            {
                int index = sortKey * toKeepSpawn + i; //math.min(sortKey * toKeepSpawn + i, toKeep.Length-1);

                if (index >= toKeep.Length) 
                {
                    break;
                }

                Entity e = toKeep[index];
                float3 pos = transformRef.ValueRO.Value.Position + new float3(spawnPoint.ValueRW.random.NextFloat(minX, maxX), 0f, spawnPoint.ValueRW.random.NextFloat(minY, maxY));

                //Set the transform
                UniformScaleTransform transform = new UniformScaleTransform { Position = pos, Rotation = quaternion.identity, Scale = transformRef.ValueRO.Value.Scale };
                ecb.SetComponent<LocalToWorldTransform>(index, e, new LocalToWorldTransform { Value = transform });

                //Set health and energy components back to max
                ecb2.SetComponent<HealthComponent>(index, e, new HealthComponent { value = toKeepMaxHealth[index].ValueRO.value });
                ecb2.SetComponent<EnergyComponent>(index, e, new EnergyComponent { value = toKeepMaxEnergy[index].ValueRO.value });

                //Set the boundary and target position 
                float4 boundary = new float4(transformRef.ValueRO.Value.Position.x, transformRef.ValueRO.Value.Position.z, transformRef.ValueRO.Value.Position.x, transformRef.ValueRO.Value.Position.z) + spawnPoint.ValueRO.boundary;
                ecb2.SetComponent<TargetPositionComponent>(sortKey, e, new TargetPositionComponent { value = pos, boundary = boundary });
                
            }
        }

    }

}

/// <summary>
/// Spawn aspect that allows the IEntityJob to loop through all the spawners.
/// </summary>
[BurstCompile]
public readonly partial struct FoodSpawnAspect : IAspect
{

    private readonly TransformAspect transformAspect;
    private readonly RefRW<FoodSpawnPointComponent> spawnPoint;


    /// <summary>
    /// This function spawns entities at the spawner location within the given boundaries
    /// </summary>
    /// <param name="ecb"> The entity command buffer which allows the entities to be spawned </param>
    /// <param name="sortKey"> The sort key (needs to be unique to this thread) </param>
    /// <param name="foodSpawnCount"> The number of food entities to spawn for this spawner</param>

    [BurstCompile]
    public void SpawnEntity(EntityCommandBuffer.ParallelWriter ecb, int sortKey, int foodSpawnCount)
    {
        //Define boundaries as min and max points
        float maxX = spawnPoint.ValueRO.boundary.z;
        float maxY = spawnPoint.ValueRO.boundary.w;
        float minX = spawnPoint.ValueRO.boundary.x;
        float minY = spawnPoint.ValueRO.boundary.y;

        //The way this is currently done, it only allows for one type of moving entity
        if (spawnPoint.ValueRO.type == EntityType.food)
        {
            //Spawn the found within the boundary
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
