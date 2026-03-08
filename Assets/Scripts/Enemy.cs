using UnityEngine;

public class Enemy: MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform eyePoint;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 6f;
    [SerializeField, Range(1f, 180f)] private float viewAngle = 60f;

    [Header("Look Angles")]
    [SerializeField] private float minAngle = -60f;
    [SerializeField] private float maxAngle = 60f;
    [SerializeField] private float lookSpeed = 2f;

    private Transform player;
    private GameFlowController gameFlow;
    private bool detected;

    private void Awake()
    {
        ResolveSceneRefs();
    }

    private void OnEnable()
    {
        detected = false;
        ResolveSceneRefs();
    }

    private void Update()
    {
        if (detected) return;

        if (eyePoint == null)
            eyePoint = transform;

        if (player == null || gameFlow == null)
            ResolveSceneRefs();

        if (player == null || gameFlow == null)
            return;

        UpdateLookRotation();
        CheckPlayerInVision();
    }

    private void ResolveSceneRefs()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (gameFlow == null)
        {
            gameFlow = FindObjectOfType<GameFlowController>();
        }
    }

    private void UpdateLookRotation()
    {
        float t = (Mathf.Sin(Time.time * lookSpeed) + 1f) * 0.5f;
        float angle = Mathf.Lerp(minAngle, maxAngle, t);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void CheckPlayerInVision()
    {
        Vector3 origin = eyePoint.position;
        Vector3 targetPos = player.position;
        Vector3 dir = targetPos - origin;
        dir.y = 0f;

        float distance = dir.magnitude;
        if (distance > viewDistance) return;

        Vector3 forward = eyePoint.forward;
        forward.y = 0f;

        float angle = Vector3.Angle(forward, dir.normalized);
        if (angle > viewAngle * 0.5f) return;

        Vector3 rayDir = (targetPos - origin).normalized;

        if (Physics.Raycast(origin, rayDir, out RaycastHit hit, viewDistance))
        {
            if (hit.transform.CompareTag("Player"))
            {
                detected = true;
                gameFlow.FailFromEnemy();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform originTf = eyePoint != null ? eyePoint : transform;
        Vector3 origin = originTf.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, viewDistance);

        Vector3 leftDir = Quaternion.Euler(0f, -viewAngle * 0.5f, 0f) * originTf.forward;
        Vector3 rightDir = Quaternion.Euler(0f, viewAngle * 0.5f, 0f) * originTf.forward;

        Gizmos.DrawLine(origin, origin + leftDir * viewDistance);
        Gizmos.DrawLine(origin, origin + rightDir * viewDistance);
    }
}