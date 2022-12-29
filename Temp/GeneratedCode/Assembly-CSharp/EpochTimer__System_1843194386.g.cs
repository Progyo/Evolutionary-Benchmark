#pragma warning disable 0219
#line 1 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Temp\GeneratedCode\Assembly-CSharp\EpochTimer__System_1843194386.g.cs"
using Unity.Entities;

[global::System.Runtime.CompilerServices.CompilerGenerated]
public partial class EpochTimer : Unity.Entities.SystemBase
{
    [Unity.Entities.DOTSCompilerPatchedMethod("OnUpdate")]
    void __OnUpdate_1817F1CB()
    {
        #line 9 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
        RefRW<SimStateComponent> state = __query_595798923_0.GetSingletonRW<SimStateComponent>();
        #line 11 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
        if (state.ValueRO.currentEpoch > state.ValueRO.maxEpochs)
        {
            #line 13 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            state.ValueRW.phase = Phase.stop;
        }

        #line 17 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
        if ((state.ValueRO.timeElapsed == 0f || state.ValueRO.phase == Phase.start) && state.ValueRO.phase != Phase.stop)
        {
            #line 19 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            state.ValueRW.phase = Phase.start;
        }

        #line 23 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
        if (state.ValueRO.timeElapsed > state.ValueRO.epochDuration && state.ValueRO.phase != Phase.stop)
        {
            #line 25 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            state.ValueRW.phase = Phase.end;
            #line 26 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            state.ValueRW.currentEpoch++;
            #line 27 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            state.ValueRW.timeElapsed = 0f;
        }
        else
        {
            #line 29 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
            if (state.ValueRO.timeElapsed > 0 && state.ValueRO.timeElapsed <= state.ValueRO.epochDuration && state.ValueRO.phase != Phase.stop)
            {
                #line 31 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
                state.ValueRW.phase = Phase.running;
            }
        }

        #line 35 "C:\Users\Nadim\Documents\Programming\Unity\Evolutionary Benchmark\Evolutionary Benchmark\Assets/Scripts/Per Frame/EpochTimer.cs"
        state.ValueRW.timeElapsed += this.CheckedStateRef.WorldUnmanaged.Time.DeltaTime;
    }

    Unity.Entities.EntityQuery __query_595798923_0;
    protected override void OnCreateForCompiler()
    {
        base.OnCreateForCompiler();
        __query_595798923_0 = this.CheckedStateRef.GetEntityQuery(new Unity.Entities.EntityQueryDesc{All = new Unity.Entities.ComponentType[]{Unity.Entities.ComponentType.ReadWrite<SimStateComponent>()}, Any = new Unity.Entities.ComponentType[]{}, None = new Unity.Entities.ComponentType[]{}, Options = Unity.Entities.EntityQueryOptions.Default | Unity.Entities.EntityQueryOptions.IncludeSystems});
    }
}