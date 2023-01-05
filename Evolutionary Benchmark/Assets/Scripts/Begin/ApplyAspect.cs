using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using System.Drawing;

[BurstCompile]
public readonly partial struct ApplyAspect : IAspect
{
    public readonly Entity entity;

    private readonly RefRW<EntityTypeComponent> entityType;
    private readonly RefRW<LocalToWorldTransform> transform;


    [BurstCompile]
    public void SetTraits(BufferLookup<TraitBufferComponent<int>> intTrait, BufferLookup<TraitBufferComponent<float>> floatTrait, EntityCommandBuffer.ParallelWriter ecb, int sortKey) 
    {
        if(entityType.ValueRO.value == EntityType.blob) 
        {
            bool success = intTrait.TryGetBuffer(entity, out DynamicBuffer<TraitBufferComponent<int>> intBuffer);

            if (success) 
            {

                int size = 1;

                for (int i = 0; i < intBuffer.Length; i++)
                {
                    if(intBuffer.ElementAt(i).traitType == TraitType.size) 
                    {
                        size = intBuffer.ElementAt(i).value;
                        break;
                    }
                }

                //https://github.com/needle-mirror/com.unity.entities.git
                //transformAspect.localsc

                //https://answers.unity.com/questions/1791874/how-to-change-scale-an-entiy-ecs-question.html
                UniformScaleTransform uniTransform = new UniformScaleTransform { Position = transform.ValueRO.Value.Position, Rotation = transform.ValueRO.Value.Rotation, Scale = size / 10f };
                ecb.SetComponent<LocalToWorldTransform>(sortKey, entity, new LocalToWorldTransform { Value = uniTransform});
            }
        }

    }
}
