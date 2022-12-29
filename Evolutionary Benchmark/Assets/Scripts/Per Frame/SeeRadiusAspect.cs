using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

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

}
