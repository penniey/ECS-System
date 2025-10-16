using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerPositionUpdater : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity singletonEntity;
    private bool initialized = false;

    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        InitializeSingleton();
        Debug.Log("PlayerPositionUpdater initialized");
    }

    void InitializeSingleton()
    {
        //Check if singleton already exists
        var query = entityManager.CreateEntityQuery(typeof(PlayerPositionSingleton));
        
        if (query.IsEmpty)
        {
            //Create new singleton entity
            singletonEntity = entityManager.CreateEntity(typeof(PlayerPositionSingleton));
            entityManager.SetComponentData(singletonEntity, new PlayerPositionSingleton 
            { 
                Position = transform.position 
            });
            Debug.Log("Created new PlayerPositionSingleton entity");
        }
        else
        {
            singletonEntity = query.GetSingletonEntity();
            Debug.Log("Found existing PlayerPositionSingleton entity");
        }
        
        query.Dispose();
        initialized = true;
    }

    void Update()
    {
        if (!initialized || !entityManager.Exists(singletonEntity))
        {
            InitializeSingleton();
        }

        //Update player position every frame
        entityManager.SetComponentData(singletonEntity, new PlayerPositionSingleton 
        { 
            Position = transform.position 
        });
    }

    void OnDestroy()
    {
        if (World.DefaultGameObjectInjectionWorld == null) return;
        
        if (initialized && entityManager.Exists(singletonEntity))
        {
            entityManager.DestroyEntity(singletonEntity);
        }
    }
}

public struct PlayerPositionSingleton : IComponentData
{
    public float3 Position;
}