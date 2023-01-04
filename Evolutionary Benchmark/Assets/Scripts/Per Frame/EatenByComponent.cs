using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

public struct EatenByComponent : IComponentData
{
    /// <summary>
    /// The amount the food item restores energy/health
    /// </summary>
    //public float nurishment;

    /// <summary>
    /// Reference to the entity that is eating this item
    /// </summary>
    public RefStruct<Entity> eatenBy;

    public RefStruct<HealthComponent> health;
    public RefStruct<MaxHealthComponent> maxHealth;
    public RefStruct<EnergyComponent> energy;
    public RefStruct<MaxEnergyComponent> maxEnergy;
}

//[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "ENABLE_UNITY_COLLECTIONS_CHECKS", GenericTypeArguments = new Type[] { typeof(BurstCompatibleComponentData) })]
//Taken from RefRW implementation
public struct RefStruct<T> : IQueryTypeParameter where T : struct
{
    private unsafe readonly byte* _Data;

    public unsafe bool IsValid => _Data != null;

    public unsafe ref T ValueRW
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(_Data);
        }
    }

    public unsafe ref readonly T ValueRO
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(_Data);
        }
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    [Conditional("UNITY_DOTS_DEBUG")]
    private void OutOfBoundsArrayConstructor(int index, int length)
    {
        if (index < 0 || index >= length)
        {
            throw new ArgumentOutOfRangeException("index is out of bounds of NativeArray<>.Length.");
        }
    }


    public unsafe RefStruct(NativeArray<T> componentDataNativeArray, int index)
    {
        _Data = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(componentDataNativeArray);
        _Data += UnsafeUtility.SizeOf<T>() * index;
        //NativeArrayUnsafeUtility.GetAtomicSafetyHandle(componentDataNativeArray);
        OutOfBoundsArrayConstructor(index, componentDataNativeArray.Length);
    }

    public static RefStruct<T> Optional(NativeArray<T> componentDataNativeArray, int index)
    {
        return (componentDataNativeArray.Length == 0) ? default(RefStruct<T>) : new RefStruct<T>(componentDataNativeArray, index);
    }

}


public struct RefRef<T> : IQueryTypeParameter where T : struct
{
    private unsafe readonly byte* _Data;

    public unsafe bool IsValid => _Data != null;

    public unsafe ref T ValueRW
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(_Data);
        }
    }

    public unsafe ref readonly T ValueRO
    {
        get
        {
            return ref UnsafeUtility.AsRef<T>(_Data);
        }
    }

    [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
    [Conditional("UNITY_DOTS_DEBUG")]
    private void OutOfBoundsArrayConstructor(int index, int length)
    {
        if (index < 0 || index >= length)
        {
            throw new ArgumentOutOfRangeException("index is out of bounds of NativeArray<>.Length.");
        }
    }


    public unsafe RefRef(ref T value) 
    {
        _Data = (byte*)UnsafeUtility.AddressOf<T>(ref value);
    }

    public unsafe RefRef(NativeArray<T> componentDataNativeArray, int index)
    {
        _Data = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(componentDataNativeArray);
        _Data += UnsafeUtility.SizeOf<T>() * index;
        //NativeArrayUnsafeUtility.GetAtomicSafetyHandle(componentDataNativeArray);
        OutOfBoundsArrayConstructor(index, componentDataNativeArray.Length);
    }

    public static RefRef<T> Optional(NativeArray<T> componentDataNativeArray, int index)
    {
        return (componentDataNativeArray.Length == 0) ? default(RefRef<T>) : new RefRef<T>(componentDataNativeArray, index);
    }

}