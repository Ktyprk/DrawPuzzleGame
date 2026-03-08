using System.Collections.Generic;
using UnityEngine;

public class InputPathDrawer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform playerTransform;

    [Header("Settings")]
    [SerializeField] private float minPointDistance = 0.2f;
    [SerializeField] private float lineYOffset = 0.05f;
    [SerializeField] private float maxStartDistanceFromPlayer = 1.5f;

    private readonly List<Vector3> pathPoints = new List<Vector3>();
    private bool isDrawing;

    public IReadOnlyList<Vector3> PathPoints => pathPoints;
    public bool IsDrawing => isDrawing;
    
    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public bool BeginDraw()
    {
        isDrawing = false;
        pathPoints.Clear();

        if (playerTransform == null)
        {
            UpdateLine();
            return false;
        }

        if (!TryGetGroundPoint(out Vector3 point))
        {
            UpdateLine();
            return false;
        }

        Vector3 playerPoint = playerTransform.position;
        playerPoint.y = 0f;

        float startDistance = Vector3.Distance(playerPoint, point);
        if (startDistance > maxStartDistanceFromPlayer)
        {
            UpdateLine();
            return false;
        }

        isDrawing = true;

        pathPoints.Add(playerPoint);

        float distanceToFirstHit = Vector3.Distance(playerPoint, point);
        if (distanceToFirstHit >= minPointDistance)
        {
            pathPoints.Add(point);
        }

        UpdateLine();
        return true;
    }

    public void DrawStep()
    {
        if (!isDrawing) return;
        if (!TryGetGroundPoint(out Vector3 point)) return;

        if (pathPoints.Count == 0)
        {
            pathPoints.Add(point);
            UpdateLine();
            return;
        }

        float distance = Vector3.Distance(pathPoints[pathPoints.Count - 1], point);
        if (distance >= minPointDistance)
        {
            pathPoints.Add(point);
            UpdateLine();
        }
    }

    public void EndDraw()
    {
        isDrawing = false;
        UpdateLine();
    }

    public void ClearPath()
    {
        isDrawing = false;
        pathPoints.Clear();
        UpdateLine();
    }

    public Vector3 GetPoint(int index)
    {
        if (index < 0 || index >= pathPoints.Count) return Vector3.zero;
        return pathPoints[index];
    }

    public int GetPointCount()
    {
        return pathPoints.Count;
    }

    private bool TryGetGroundPoint(out Vector3 point)
    {
        point = Vector3.zero;

        if (mainCamera == null) return false;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            point = hit.point;
            point.y = 0f;
            return true;
        }

        return false;
    }

    private void UpdateLine()
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = pathPoints.Count;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            lineRenderer.SetPosition(i, pathPoints[i] + Vector3.up * lineYOffset);
        }
    }
}