using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[assembly: RegisterGenericComponentType(typeof(FoodAuthoring))]

public class FoodAuthoring : MonoBehaviour
{
    public float nurishment = 1f;
}

public class FoodBaker : Baker<FoodAuthoring>
{
    public override void Bake(FoodAuthoring authoring)
    {
        AddComponent<EntityTypeComponent>(new EntityTypeComponent { value = EntityType.food });
    }
}

