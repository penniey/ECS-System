using Unity.Entities;
using Unity.Mathematics;

public struct EnemyDeathEvent : IComponentData
{
    public Entity enemyEntity;
    public float3 deathPosition;
    public float xpValue;
}

