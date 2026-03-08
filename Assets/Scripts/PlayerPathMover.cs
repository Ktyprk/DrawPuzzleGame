using UnityEngine;

public class PlayerPathMover : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private InputPathDrawer pathDrawer;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float reachDistance = 0.1f;
    [SerializeField] private bool rotateToMoveDirection = true;
    [SerializeField] private float rotationSpeed = 12f;

    private int currentPointIndex;
    private bool canMove;

    public bool CanMove => canMove;

    public void PreparePath()
    {
        currentPointIndex = 0;
        canMove = pathDrawer != null && pathDrawer.GetPointCount() > 0;
    }

    public void StopMove()
    {
        canMove = false;
    }

    public void TickMove(bool holdInput)
    {
        if (!canMove) return;
        if (!holdInput) return;
        if (pathDrawer == null) return;

        int pointCount = pathDrawer.GetPointCount();
        if (pointCount == 0)
        {
            canMove = false;
            return;
        }

        if (currentPointIndex >= pointCount)
        {
            canMove = false;
            return;
        }
        
          

        Vector3 targetPoint = pathDrawer.GetPoint(currentPointIndex);
        Vector3 moveTarget = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);

        Vector3 moveDir = (moveTarget - transform.position);
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude > 0.0001f && rotateToMoveDirection)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            moveTarget,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, moveTarget) <= reachDistance)
        {
            currentPointIndex++;

            if (currentPointIndex >= pointCount)
            {
                canMove = false;
            }
        }
    }
}