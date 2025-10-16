using Unity.Entities;
using UnityEngine;

public class EnemyHealthAuthoring : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float xpValue = 10;
    public GameObject deathVFXPrefab;
    
    class Baker : Baker<EnemyHealthAuthoring>
    {
        public override void Bake(EnemyHealthAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new EnemyHealthComponent
            {
                currentHealth = authoring.maxHealth,
                maxHealth = authoring.maxHealth,
                xpValue = authoring.xpValue,
                isDead = false
            });
        }
    }
}

public struct EnemyHealthComponent : IComponentData
{
    public float currentHealth;
    public float maxHealth;
    public float xpValue;
    public bool isDead;
}