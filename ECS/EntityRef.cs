using Unity.Entities;
using UnityEngine;

public class EntityRef : MonoBehaviour
{
    public Entity entity;
    public EntityManager entityManager;
    
    public void SetEntity(Entity e, EntityManager em)
    {
        entity = e;
        entityManager = em;
    }
}