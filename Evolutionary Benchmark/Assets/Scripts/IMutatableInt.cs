using System.Collections;
using System.Collections.Generic;
using Unity.Entities;


public interface IMutatableInt : IMutatable
{
    [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.Cdecl)]
    public delegate int PerformCalculation(int y);
    public void Mutate(PerformCalculation a, int val);
}
