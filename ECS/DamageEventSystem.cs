using Unity.Entities;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyAttackSystem))]
public partial class DamageEventSystem : SystemBase
{
    private PlayerStats playerStats;
    private GameObject playerObject;

    protected override void OnStartRunning()
    {
        playerObject = GameObject.FindGameObjectWithTag("Character");
        if (playerObject != null)
        {
            playerStats = playerObject.GetComponent<PlayerStats>();
        }
    }

    protected override void OnUpdate()
    {
        if (playerStats == null)
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Character");
            }
            playerStats = playerObject.GetComponent<PlayerStats>();
        }

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var localPlayerStats = playerStats; 

        foreach (var (damageEvent, entity) in SystemAPI.Query<RefRO<DamageEventComponent>>().WithEntityAccess())
        {
            localPlayerStats.TakeDamage(damageEvent.ValueRO.Damage);

            ecb.DestroyEntity(entity);
        }

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}