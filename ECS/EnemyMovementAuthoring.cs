using Unity.Entities;
using UnityEngine;

public class EnemyMovementAuthoring : MonoBehaviour
{
    public float movementSpeed = 25f;
    public float stopDistance = 0.5f; //From player

    class Baker : Baker<EnemyMovementAuthoring>
    {
        public override void Bake(EnemyMovementAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new EnemyMovementComponent
            {
                movementSpeed = authoring.movementSpeed,
                stopDistance = authoring.stopDistance,
                verticalVelocity = 0f,
                stallTimer = 0f,
                lastDistanceSq = float.MaxValue,
                currentWaypointIndex = 0,
                hasPath = 0,
                pathPending = 0
            });
        }
    }
}

public struct EnemyMovementComponent : IComponentData
{
    public float movementSpeed;
    public float stopDistance;
    public float verticalVelocity;
    public float stallTimer;
    public float lastDistanceSq;
    public int currentWaypointIndex;
    public byte hasPath;
    public byte pathPending;
}

