using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// NavMesh steering API for behavior trees. Call <see cref="MoveTo"/>, <see cref="Stop"/>,
/// and <see cref="GetDistanceToTarget"/> from BT nodes instead of driving the agent in <c>Update</c>.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Tooltip("If > 0, applied to the agent on Awake. Otherwise use the value already set on NavMeshAgent.")]
    [SerializeField] float _stoppingDistance = 1.75f;

    [Header("Debug gizmos")]
    [SerializeField] bool _drawGizmos = true;
    [SerializeField] Color _movementGizmoColor = new Color(0f, 0.85f, 1f, 0.95f);
    [SerializeField] Color _stoppingGizmoColor = new Color(1f, 0.75f, 0.15f, 0.95f);

    NavMeshAgent _agent;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_stoppingDistance > 0f)
            _agent.stoppingDistance = _stoppingDistance;
    }

    public void MoveTo(Vector3 position)
    {
        _agent.isStopped = false;
        _agent.SetDestination(position);
    }

    public void Stop()
    {
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    /// <summary>Horizontal distance on XZ to <paramref name="target"/> (ignores Y).</summary>
    public float GetDistanceToTarget(Transform target)
    {
        if (target == null)
            return float.PositiveInfinity;
        return Mathf.Sqrt(HorizontalDistanceSqr(transform.position, target.position));
    }

    /// <summary>Horizontal distance on XZ to a world position (ignores Y).</summary>
    public float GetDistanceToTarget(Vector3 worldPosition)
    {
        return Mathf.Sqrt(HorizontalDistanceSqr(transform.position, worldPosition));
    }

    static float HorizontalDistanceSqr(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    static float ResolveStoppingDistance(float serialized, NavMeshAgent agent)
    {
        if (serialized > 0f)
            return serialized;
        if (agent != null)
            return Mathf.Max(agent.stoppingDistance, 0.01f);
        return 1.75f;
    }

    void OnDrawGizmos()
    {
        if (!_drawGizmos)
            return;

        if (!TryGetComponent(out NavMeshAgent agent))
            return;

        float stopR = ResolveStoppingDistance(_stoppingDistance, agent);

        bool navigating = Application.isPlaying && agent.enabled && !agent.isStopped
            && (agent.hasPath || agent.pathPending);

        if (navigating)
        {
            Vector3 dest = agent.destination;
            Gizmos.color = _movementGizmoColor;
            Gizmos.DrawLine(transform.position, dest);

            Gizmos.color = _stoppingGizmoColor;
            Gizmos.DrawWireSphere(dest, stopR);
        }
        else
        {
            Vector3 p = transform.position;
            Gizmos.color = _stoppingGizmoColor;
            DrawHorizontalWireCircle(p, stopR, 32);
        }
    }

    static void DrawHorizontalWireCircle(Vector3 center, float radius, int segments)
    {
        if (segments < 3 || radius <= 0f)
            return;

        float step = Mathf.PI * 2f / segments;
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = step * i;
            Vector3 next = center + new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}
