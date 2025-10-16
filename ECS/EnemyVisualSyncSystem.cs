using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using System.Collections.Generic;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class EnemyVisualSyncSystem : SystemBase
{
    private Dictionary<Entity, GameObject> entityToVisual = new Dictionary<Entity, GameObject>();
    
    protected override void OnUpdate()
    {
        //Register new visuals
        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithAll<EnemyHealthComponent>().WithEntityAccess())
        {
            //If not cached, try to find it
            if (!entityToVisual.ContainsKey(entity))
            {
                var enemyVisual = GameObject.Find($"Enemy_{entity.Index}");
                if (enemyVisual != null)
                {
                    entityToVisual[entity] = enemyVisual;
                    Debug.Log($"Registered visual for entity {entity.Index}");
                }
            }
            
            //Update position if we have the visual
            if (entityToVisual.TryGetValue(entity, out GameObject visual) && visual != null)
            {
                visual.transform.position = new Vector3(
                    transform.ValueRO.Position.x, 
                    transform.ValueRO.Position.y, 
                    transform.ValueRO.Position.z
                );
                visual.transform.rotation = transform.ValueRO.Rotation;
            }
        }
        
        //Clean up destroyed entities
        var entitiesToRemove = new List<Entity>();
        foreach (var kvp in entityToVisual)
        {
            if (!EntityManager.Exists(kvp.Key) || kvp.Value == null)
            {
                entitiesToRemove.Add(kvp.Key);
            }
        }
        
        foreach (var entity in entitiesToRemove)
        {
            entityToVisual.Remove(entity);
        }
    }
}