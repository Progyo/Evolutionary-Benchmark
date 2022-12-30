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
    private readonly RefRW<EnergyComponent> energy;
    private readonly RefRW<HealthComponent> health;


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


            float cost = 1f;

            //Action cost
            if(energy.ValueRO.value > 0) 
            {
                energy.ValueRW.value -= cost;
            }
            else 
            {
                health.ValueRW.value -= cost;
            }
            
        }

        
    }

}

[BurstCompile]
public struct TargetTest
{
    private RefRW<TargetPositionComponent> _target;

    [BurstCompile]
    public void SetTarget(RefRW<TargetPositionComponent> target) 
    {
        _target = target;
    }


    [BurstCompile]
    public void Move() 
    {
        _target.ValueRW.value = new float3(1000f, 0f, 0f);
    }
}



[BurstCompile]
public struct BrainTest :  IBrain
{
    //bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);
    public  Entity entity;


    private LocalToWorldTransform _transform;
    private RefRW<TargetPositionComponent> _target;
    private MaxDecisionSpeedComponent _maxDecisionSpeed;
    private DecisionSpeedComponent _decisionTimeLeft;
    private EnergyComponent _energy;
    private HealthComponent _health;

    public LocalToWorldTransform transform { get { return _transform; } set { _transform = value; } }
    public RefRW<TargetPositionComponent> target { get { return _target; } set { _target = value; } }
    public MaxDecisionSpeedComponent maxDecisionSpeed { get { return _maxDecisionSpeed; } set { _maxDecisionSpeed = value; } }
    public DecisionSpeedComponent decisionTimeLeft { get { return _decisionTimeLeft; } set { _decisionTimeLeft = value; } }
    public EnergyComponent energy { get { return _energy; } set { _energy = value; } }
    public HealthComponent health { get { return _health; } set { _health = value; } }




    [BurstCompile]
    public void See(float deltaTime, BufferLookup<SeeBufferComponent> bufferLookup, RefRW<RandomComponent> random)
    {

        float max = 10f;
        float min = -max;


        _decisionTimeLeft.value -= deltaTime;

        if (decisionTimeLeft.value <= 0f)
        {
            bool successful = bufferLookup.TryGetBuffer(entity, out DynamicBuffer<SeeBufferComponent> buffer);

            if (!successful)
            {
                return;
            }

            float closestDistance = 1000f;
            float3 closestPos = _transform.Value.Position;
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

            _target.ValueRW.value = new float3(100f,0f,0f);//closestPos;

            //Reset
            _decisionTimeLeft.value = maxDecisionSpeed.value * random.ValueRW.value.NextFloat(0.01f, 1f);


            float cost = 1f;

            //Action cost
            if (energy.value > 0)
            {
                _energy.value -= cost;
            }
            else
            {
                _health.value -= cost;
            }

        }


    }

}