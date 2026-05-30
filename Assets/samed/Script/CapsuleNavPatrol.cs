using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CapsuleNavPatrol : MonoBehaviour
{
    [Header("İki nokta (opsiyonel)")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Nokta yoksa — başlangıçtan ileri-geri")]
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private bool useLocalForward = true;

    [Header("Hareket")]
    [SerializeField] private float waitAtPoint = 0.5f;
    [SerializeField] private float arriveDistance = 0.4f;

    private NavMeshAgent _agent;
    private Vector3 _posA;
    private Vector3 _posB;
    private bool _goingToB;
    private float _waitTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = arriveDistance;
    }

    private void Start()
    {
        SetupPoints();
        _goingToB = true;
        GoToCurrentTarget();
    }

    private void Update()
    {
        if (_agent.pathPending) return;
        if (_agent.remainingDistance > _agent.stoppingDistance) return;

        _waitTimer += Time.deltaTime;
        if (_waitTimer < waitAtPoint) return;

        _waitTimer = 0f;
        _goingToB = !_goingToB;
        GoToCurrentTarget();
    }

    private void SetupPoints()
    {
        if (pointA != null && pointB != null)
        {
            _posA = pointA.position;
            _posB = pointB.position;
            return;
        }

        Vector3 start = transform.position;
        Vector3 dir = useLocalForward ? transform.forward : Vector3.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            dir = Vector3.forward;

        dir.Normalize();
        _posA = start;
        _posB = start + dir * patrolDistance;
    }

    private void GoToCurrentTarget()
    {
        Vector3 target = _goingToB ? _posB : _posA;

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
        else
            _agent.SetDestination(target);
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.25f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.25f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            return;
        }

        Vector3 a = Application.isPlaying ? _posA : transform.position;
        Vector3 dir = useLocalForward ? transform.forward : Vector3.forward;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        Vector3 b = a + dir.normalized * patrolDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(a, 0.25f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(b, 0.25f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
    }
}
