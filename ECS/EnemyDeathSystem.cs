using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyHealthSystem))]
public partial class EnemyDeathSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob);

        foreach (var (deathEvent, entity) in SystemAPI.Query<RefRO<EnemyDeathEvent>>().WithEntityAccess())
        {
            var enemyEntity = deathEvent.ValueRO.enemyEntity;
            
            if (EntityManager.Exists(enemyEntity))
            {
                Vector3 deathPos = new Vector3(
                    deathEvent.ValueRO.deathPosition.x,
                    deathEvent.ValueRO.deathPosition.y,
                    deathEvent.ValueRO.deathPosition.z
                );
                
                //Death VFX
                if (PublicPool.publicPoolInstance != null)
                {
                    var deathVFX = PublicPool.publicPoolInstance.deathVFXPool.Get();
                    deathVFX.transform.position = deathPos;
                    PublicPool.publicPoolInstance.ReturnGameObjectToPool(deathVFX, 2f);
                }
                
                //XP
                if (PlayerStats.playerStatsInstance != null)
                {
                    PlayerStats.playerStatsInstance.GainXP(deathEvent.ValueRO.xpValue * PlayerStats.playerStatsInstance.experienceMultiplier);
                }
                
                //Drop gold 
                if (GoldDropService.Instance != null && PlayerStats.playerStatsInstance != null)
                {
                    Debug.Log("yo");
                    EffectManager.InstantiateEffect(
                            EnemyDropConfig.Instance.goldEffectData,
                            deathPos + Vector3.up * 0.5f,
                            Quaternion.identity,
                            EffectType.AddGold
                        );
                }
                
                //Hp DRop
                if (PlayerStats.playerStatsInstance != null && EnemyDropConfig.Instance != null)
                {
                    float dropChance = EnemyDropConfig.Instance.baseHealthDropChance + PlayerStats.playerStatsInstance.luck;
                    float roll = Random.Range(0f, 100f);
                    
                    if (roll < dropChance && EnemyDropConfig.Instance.healthEffectData != null)
                    {
                        EffectManager.InstantiateEffect(
                            EnemyDropConfig.Instance.healthEffectData, 
                            deathPos + Vector3.up * 0.5f, 
                            Quaternion.identity
                        );
                        
                        Debug.Log($"Health dropped! (Luck: {PlayerStats.playerStatsInstance.luck}%, Roll: {roll:F2}/{dropChance:F2})");
                    }
                }
                
                GameObject enemyVisual = GameObject.Find($"Enemy_{enemyEntity.Index}");
                if (enemyVisual != null)
                {
                    enemyVisual.SetActive(false);
                }
                
                // Destroy entity
                ecb.DestroyEntity(enemyEntity);
            }
            
            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}