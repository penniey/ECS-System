using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

public class AddPhysicsToWalls : MonoBehaviour
{
    [ContextMenu("Add Physics Components to All Walls")]
    void AddPhysicsComponents()
    {
        UnityEngine.Collider[] colliders = FindObjectsByType<UnityEngine.Collider>(FindObjectsSortMode.None);
        
        foreach (var col in colliders)
        {
            if (col.gameObject.layer == 6 || col.gameObject.layer == 8 || col.gameObject.layer == 9) // Your wall layers
            {
                if (col.GetComponent<PhysicsShapeAuthoring>() == null)
                {
                    var shape = col.gameObject.AddComponent<PhysicsShapeAuthoring>();
                    Debug.Log($"Added PhysicsShape to {col.gameObject.name}");
                }
                
                if (col.GetComponent<PhysicsBodyAuthoring>() == null)
                {
                    var body = col.gameObject.AddComponent<PhysicsBodyAuthoring>();
                    body.MotionType = BodyMotionType.Static;
                    Debug.Log($"Added PhysicsBody to {col.gameObject.name}");
                }
            }
        }
    }
}