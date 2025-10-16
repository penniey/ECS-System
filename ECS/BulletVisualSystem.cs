using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class BulletVisualSystem : SystemBase
{
    private Dictionary<Entity, GameObject> bulletVisuals = new Dictionary<Entity, GameObject>();
    private Queue<GameObject> availableBullets = new Queue<GameObject>();
    private Transform poolParent;
    private Material bulletMaterial;

    protected override void OnStartRunning()
    {
        poolParent = new GameObject("BulletPool").transform;
        
        bulletMaterial = null;
        
        for (int i = 0; i < 50; i++)
        {
            CreateNewBullet();
        }
        
        Debug.Log($"BulletVisualSystem initialized with {availableBullets.Count} bullets");
    }

    private GameObject CreateNewBullet()
    {
        var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Bullet";
        bullet.transform.SetParent(poolParent);
        bullet.transform.localScale = Vector3.one * 0.2f;
        
        Object.Destroy(bullet.GetComponent<Collider>());
        
        if (bulletMaterial == null)
        {
            bulletMaterial = new Material(bullet.GetComponent<Renderer>().sharedMaterial);
            bulletMaterial.color = Color.yellow;
        }
        else
        {
            bullet.GetComponent<Renderer>().material = bulletMaterial;
        }
        
        bullet.SetActive(false);
        availableBullets.Enqueue(bullet);
        return bullet;
    }

    private GameObject GetBullet()
    {
        GameObject bullet;
        if (availableBullets.Count > 0)
        {
            bullet = availableBullets.Dequeue();
        }
        else
        {
            Debug.LogWarning("Bullet pool exhausted, creating new bullet");
            bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet_Extra";
            bullet.transform.SetParent(poolParent);
            bullet.transform.localScale = Vector3.one * 0.2f;
            Object.Destroy(bullet.GetComponent<Collider>());
            bullet.GetComponent<Renderer>().material = bulletMaterial;
        }
        
        bullet.SetActive(true);
        return bullet;
    }

    private void ReturnBullet(GameObject bullet)
    {
        if (bullet != null)
        {
            bullet.SetActive(false);
            bullet.transform.SetParent(poolParent);
            availableBullets.Enqueue(bullet);
        }
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.TempJob); //ADD THIS
        
        foreach (var (transform, entity) in 
            SystemAPI.Query<RefRO<LocalTransform>>()
                .WithNone<BulletVisualTag>()
                .WithAll<BulletComponent>()
                .WithEntityAccess())
        {
            if (bulletVisuals.ContainsKey(entity))
            {
                continue;
            }
            
            var bullet = GetBullet();
            bullet.transform.position = new Vector3(
                transform.ValueRO.Position.x,
                transform.ValueRO.Position.y,
                transform.ValueRO.Position.z
            );
            
            bulletVisuals[entity] = bullet;
            
            ecb.AddComponent<BulletVisualTag>(entity);
            
            Debug.Log($"Spawned bullet visual at {bullet.transform.position}");
        }

        foreach (var (transform, entity) in 
            SystemAPI.Query<RefRO<LocalTransform>>()
                .WithAll<BulletComponent, BulletVisualTag>()
                .WithEntityAccess())
        {
            if (bulletVisuals.TryGetValue(entity, out GameObject visual) && visual != null)
            {
                Vector3 newPos = new Vector3(
                    transform.ValueRO.Position.x,
                    transform.ValueRO.Position.y,
                    transform.ValueRO.Position.z
                );
                
                visual.transform.position = newPos;
            }
        }

            //clean
        var entitiesToRemove = new List<Entity>();
        foreach (var kvp in bulletVisuals)
        {
            if (!EntityManager.Exists(kvp.Key))
            {
                ReturnBullet(kvp.Value);
                entitiesToRemove.Add(kvp.Key);
            }
        }

        foreach (var entity in entitiesToRemove)
        {
            bulletVisuals.Remove(entity);
        }
        
        // Playback and dispose ECB
        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

public struct BulletVisualTag : IComponentData { }