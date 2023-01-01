using Unity.Entities;

public partial class EpochTimer : SystemBase
{
    protected override void OnUpdate()
    {


        RefRW<SimStateComponent> state = SystemAPI.GetSingletonRW<SimStateComponent>();



        /*if (state.ValueRO.phase == Phase.start)
        {
            state.ValueRW.phase = Phase.running;
        }*/


        if (state.ValueRO.currentEpoch > state.ValueRO.maxEpochs) 
        {
            state.ValueRW.phase = Phase.stop;
        }


        
        if(state.ValueRO.timeElapsed > state.ValueRO.epochDuration && state.ValueRO.phase == Phase.running) 
        {
            state.ValueRW.phase = Phase.end;
            state.ValueRW.currentEpoch++;
            state.ValueRW.timeElapsed = 0f;
        }
        /*else if (state.ValueRO.timeElapsed > 0 && state.ValueRO.timeElapsed <= state.ValueRO.epochDuration && state.ValueRO.phase != Phase.stop && state.ValueRO.phase != Phase.end) 
        {
            state.ValueRW.phase = Phase.running;
        }*/


        //Start phase
        if ((state.ValueRO.timeElapsed == 0f || state.ValueRO.phase == Phase.evaluated) && (state.ValueRO.phase != Phase.stop && state.ValueRO.phase != Phase.end && state.ValueRO.phase != Phase.start))
        {
            state.ValueRW.phase = Phase.start;
            state.ValueRW.timeElapsed = 0f;

        }

        //Increment time
        state.ValueRW.timeElapsed += SystemAPI.Time.DeltaTime;

    }
}
