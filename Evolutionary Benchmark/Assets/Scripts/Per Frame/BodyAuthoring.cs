using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

[assembly: RegisterGenericComponentType(typeof(BodyAuthoring))]
[assembly: RegisterGenericComponentType(typeof(SizeComponent))]
[assembly: RegisterGenericComponentType(typeof(SpeedComponent))]
[assembly: RegisterGenericComponentType(typeof(MaxHealthComponent))]
[assembly: RegisterGenericComponentType(typeof(MaxEnergyComponent))]
[assembly: RegisterGenericComponentType(typeof(HealthComponent))]
[assembly: RegisterGenericComponentType(typeof(EnergyComponent))]
[assembly: RegisterGenericComponentType(typeof(EyeRadiusComponent))]
[assembly: RegisterGenericComponentType(typeof(TargetPositionComponent))]
public class BodyAuthoring : MonoBehaviour
{
    public float speed;
    public int size;
    public float maxHealth;
    public float maxEnergy;
    public float viewDistance;
}


public class BodyBaker : Baker<BodyAuthoring>
{
    public override void Bake(BodyAuthoring authoring)
    {
        AddComponent<SizeComponent>(new SizeComponent { value = authoring.size });
        AddComponent<SpeedComponent>(new SpeedComponent { value = authoring.speed });
        AddComponent<MaxHealthComponent>(new MaxHealthComponent { value = authoring.maxHealth });
        AddComponent<MaxEnergyComponent>(new MaxEnergyComponent { value = authoring.maxEnergy});
        AddComponent<HealthComponent>(new HealthComponent { value = authoring.maxHealth });
        AddComponent<EnergyComponent>(new EnergyComponent { value = authoring.maxEnergy });
        AddComponent<EyeRadiusComponent>(new EyeRadiusComponent { value = authoring.viewDistance });
        //AddComponent(new SeeComponent { value = new NativeArray<SeeItem>(0,Allocator.Persistent) });
        AddComponent<TargetPositionComponent>(new TargetPositionComponent { value = new float3(10f, 0f, 0f) });
    }
}
