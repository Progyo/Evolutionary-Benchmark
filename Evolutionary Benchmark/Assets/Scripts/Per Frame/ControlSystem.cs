using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.VisualScripting;


[BurstCompile]
[UpdateAfter(typeof(InputSystem))]
//[CreateAfter(typeof(EpochTimer))]
public partial struct ControlSystem : ISystem
{

    //private RefRW<SimStateComponent> simState;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
       
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

            JobHandle handle = new MoveJob { deltaTime = deltaTime }.ScheduleParallel(state.Dependency);

            handle.Complete();

        }


    }

    [BurstCompile]
    private partial struct MoveJob : IJobEntity 
    {
        public float deltaTime;

        [BurstCompile]
        public void Execute(ref MoveAspect move) 
        {
            move.Move(deltaTime);
            //move.Position += new float3(deltaTime,0f,0f);
        }
    }

}
