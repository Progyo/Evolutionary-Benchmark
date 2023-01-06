using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public readonly partial struct MoveAspect : IAspect
{
    private readonly Entity entity;
    private readonly TransformAspect transformAspect;
    //private readonly RefRO<SpeedComponent> speed;
    //private readonly RefRO<SizeComponent> size;
    private readonly RefRW<TargetPositionComponent> targetPosition;

    [BurstCompile]
    public void Move(float deltaTime, BufferLookup<TraitBufferComponent<float>> floatTraitbufferLookup, BufferLookup<TraitBufferComponent<int>> intTraitbufferLookup, RefRW<RandomComponent> random) 
    {




        float speed = 0f;

        bool successful = floatTraitbufferLookup.TryGetBuffer(entity, out DynamicBuffer<TraitBufferComponent<float>> floatbuffer);

        if (successful) 
        {
            for (int i = 0; i < floatbuffer.Length; i++)
            {
                if (floatbuffer[i].traitType == TraitType.speed) 
                {
                    speed = floatbuffer[i].value;
                    break;
                }
            }        

        }


        int size = 1;

        successful = intTraitbufferLookup.TryGetBuffer(entity, out DynamicBuffer<TraitBufferComponent<int>> intbuffer);

        if (successful)
        {
            for (int i = 0; i < intbuffer.Length; i++)
            {
                if (intbuffer[i].traitType == TraitType.size)
                {
                    size = intbuffer[i].value;
                    break;
                }
            }

        }

        float3 dir = targetPosition.ValueRO.value - transformAspect.Position;
        float3 dirNormal = math.normalize(dir);


        if (math.lengthsq(dir) > 0.05f) 
        {
            transformAspect.Position += dirNormal * deltaTime * speed / size;
            
        }


        float maxX = targetPosition.ValueRO.boundary.z;
        float maxY = targetPosition.ValueRO.boundary.w;
        float minX = targetPosition.ValueRO.boundary.x;
        float minY = targetPosition.ValueRO.boundary.y;

        if (transformAspect.Position.x > maxX || transformAspect.Position.x < minX || transformAspect.Position.z > maxY || transformAspect.Position.z < minY) 
        {
            transformAspect.Position = new float3(random.ValueRW.value.NextFloat(minX,maxX), 0f, random.ValueRW.value.NextFloat(minY, maxY));
        }

    }

    [BurstCompile]
    public void Rotate()
    {
        transformAspect.LookAt(targetPosition.ValueRO.value);

    }
}
