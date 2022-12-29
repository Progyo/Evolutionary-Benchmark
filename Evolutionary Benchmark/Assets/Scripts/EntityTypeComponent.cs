using Unity.Entities;

/// <summary>
/// Tags the entity
/// </summary>
public struct EntityTypeComponent : IComponentData
{
    public EntityType value;
}
