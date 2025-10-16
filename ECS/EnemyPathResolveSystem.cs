using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

//DONT MAKE THIS RUN ON THE MAIN THREAD (BURST COMPILE)
//WE NEED TO REACH NAVMESH
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(EnemyMovementSystem))]
public partial class EnemyPathResolveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var targetFloat3 = SystemAPI.GetSingleton<PlayerPositionSingleton>().Position;
        var target = new Vector3(targetFloat3.x, targetFloat3.y, targetFloat3.z);

        foreach (var (movement, waypoints, entity) in SystemAPI
                     .Query<RefRW<EnemyMovementComponent>, DynamicBuffer<EnemyWaypoint>>()
                     .WithEntityAccess())
        {
            if (movement.ValueRO.pathPending == 0)
            {
                continue;
            }

            movement.ValueRW.pathPending = 0;
            movement.ValueRW.stallTimer = 0f;

            if (!EntityManager.HasComponent<EnemyNavMeshBridge>(entity))
            {
                continue;
            }

            var bridge = EntityManager.GetComponentObject<EnemyNavMeshBridge>(entity);
            if (bridge == null)
            {
                continue;
            }

            var navPath = new NavMeshPath();
            if (bridge.TryRequestPath(target, navPath))
            {
                waypoints.Clear();
                foreach (var corner in navPath.corners)
                {
                    waypoints.Add(new EnemyWaypoint
                    {
                        Position = new float3(corner.x, corner.y, corner.z)
                    });
                }

                movement.ValueRW.currentWaypointIndex = 0;
                movement.ValueRW.hasPath = (byte)(waypoints.Length > 0 ? 1 : 0);
            }
        }
    }
}
