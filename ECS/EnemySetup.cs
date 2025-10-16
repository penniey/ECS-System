using UnityEngine;
using Unity.Entities;

public class EnemySetup : MonoBehaviour
{
    private EntityRef entityRef;
    
    void Awake()
    {
        entityRef = GetComponentInChildren<EntityRef>();
        
        if (entityRef == null)
        {
            Debug.LogError("EntityRef component not found! Make sure the hitbox child has an EntityRef component.");
        }
    }
    
    void OnDisable()
    {
        if (entityRef != null)
        {
            entityRef.entity = default(Entity);
        }
    }
    
    public void SetEntity(Entity entity, EntityManager entityManager)
    {
        if (entityRef != null)
        {
            entityRef.SetEntity(entity, entityManager);
            Debug.Log($"Linked GameObject '{gameObject.name}' to Entity {entity.Index}");
        }
        else
        {
            Debug.LogError("EntityRef is null!");
        }
    }
}