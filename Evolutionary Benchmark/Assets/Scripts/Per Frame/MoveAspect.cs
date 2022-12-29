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
    private readonly RefRO<SpeedComponent> speed;
    private readonly RefRO<SizeComponent> size;
    private readonly RefRW<TargetPositionComponent> targetPosition;

    [BurstCompile]
    public void Move(float deltaTime) 
    {
        float3 dir = math.normalize(targetPosition.ValueRO.value - transformAspect.Position);

        if(math.lengthsq(dir) > 0.05f) 
        {
            transformAspect.Position += dir * deltaTime * speed.ValueRO.value / size.ValueRO.value;
            
        }

    }

    [BurstCompile]
    public void Rotate()
    {
        transformAspect.LookAt(targetPosition.ValueRO.value);

    }
}
