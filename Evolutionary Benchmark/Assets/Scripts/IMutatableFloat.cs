using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public interface IMutatableFloat : IMutatable
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.Cdecl)]
    public delegate float PerformCalculation(float y);
    public void Mutate(PerformCalculation a, float val);
}