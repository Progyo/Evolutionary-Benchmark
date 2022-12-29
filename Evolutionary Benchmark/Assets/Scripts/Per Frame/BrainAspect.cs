using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public readonly partial struct BrainAspect : IAspect
{
    //bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);
    public readonly Entity entity;

    private readonly TransformAspect transformAspect;
    private readonly RefRW<TargetPositionComponent> target;
    private readonly RefRO<MaxDecisionSpeedComponent> maxDecisionSpeed;
    private readonly RefRW<DecisionSpeedComponent> decisionTimeLeft;


    [BurstCompile]
    public void MoveToClosest(float deltaTime, BufferLookup<SeeBufferComponent> bufferLookup, RefRW<RandomComponent> random) 
    {
        
        float max = 10f;
        float min = -max;


        decisionTimeLeft.ValueRW.value -= deltaTime;

        if(decisionTimeLeft.ValueRO.value <= 0f) 
        {
            bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);

            if (!successful)
            {
                return;
            }

            float closestDistance = 1000f;
            float3 closestPos = transformAspect.Position;
            bool foodFound = false;

            if (!buffer.IsEmpty)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].itemType == ItemType.food && closestDistance > buffer[i].distance)
                    {
                        closestDistance = buffer[i].distance;
                        closestPos = buffer[i].position;
                        foodFound |= true;
                    }
                }
            }

            if (!foodFound) 
            {
                closestPos += new float3(random.ValueRW.value.NextFloat(min, max), 0f, random.ValueRW.value.NextFloat(min, max));
            }

            target.ValueRW.value = closestPos;

            //Reset
            decisionTimeLeft.ValueRW.value = maxDecisionSpeed.ValueRO.value * random.ValueRW.value.NextFloat(0.01f,1f);
        }

        
    }

}
