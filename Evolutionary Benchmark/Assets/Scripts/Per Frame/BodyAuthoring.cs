using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[assembly: RegisterGenericComponentType(typeof(BodyAuthoring))]
[assembly: RegisterGenericComponentType(typeof(MaxHealthComponent))]
[assembly: RegisterGenericComponentType(typeof(MaxEnergyComponent))]
[assembly: RegisterGenericComponentType(typeof(HealthComponent))]
[assembly: RegisterGenericComponentType(typeof(EnergyComponent))]
[assembly: RegisterGenericComponentType(typeof(EyeRadiusComponent))]
//[assembly: RegisterGenericComponentType(typeof(SeeComponent))]
[assembly: RegisterGenericComponentType(typeof(TargetPositionComponent))]
[assembly: RegisterGenericComponentType(typeof(EntityTypeComponent))]
[assembly: RegisterGenericComponentType(typeof(MaxDecisionSpeedComponent))]
[assembly: RegisterGenericComponentType(typeof(DecisionSpeedComponent))]
public class BodyAuthoring : MonoBehaviour
{
    public float speed;
    public int size;
    public float maxHealth;
    public float maxEnergy;
    public float viewDistance;
    public float maxDecisionSpeed;
}


public class BodyBaker : Baker<BodyAuthoring>
{
    public override void Bake(BodyAuthoring authoring)
    {
        //AddComponent<SizeComponent>(new SizeComponent { value = authoring.size });
        //AddComponent<SpeedComponent>(new SpeedComponent { value = authoring.speed });
        AddComponent<MaxHealthComponent>(new MaxHealthComponent { value = authoring.maxHealth });
        AddComponent<MaxEnergyComponent>(new MaxEnergyComponent { value = authoring.maxEnergy});
        AddComponent<HealthComponent>(new HealthComponent { value = authoring.maxHealth });
        AddComponent<EnergyComponent>(new EnergyComponent { value = authoring.maxEnergy });
        AddComponent<EyeRadiusComponent>(new EyeRadiusComponent { value = authoring.viewDistance });

        //Shouldn't be able to see more than 50 things at once
        AddComponent<TargetPositionComponent>(new TargetPositionComponent { value = new float3(10f, 0f, 0f) });
        AddComponent<EntityTypeComponent>(new EntityTypeComponent { value = EntityType.blob});
        AddBuffer<SeeBufferComponent>();

        AddComponent<MaxDecisionSpeedComponent>(new MaxDecisionSpeedComponent { value = authoring.maxDecisionSpeed });
        AddComponent<DecisionSpeedComponent>(new DecisionSpeedComponent { value = authoring.maxDecisionSpeed });

        AddComponent<FitnessComponent>(new FitnessComponent { value = 0f });
        AddComponent<FoodConsumedComponent>(new FoodConsumedComponent { value = 0 });


        AddBuffer<TraitBufferComponent<float>>();
        AppendToBuffer<TraitBufferComponent<float>>(new TraitBufferComponent<float> { traitType = TraitType.speed, value = authoring.speed, maxValue = 10f, minValue = 0.1f });
        AddBuffer<TraitBufferComponent<int>>();
        AppendToBuffer<TraitBufferComponent<int>>(new TraitBufferComponent<int> { traitType = TraitType.size, value = authoring.size, maxValue = 50, minValue = 3 });
    }
}
