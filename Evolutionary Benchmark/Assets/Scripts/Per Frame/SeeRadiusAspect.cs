using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;


[assembly: RegisterGenericComponentType(typeof(EntityTypeComponent))]

[BurstCompile]
public readonly partial struct SeeRadiusAspect : IAspect
{
    public readonly Entity entity;
    private readonly TransformAspect transformAspect;
    private readonly RefRO<EyeRadiusComponent> seeRadius;


    //Entity command buffer to add component
    //Watch turbo makes games video again
    //https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/systems-entityquery-create.html
    //https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/components-buffer-command-buffer.html
    /*
    public void CheckDistances(NativeArray<Entity> entities, BufferLookup<SeeBufferComponent> bufferLookup) 
    {
        for(int i =0; i< entities.Length; i++) 
        {
            if (entities[i] != entity) 
            {
                float3 pos = SystemAPI.GetComponent<LocalToWorldTransform>(entities[i]).Value.Position;
                bufferLookup.Update()
            }
            
        }
    }*/

    [BurstCompile]
    public void UpdateView(NativeArray<LocalToWorldTransform> transforms, NativeArray<EntityTypeComponent> types, BufferLookup<SeeBufferComponent> bufferLookup, NativeArray<Entity> entities)
    {
        
        bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);
        
        if (!successful) 
        {
            return;
        }

        buffer.Clear();

        for (int i = 0; i < transforms.Length; i++)
        {
            float3 pos =transforms[i].Value.Position;//SystemAPI.GetComponent<LocalToWorldTransform>(entities[i]).Value.Position;
            float distance = math.distance(transformAspect.Position, pos);

            if(distance == 0) 
            {
                continue;
            }

            if(distance < seeRadius.ValueRO.value) 
            {
                EntityType type = types[i].value;
                ItemType itemType = getType(type);
                buffer.Add(new SeeBufferComponent { distance = distance, itemType = itemType, position = pos, entity= entities[i] });
            }


            
        }
    }

    [BurstCompile]
    private ItemType getType(EntityType entityType) 
    {
        if(entityType == EntityType.blob) 
        {
            return ItemType.enemy;
        }
        else if (entityType == EntityType.food) 
        {
            return ItemType.food;
        }


        return ItemType.enemy;
    }
}
