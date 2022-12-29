#pragma warning disable 0219
#line 1 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Temp\GeneratedCode\Assembly-CSharp\ControlSystem__System_1034821628.g.cs"
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;

[global::System.Runtime.CompilerServices.CompilerGenerated]
public partial struct ControlSystem : Unity.Entities.ISystem, Unity.Entities.ISystemCompilerGenerated
{
    [Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate_ref_Unity.Entities.SystemState")]
    void __OnUpdate_6E994214(ref SystemState state)
    {
        #line 28 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/ControlSystem.cs"
        float deltaTime = state.WorldUnmanaged.Time.DeltaTime;
        #line 30 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/ControlSystem.cs"
        JobHandle handle = state.Dependency = __ScheduleViaJobChunkExtension_0(new MoveJob{deltaTime = deltaTime}, __query_1690508992_0, state.Dependency, ref state);
        #line 32 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/ControlSystem.cs"
        handle.Complete();
    }

    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    Unity.Jobs.JobHandle __ScheduleViaJobChunkExtension_0(ControlSystem.MoveJob job, Unity.Entities.EntityQuery entityQuery, Unity.Jobs.JobHandle dependency, ref Unity.Entities.SystemState state)
    {
        __MoveAspect_RW_AspectTypeHandle.Update(ref state);
        job.__MoveAspectTypeHandle = __MoveAspect_RW_AspectTypeHandle;
        return Unity.Entities.JobChunkExtensions.ScheduleParallel(job, entityQuery, dependency);
        ;
    }

    Unity.Entities.EntityQuery __query_1690508992_0;
    MoveAspect.TypeHandle __MoveAspect_RW_AspectTypeHandle;
    public void OnCreateForCompiler(ref SystemState state)
    {
        __query_1690508992_0 = state.GetEntityQuery(new Unity.Entities.EntityQueryDesc{All = MoveAspect.RequiredComponents, Any = new Unity.Entities.ComponentType[]{}, None = new Unity.Entities.ComponentType[]{}, Options = Unity.Entities.EntityQueryOptions.Default});
        __MoveAspect_RW_AspectTypeHandle = new MoveAspect.TypeHandle(ref state, false);
    }
}