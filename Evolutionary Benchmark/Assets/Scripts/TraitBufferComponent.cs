using Unity.Entities;

[assembly: RegisterGenericComponentType(typeof(TraitBufferComponent<int>))]
[assembly: RegisterGenericComponentType(typeof(TraitBufferComponent<float>))]
public struct TraitBufferComponent<T> : IBufferElementData where T: struct
{
    /// <summary>
    /// The trait
    /// </summary>
    public TraitType traitType;

    /// <summary>
    /// That value of the trait
    /// </summary>
    public T value;
}
