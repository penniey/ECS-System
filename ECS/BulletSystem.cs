using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Physics;
using Unity.Collections;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAttackSystem))]
public partial struct BulletSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BulletComponent>();
        state.RequireForUpdate<PlayerPositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        var playerPosition = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;

        //Move bullets and check collisions
        foreach (var (bullet, transform, entity) in 
            SystemAPI.Query<RefRW<BulletComponent>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            bullet.ValueRW.currentLifetime += deltaTime;
            
            //Destroy if oldge
            if (bullet.ValueRO.currentLifetime >= bullet.ValueRO.lifetime)
            {
                ecb.DestroyEntity(entity);
                continue;
            }

            //Calculate movement
            float3 movement = bullet.ValueRO.direction * bullet.ValueRO.speed * deltaTime;
            float3 startPos = transform.ValueRO.Position;
            float3 endPos = startPos + movement;
            
            //Updatrtes along the path
            var playerRayInput = new RaycastInput
            {
                Start = startPos,
                End = endPos,
                Filter = new CollisionFilter
                {
                    BelongsTo = 1u << 3,      
                    CollidesWith = ~0u,        
                    GroupIndex = 0
                }
            };
            
            //check distance at both start and end positions
            float distToPlayerStart = math.distance(startPos, playerPosition);
            float distToPlayerEnd = math.distance(endPos, playerPosition);
            
            bool hitPlayer = false;
            
            //Check if bullet passes through player's position
            if (distToPlayerStart < 1.5f || distToPlayerEnd < 1.5f)
            {
                hitPlayer = true;
            }
            else
            {
                float3 bulletPath = endPos - startPos;
                float pathLength = math.length(bulletPath);
                
                if (pathLength > 0.001f)
                {
                    float3 bulletDir = math.normalize(bulletPath);
                    float3 toPlayer = playerPosition - startPos;
                    float projectionLength = math.dot(toPlayer, bulletDir);
                    
                    //check if player is along the bullet's path
                    if (projectionLength >= 0 && projectionLength <= pathLength)
                    {
                        float3 closestPoint = startPos + bulletDir * projectionLength;
                        float distToPath = math.distance(closestPoint, playerPosition);
                        
                        if (distToPath < 1f) 
                        {
                            hitPlayer = true;
                        }
                    }
                }
            }
            
            if (hitPlayer)
            {
                var damageEntity = ecb.CreateEntity();
                ecb.AddComponent(damageEntity, new DamageEventComponent
                {
                    Damage = bullet.ValueRO.damage,
                    SourcePosition = startPos,
                    TargetPosition = playerPosition
                });
                
                ecb.DestroyEntity(entity);
                continue;
            }

            var wallRayInput = new RaycastInput
            {
                Start = startPos,
                End = endPos,
                Filter = new CollisionFilter
                {
                    BelongsTo = 1u << 3,      
                    CollidesWith = 1u << 0,    
                    GroupIndex = 0
                }
            };

            if (physicsWorld.CastRay(wallRayInput, out var hit))
            {
                ecb.DestroyEntity(entity);
            }
            else
            {
                //move bullet
                transform.ValueRW.Position = endPos;
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}