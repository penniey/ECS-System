using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System;

public class EnemySpawnerBridge : MonoBehaviour
{
    [SerializeField] private GameObject enemyVisualPrefab;
    [SerializeField] private int spawnCount = 10;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [SerializeField] private Transform poolParent;
    
    [Header("Enemy Stats")]
    [SerializeField] private EnemyType enemyType;

    [Header("Augment")]
    [SerializeField] private float xpMultipler = 1.0f;

    [Header("Time")]
    public float time = 0f;
    private float fakerTimer = 0f;
    private int currentEnemySpawn = 10;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private BoxCollider arenaBounds;

    private EntityManager entityManager;
    private Pool enemyVisualPool;
    public static EnemySpawnerBridge enemySpawnerInstance;

    [SerializeField] private PlayerCharacter playerCharacter;

    void Start()
    {
        enemySpawnerInstance = this;
        //Get ECS world
        var world = World.DefaultGameObjectInjectionWorld;
        entityManager = world.EntityManager;

        //Create pool for enemy visuals
        enemyVisualPool = new Pool(enemyVisualPrefab, spawnCount, poolParent);

        currentEnemySpawn = spawnCount;
        SpawnEnemies(spawnCount, enemyType);
    }

    void Update()
    {
        time += Time.deltaTime;
        fakerTimer += Time.deltaTime;
        if(fakerTimer > 15)
        {
            fakerTimer = 0;
            Debug.Log ("Spawning more enemies");
            fakerTimer = 0;
            currentEnemySpawn += 1;
            SpawnEnemies(currentEnemySpawn, enemyType);
        }
    }


    public void SpawnEnemies(int count, EnemyType enemyType)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnEnemy(enemyType);
        }
    }

public Entity SpawnEnemy(EnemyType enemyType)
{
    spawnCenter = playerCharacter.transform.position;
    
    //Calculate spawn position
    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * spawnRadius;
    randomOffset.y = 0;
    Vector3 spawnPos = spawnCenter + randomOffset;

    //Clamp to arena bounds
    if (arenaBounds != null)
    {
        Vector3 minBounds = arenaBounds.bounds.min;
        Vector3 maxBounds = arenaBounds.bounds.max;
        
        spawnPos.x = Mathf.Clamp(spawnPos.x, minBounds.x, maxBounds.x);
        spawnPos.z = Mathf.Clamp(spawnPos.z, minBounds.z, maxBounds.z);
        
        spawnPos.y = playerCharacter.transform.position.y;
    }
    else
    {
        spawnPos.y = playerCharacter.transform.position.y;
    }

    Entity enemyEntity = entityManager.CreateEntity();
    entityManager.AddComponentData(enemyEntity, LocalTransform.FromPosition(
        new float3(spawnPos.x, spawnPos.y, spawnPos.z)
    ));

    entityManager.AddBuffer<EnemyWaypoint>(enemyEntity);

    SetValueBasedOnEntity(enemyEntity, enemyType);

    //Get visual from pool
    GameObject enemyVisual = enemyVisualPool.Get();
    enemyVisual.transform.position = spawnPos; 
    enemyVisual.transform.rotation = Quaternion.identity;
    enemyVisual.name = $"Enemy_{enemyEntity.Index}";

    var enemySetup = enemyVisual.GetComponent<EnemySetup>();
    if (enemySetup != null)
    {
        enemySetup.SetEntity(enemyEntity, entityManager);
    }

    var navBridge = enemyVisual.GetComponent<EnemyNavMeshBridge>();
    if (navBridge != null)
    {
        entityManager.AddComponentObject(enemyEntity, navBridge);
    }

    return enemyEntity;
}
    
    private void SetValueBasedOnEntity(Entity entity, EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Ranged:
            entityManager.AddComponentData(entity, new EnemyMovementComponent
            {
                movementSpeed = 10f,
                stopDistance = 45f,
                verticalVelocity = 0f,
                stallTimer = 0f,
                lastDistanceSq = float.MaxValue,
                currentWaypointIndex = 0,
                hasPath = 0,
                pathPending = 0
            });

            entityManager.AddComponentData(entity, new EnemyHealthComponent
            {
                currentHealth = 60f,
                maxHealth = 60f,
                xpValue = 25 * xpMultipler,
                isDead = false
            });

            entityManager.AddComponentData(entity, new EnemyAttackComponent
            {
                attackRange = 50f,
                attackCooldown = 2,
                damage = 25,
                enemyType = EnemyType.Ranged,
                bulletSpeed = 20f,
                trailSpeed = 20f,
                timeSinceLastAttack = 0f
            });
                break;
            case EnemyType.Bomber:  
            /////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            entityManager.AddComponentData(entity, new EnemyMovementComponent
            {
                movementSpeed = 25f,
                stopDistance = 1f,
                verticalVelocity = 0f,
                stallTimer = 0f,
                lastDistanceSq = float.MaxValue,
                currentWaypointIndex = 0,
                hasPath = 0,
                pathPending = 0
            });

            entityManager.AddComponentData(entity, new EnemyHealthComponent
            {
                currentHealth = 20f,
                maxHealth = 20f,
                xpValue = 35 * xpMultipler,
                isDead = false
            });

            entityManager.AddComponentData(entity, new EnemyAttackComponent
            {
                attackRange = 5f,
                attackCooldown = 1.5f,
                damage = 70,
                enemyType = EnemyType.Bomber,
                bulletSpeed = 20f,
                trailSpeed = 20f,
                timeSinceLastAttack = 3f
            });
                break;
            case EnemyType.Melee:
            /////////////////////////////////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            entityManager.AddComponentData(entity, new EnemyMovementComponent
            {
                movementSpeed = 15f,
                stopDistance = 1f,
                verticalVelocity = 0f,
                stallTimer = 0f,
                lastDistanceSq = float.MaxValue,
                currentWaypointIndex = 0,
                hasPath = 0,
                pathPending = 0
            });

            entityManager.AddComponentData(entity, new EnemyHealthComponent
            {
                currentHealth = 150f,
                maxHealth = 150f,
                xpValue = 35 * xpMultipler,
                isDead = false
            });

            entityManager.AddComponentData(entity, new EnemyAttackComponent
            {
                attackRange = 1.5f,
                attackCooldown = 1,
                damage = 20,
                enemyType = EnemyType.Melee,
                bulletSpeed = 20f,
                trailSpeed = 20f,
                timeSinceLastAttack = 1f
            });
                break;
        }
    }

    //Debug 
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && PlayerCharacter.playerCharacterInstance != null)
        {
            spawnCenter = PlayerCharacter.playerCharacterInstance.transform.position;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);

        if (arenaBounds != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(arenaBounds.bounds.center, arenaBounds.bounds.size);
            
            Vector3 minSpawn = new Vector3(
                Mathf.Max(spawnCenter.x - spawnRadius, arenaBounds.bounds.min.x),
                arenaBounds.bounds.min.y,
                Mathf.Max(spawnCenter.z - spawnRadius, arenaBounds.bounds.min.z)
            );
                    
            Vector3 maxSpawn = new Vector3(
                Mathf.Min(spawnCenter.x + spawnRadius, arenaBounds.bounds.max.x),
                arenaBounds.bounds.max.y,
                Mathf.Min(spawnCenter.z + spawnRadius, arenaBounds.bounds.max.z)
            );
            
            Vector3 validCenter = (minSpawn + maxSpawn) / 2f;
            Vector3 validSize = maxSpawn - minSpawn;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(validCenter, validSize);
        }
    }
}