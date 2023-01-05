using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
public struct BrainTest :  IBrain
{
    private RefRW<LocalToWorldTransform> _transform;
    private RefRW<TargetPositionComponent> _target;
    private RefRW<MaxDecisionSpeedComponent> _maxDecisionSpeed;
    private RefRW<DecisionSpeedComponent> _decisionTimeLeft;
    private RefRW<EnergyComponent> _energy;
    private RefRW<HealthComponent> _health;
    private RefRW<Entity> _entity;

    public RefRW<Entity> entity { get { return _entity; } set { _entity = value; } }
    public RefRW<LocalToWorldTransform> transform { get { return _transform; } set { _transform = value; } }
    public RefRW<TargetPositionComponent> target { get { return _target; } set { _target = value; } }
    public RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed { get { return _maxDecisionSpeed; } set { _maxDecisionSpeed = value; } }
    public RefRW<DecisionSpeedComponent> decisionTimeLeft { get { return _decisionTimeLeft; } set { _decisionTimeLeft = value; } }
    public RefRW<EnergyComponent> energy { get { return _energy; } set { _energy = value; } }
    public RefRW<HealthComponent> health { get { return _health; } set { _health = value; } }

    [BurstCompile]
    public BrainAction See(float deltaTime, BufferLookup<SeeBufferComponent> bufferLookup, RefRW<RandomComponent> random, out float3 moveTo, out Entity entityToConsume, BufferLookup<TraitBufferComponent<int>> intLookup, BufferLookup<TraitBufferComponent<float>> floatLookup)
    {

        float max = 10f;
        float min = -max;
        moveTo = transform.ValueRO.Value.Position;

        entityToConsume = _entity.ValueRO;

        bool successful = bufferLookup.TryGetBuffer(_entity.ValueRO, out DynamicBuffer<SeeBufferComponent> buffer);

        if (!successful)
        {
            
            return BrainAction.nothing;
        }

        float closestDistance = 1000f;
        float3 closestPos = _transform.ValueRO.Value.Position;
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

                    if (foodFound && closestDistance < 0.5f)
                    {
                        entityToConsume = buffer[i].entity;
                        return BrainAction.eat;
                    }
                    
                }
            }
        }

        if (!foodFound)
        {
            closestPos += new float3(random.ValueRW.value.NextFloat(min, max), 0f, random.ValueRW.value.NextFloat(min, max));
        }

        moveTo = closestPos;


        int size = 1;
        float speed = 1f;


        bool success = intLookup.TryGetBuffer(_entity.ValueRO, out DynamicBuffer<TraitBufferComponent<int>> intBuffer);

        if (success) 
        {
            for (int i = 0; i < intBuffer.Length; i++)
            {
                if (intBuffer[i].traitType == TraitType.size) 
                {
                    size = intBuffer[i].value;
                    break;
                }
            }
        }



        success = floatLookup.TryGetBuffer(_entity.ValueRO, out DynamicBuffer<TraitBufferComponent<float>> floatBuffer);

        if (success)
        {
            for (int i = 0; i < floatBuffer.Length; i++)
            {
                if (floatBuffer[i].traitType == TraitType.speed)
                {
                    speed = floatBuffer[i].value;
                    break;
                }
            }
        }

        float cost = speed * size / 10f;

        //Action cost
        if (_energy.ValueRO.value > 0)
        {
            _energy.ValueRW.value -= cost;
        }
        else
        {
            _health.ValueRW.value -= cost;
        }


        return BrainAction.move;

    }

}