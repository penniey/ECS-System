using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(DamageEventSystem))]
public partial struct EnemyHealthSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyHealthComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        // Process bullet damage events
        foreach (var (damageEvent, entity) in SystemAPI.Query<RefRO<PlayerBulletDamageEvent>>().WithEntityAccess())
        {
            var targetEnemy = damageEvent.ValueRO.targetEnemy;
            
            if (state.EntityManager.Exists(targetEnemy) && 
                state.EntityManager.HasComponent<EnemyHealthComponent>(targetEnemy))
            {
                var healthComponent = SystemAPI.GetComponentRW<EnemyHealthComponent>(targetEnemy);
                
                if (!healthComponent.ValueRO.isDead)
                {
                    // Apply damage (headshot = 2x damage)
                    float finalDamage = damageEvent.ValueRO.damage;
                    if (damageEvent.ValueRO.isHeadshot)
                    {
                        finalDamage *= 2f;
                    }
                    
                    healthComponent.ValueRW.currentHealth -= finalDamage;
                    
                    // Create damage text event
                    var damageTextEntity = ecb.CreateEntity();
                    ecb.AddComponent(damageTextEntity, new DamageTextEvent
                    {
                        damage = finalDamage,
                        position = damageEvent.ValueRO.hitPosition,
                        isHeadshot = damageEvent.ValueRO.isHeadshot
                    });
                    
                    // Check if enemy died
                    if (healthComponent.ValueRO.currentHealth <= 0)
                    {
                        healthComponent.ValueRW.isDead = true;
                        
                        // Create death event
                        var deathEntity = ecb.CreateEntity();
                        ecb.AddComponent(deathEntity, new EnemyDeathEvent
                        {
                            enemyEntity = targetEnemy,
                            deathPosition = SystemAPI.GetComponent<LocalTransform>(targetEnemy).Position,
                            xpValue = healthComponent.ValueRO.xpValue
                        });
                    }
                }
            }
            
            // Destroy the damage event
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

// New component for damage text events
public struct DamageTextEvent : IComponentData
{
    public float damage;
    public float3 position;
    public bool isHeadshot;
}