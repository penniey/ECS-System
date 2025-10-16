using Unity.Entities;
using Unity.Mathematics;

// This component is added to bullets/raycasts that hit enemies
public struct PlayerBulletDamageEvent : IComponentData
{
    public Entity targetEnemy;
    public float damage;
    public bool isHeadshot;
    public float3 hitPosition;
}