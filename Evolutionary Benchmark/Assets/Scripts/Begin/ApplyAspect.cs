using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public readonly partial struct ApplyAspect : IAspect
{
    public readonly Entity entity;

    private readonly RefRW<EntityTypeComponent> entityType;
    private readonly TransformAspect transformAspect;

    public void SetTraits(BufferLookup<TraitBufferComponent<int>> intTrait, BufferLookup<TraitBufferComponent<float>> floatTrait) 
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
            }
        }
    }
}
