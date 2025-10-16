using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshBridge : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public bool TryRequestPath(Vector3 target, NavMeshPath path)
    {
        agent.enabled = true;
        bool ok = agent.CalculatePath(target, path) && path.status == NavMeshPathStatus.PathComplete;
        agent.enabled = false;
        return ok;
    }
}
