using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;


[BurstCompile]
[UpdateAfter(typeof(EpochTimer))]
public partial struct ControlSystem : ISystem
{
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
        float deltaTime = SystemAPI.Time.DeltaTime;

        JobHandle handle = new MoveJob { deltaTime = deltaTime}.ScheduleParallel(state.Dependency);

        handle.Complete();
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
