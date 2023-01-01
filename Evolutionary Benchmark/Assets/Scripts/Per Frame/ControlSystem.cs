
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

[BurstCompile]
[UpdateAfter(typeof(BrainSystem))]
//[CreateAfter(typeof(EpochTimer))]
public partial struct ControlSystem : ISystem
{

    //private RefRW<SimStateComponent> simState;

    BufferLookup<TraitBufferComponent<float>> _floatTraitBufferLookup;
    BufferLookup<TraitBufferComponent<int>> _intTraitBufferLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimStateComponent>();
        _floatTraitBufferLookup = state.GetBufferLookup<TraitBufferComponent<float>>(true);
        _intTraitBufferLookup = state.GetBufferLookup<TraitBufferComponent<int>>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {


        //EntityQuery query = state.GetEntityQuery(typeof(SimStateComponent));

        //SystemAPI.get
        //SimStateComponent simState;
        
        bool success = SystemAPI.TryGetSingleton<SimStateComponent>(out SimStateComponent simState);


        if (success && simState.phase == Phase.running)
        {

            float deltaTime = SystemAPI.Time.DeltaTime;

            _floatTraitBufferLookup.Update(ref state);
            _intTraitBufferLookup.Update(ref state);
            JobHandle handle = new MoveJob { deltaTime = deltaTime,
                floatTraitbufferLookup = _floatTraitBufferLookup,
                intTraitbufferLookup = _intTraitBufferLookup  }.ScheduleParallel(state.Dependency);

            handle.Complete();

        }


    }

    [BurstCompile]
    private partial struct MoveJob : IJobEntity 
    {
        [ReadOnly]
        public float deltaTime;

        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<float>> floatTraitbufferLookup;

        [NativeDisableParallelForRestriction]
        public BufferLookup<TraitBufferComponent<int>> intTraitbufferLookup;

        [BurstCompile]
        public void Execute(ref MoveAspect move) 
        {
            move.Move(deltaTime, floatTraitbufferLookup, intTraitbufferLookup);
            move.Rotate();
            //move.Position += new float3(deltaTime,0f,0f);
        }
    }

}
