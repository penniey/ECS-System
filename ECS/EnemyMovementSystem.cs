using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct EnemyMovementSystem : ISystem
{
    private uint frameCount;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyMovementComponent>();
        state.RequireForUpdate<PlayerPositionSingleton>();
        state.RequireForUpdate<PhysicsWorldSingleton>();
        frameCount = 0;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        frameCount++;

        var playerPosition = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var deltaTime = SystemAPI.Time.DeltaTime;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        state.Dependency = new MoveJob
        {
            DeltaTime = deltaTime,
            PlayerPosition = playerPosition,
            StallThreshold = 1f,
            ProgressEpsilonSq = 0.25f,
            PhysicsWorld = physicsWorld
        }.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        public float StallThreshold;
        public float ProgressEpsilonSq;
        [ReadOnly] public PhysicsWorld PhysicsWorld;

        public void Execute(ref LocalTransform transform, ref EnemyMovementComponent movement, DynamicBuffer<EnemyWaypoint> waypoints)
        {
            float3 target = PlayerPosition;
            bool followingPath = movement.hasPath == 1 && waypoints.Length > 0 && movement.currentWaypointIndex < waypoints.Length;

            if (followingPath)
            {
                float3 waypoint = waypoints[movement.currentWaypointIndex].Position;
                float3 flatDelta = waypoint - transform.Position;
                flatDelta.y = 0f;

                if (math.lengthsq(flatDelta) <= ProgressEpsilonSq)
                {
                    movement.currentWaypointIndex++;
                    if (movement.currentWaypointIndex >= waypoints.Length)
                    {
                        waypoints.Clear();
                        movement.currentWaypointIndex = 0;
                        movement.hasPath = 0;
                        followingPath = false;
                    }
                }

                if (followingPath)
                {
                    target = waypoints[movement.currentWaypointIndex].Position;
                }
            }
            else if (waypoints.Length > 0)
            {
                waypoints.Clear();
                movement.currentWaypointIndex = 0;
                movement.hasPath = 0;
            }

            float3 toTarget = target - transform.Position;
            float3 horizontal = new float3(toTarget.x, 0f, toTarget.z);
            float horizontalDistance = math.length(horizontal);

            if (horizontalDistance > 0.1f)
            {
                float3 direction = horizontal / horizontalDistance;

                var forwardRay = new RaycastInput
                {
                    Start = transform.Position + new float3(0f, 0.5f, 0f),
                    End = transform.Position + new float3(0f, 0.5f, 0f) + direction * movement.movementSpeed * DeltaTime * 2f,
                    Filter = new CollisionFilter { BelongsTo = ~0u, CollidesWith = ~0u, GroupIndex = 0 }
                };

                if (!PhysicsWorld.CastRay(forwardRay, out var obstacleHit))
                {
                    transform.Position += direction * movement.movementSpeed * DeltaTime;
                }
                else
                {
                    float3 surfaceNormal = obstacleHit.SurfaceNormal;
                    float slopeAngle = math.degrees(math.acos(math.clamp(math.dot(surfaceNormal, math.up()), -1f, 1f)));

                    if (slopeAngle < 45f)
                    {
                        transform.Position += direction * movement.movementSpeed * DeltaTime;
                    }
                    else
                    {
                        float3 right = math.cross(direction, math.up());

                        var sideRay = forwardRay;
                        sideRay.End = sideRay.Start + right * movement.movementSpeed * DeltaTime * 2f;
                        if (!PhysicsWorld.CastRay(sideRay))
                        {
                            transform.Position += right * movement.movementSpeed * DeltaTime;
                        }
                        else
                        {
                            sideRay.End = sideRay.Start - right * movement.movementSpeed * DeltaTime * 2f;
                            if (!PhysicsWorld.CastRay(sideRay))
                            {
                                transform.Position -= right * movement.movementSpeed * DeltaTime;
                            }
                        }
                    }
                }

                transform.Rotation = quaternion.LookRotationSafe(direction, math.up());
            }

            const float gravity = 9.81f;
            var groundRay = new RaycastInput
            {
                Start = transform.Position + new float3(0f, 0.6f, 0f),
                End = transform.Position + new float3(0f, -0.6f, 0f),
                Filter = new CollisionFilter { BelongsTo = ~0u, CollidesWith = ~0u, GroupIndex = 0 }
            };

            if (!PhysicsWorld.CastRay(groundRay, out var groundHit))
            {
                movement.verticalVelocity -= gravity * DeltaTime;
                transform.Position += new float3(0f, movement.verticalVelocity * DeltaTime, 0f);
            }
            else
            {
                float targetY = groundHit.Position.y + 0.5f;
                transform.Position = new float3(transform.Position.x, targetY, transform.Position.z);
                movement.verticalVelocity = 0f;
            }

            float currentDistanceSq = math.lengthsq(PlayerPosition - transform.Position);
            if (currentDistanceSq > movement.lastDistanceSq - 0.01f && movement.hasPath == 0)
            {
                movement.stallTimer += DeltaTime;
                if (movement.stallTimer >= StallThreshold)
                {
                    movement.pathPending = 1;
                }
            }
            else
            {
                movement.stallTimer = 0f;
            }

            movement.lastDistanceSq = currentDistanceSq;
        }
    }
}

public struct EnemyWaypoint : IBufferElementData
{
    public float3 Position;
}
