using Unity.Entities;
using UnityEngine;

public class BakeArenaPhysics : MonoBehaviour
{
    class Baker : Baker<BakeArenaPhysics>
    {
        public override void Bake(BakeArenaPhysics authoring)
        {
            // This will bake all PhysicsShape components in children
        }
    }
}