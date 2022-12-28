using Unity.Entities;


public partial class EpochTimer : SystemBase
{
    protected override void OnUpdate()
    {
        
        RefRW<SimStateComponent> state = SystemAPI.GetSingletonRW<SimStateComponent>();

        if(state.ValueRO.currentEpoch > state.ValueRO.maxEpochs) 
        {
            state.ValueRW.phase = Phase.stop;
        }

        //Start phase
        if ((state.ValueRO.timeElapsed == 0f || state.ValueRO.phase == Phase.start) && state.ValueRO.phase != Phase.stop) 
        {
            state.ValueRW.phase = Phase.start;
        }


        if(state.ValueRO.timeElapsed > state.ValueRO.epochDuration && state.ValueRO.phase != Phase.stop) 
        {
            state.ValueRW.phase = Phase.end;
            state.ValueRW.currentEpoch++;
            state.ValueRW.timeElapsed = 0f;
        }
        else if (state.ValueRO.timeElapsed > 0 && state.ValueRO.timeElapsed <= state.ValueRO.epochDuration && state.ValueRO.phase != Phase.stop) 
        {
            state.ValueRW.phase = Phase.running;
        }

        //Increment time
        state.ValueRW.timeElapsed += SystemAPI.Time.DeltaTime;
        
    }
}
