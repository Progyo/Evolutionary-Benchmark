using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[assembly: RegisterGenericJobType(typeof(EvaluateJob<EvaluationFunction>))]

[BurstCompile]
public struct EvaluationFunction : IEvaluate
{
    private RefRW<LocalToWorldTransform> _transform;
    private RefRW<TargetPositionComponent> _target;
    private RefRW<MaxDecisionSpeedComponent> _maxDecisionSpeed;
    private RefRW<DecisionSpeedComponent> _decisionTimeLeft;
    private RefRW<EnergyComponent> _energy;
    private RefRW<HealthComponent> _health;
    private RefRW<Entity> _entity;
    private RefRW<MaxEnergyComponent> _maxEnergy;
    private RefRW<MaxHealthComponent> _maxHealth;
    private BufferLookup<TraitBufferComponent<int>> _intLookup;
    private BufferLookup<TraitBufferComponent<float>> _floatLookup;


    public RefRW<Entity> entity { get { return _entity; } set { _entity = value; } }
    public RefRW<LocalToWorldTransform> transform { get { return _transform; } set { _transform = value; } }
    public RefRW<TargetPositionComponent> target { get { return _target; } set { _target = value; } }
    public RefRW<MaxDecisionSpeedComponent> maxDecisionSpeed { get { return _maxDecisionSpeed; } set { _maxDecisionSpeed = value; } }
    public RefRW<DecisionSpeedComponent> decisionTimeLeft { get { return _decisionTimeLeft; } set { _decisionTimeLeft = value; } }
    public RefRW<EnergyComponent> energy { get { return _energy; } set { _energy = value; } }
    public RefRW<HealthComponent> health { get { return _health; } set { _health = value; } }

    public RefRW<MaxEnergyComponent> maxEnergy { get { return _maxEnergy; } set { _maxEnergy = value; } }
    public RefRW<MaxHealthComponent> maxHealth { get { return _maxHealth; } set { _maxHealth = value; } }
    public BufferLookup<TraitBufferComponent<int>> intLookup { get { return _intLookup; } set { _intLookup = value; } }
    public BufferLookup<TraitBufferComponent<float>> floatLookup { get { return _floatLookup; } set { _floatLookup = value; } }

    [BurstCompile]
    public float Evaluate()
    {
        var health = math.max(_health.ValueRO.value,0f);
        var energy = math.max(_energy.ValueRO.value,0f);
        var maxHealth = _maxHealth.ValueRO.value;
        var maxEnergy = _maxEnergy.ValueRO.value;

        return (health*health+energy)/(maxHealth*maxHealth + maxEnergy);
    }
}
