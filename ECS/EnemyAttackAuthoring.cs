using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EnemyAttackAuthoring : MonoBehaviour
{
    public float attackRange = 10f;
    public float attackCooldown = 1f;
    public int damage = 10;
    public EnemyType enemyType = EnemyType.Melee;
    public float bulletSpeed = 20f;
    public Transform bulletSpawnPoint;
    public float trailSpeed = 100f;
    public float timeSinceLastAttack = 0f;

    class Baker : Baker<EnemyAttackAuthoring>
    {
        public override void Bake(EnemyAttackAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new EnemyAttackComponent
            {
                attackRange = authoring.attackRange,
                attackCooldown = authoring.attackCooldown,
                damage = authoring.damage,
                enemyType = authoring.enemyType,
                bulletSpeed = authoring.bulletSpeed,
                trailSpeed = authoring.trailSpeed,
                timeSinceLastAttack = authoring.attackCooldown 
            });
        }
    }
}

public enum EnemyType
{
    Melee,    
    Ranged,
    Bomber
}

public struct EnemyAttackComponent : IComponentData
{
    public float attackRange;
    public float attackCooldown;
    public int damage;
    public EnemyType enemyType;
    public float bulletSpeed;
    public float trailSpeed;
    public float timeSinceLastAttack;
}

public struct BulletComponent : IComponentData
{
    public float3 direction;
    public float speed;
    public int damage;
    public float lifetime;
    public float currentLifetime;
    public Entity sourceEnemy;
}


