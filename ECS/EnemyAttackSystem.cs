using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;
using Unity.Entities.UniversalDelegates;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMovementSystem))]
public partial struct EnemyAttackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyAttackComponent>();
        state.RequireForUpdate<PlayerPositionSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var playerPosition = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().PhysicsWorld;

        //Create a command buffer to queue damage events
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        new AttackJob
        {
            DeltaTime = deltaTime,
            PlayerPosition = playerPosition,
            PhysicsWorld = physicsWorld,
            ECB = ecb.AsParallelWriter()
        }.ScheduleParallel();

        state.Dependency.Complete();
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public partial struct AttackJob : IJobEntity
    {
        public float DeltaTime;
        public float3 PlayerPosition;
        [ReadOnly] public PhysicsWorld PhysicsWorld;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref EnemyAttackComponent attack, in LocalTransform transform)
        {
            attack.timeSinceLastAttack += DeltaTime;

            if (attack.timeSinceLastAttack < attack.attackCooldown)
                return;
            //Check if player is within range
            float3 direction = PlayerPosition - transform.Position;
            float distance = math.length(direction);

            if (distance > attack.attackRange)
                return;

            bool shouldDealDamage = false;

            switch (attack.enemyType)
            {
                case EnemyType.Melee:
                    shouldDealDamage = CheckRaycastAttack(transform.Position, PlayerPosition, PhysicsWorld);
                    break;

                case EnemyType.Ranged:
                    //Spawn bullet entity
                    var bulletEntity = ECB.CreateEntity(sortKey);

                    float3 bulletDirection = math.normalize(PlayerPosition - transform.Position);

                    ECB.AddComponent(sortKey, bulletEntity, new BulletComponent
                    {
                        direction = bulletDirection,
                        speed = attack.bulletSpeed,
                        damage = attack.damage,
                        lifetime = 5f,
                        currentLifetime = 0f,
                        sourceEnemy = entity
                    });

                    ECB.AddComponent(sortKey, bulletEntity, LocalTransform.FromPosition(
                        transform.Position //Change when we have the enemy model
                    ));

                    shouldDealDamage = false;
                    break;

                case EnemyType.Bomber:
                    shouldDealDamage = CheckBombAttack(transform.Position, PlayerPosition);
                    if (shouldDealDamage)
                    {
                                // Create damage event for the explosion
                                var damageEntity = ECB.CreateEntity(sortKey);
                                ECB.AddComponent(sortKey, damageEntity, new DamageEventComponent
                                {
                                    Damage = attack.damage,
                                    SourcePosition = transform.Position,
                                    TargetPosition = PlayerPosition
                                });
                                
                                // Kill the bomber by creating a death event
                                var deathEntity = ECB.CreateEntity(sortKey);
                                ECB.AddComponent(sortKey, deathEntity, new EnemyDeathEvent
                                {
                                    enemyEntity = entity,
                                    deathPosition = transform.Position,
                                    xpValue = 0 // Or give XP if you want
                                });
                                
                                // Don't set shouldDealDamage again, we already created the damage event
                                shouldDealDamage = false;
                    }
                    break;
            }

            //Queue damage event if hit
            if (shouldDealDamage)
            {
                //Create a damage event entity
                var damageEntity = ECB.CreateEntity(sortKey);
                ECB.AddComponent(sortKey, damageEntity, new DamageEventComponent
                {
                    Damage = attack.damage,
                    SourcePosition = transform.Position,
                    TargetPosition = PlayerPosition
                });
            }

            attack.timeSinceLastAttack = 0f;
        }
        

        private bool CheckRaycastAttack(float3 attackPosition, float3 playerPos, PhysicsWorld physicsWorld)
        {
            float3 attackDir = math.normalize(playerPos - attackPosition);
            float distanceToPlayer = math.distance(attackPosition, playerPos);

            var rayInput = new RaycastInput
            {
                Start = attackPosition + new float3(0, 0.5f, 0),
                End = attackPosition + new float3(0, 0.5f, 0) + attackDir * distanceToPlayer,
                Filter = CollisionFilter.Default
            };

            bool hitSomething = physicsWorld.CastRay(rayInput, out var hit);

            if (!hitSomething)
            {
                return true; 
            }

            float distToHit = math.distance(hit.Position, playerPos);
            return distToHit < 1f;
        }

        private bool CheckBombAttack(float3 attackPosition, float3 playerPos)
        {
            float distanceToPlayer = math.distance(attackPosition, playerPos);
            return distanceToPlayer <= 5f; 
        }
    }
}

public struct DamageEventComponent : IComponentData
{
    public int Damage;
    public float3 SourcePosition;
    public float3 TargetPosition;
}