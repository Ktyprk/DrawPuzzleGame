using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TargetPoint targetPoint;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private LevelCollectible[] collectibles;

    public Transform PlayerSpawnPoint => playerSpawnPoint;
    public int TotalCollectibles => collectibles.Length;
    public int CollectedCount { get; private set; }
    public bool AllCollected => CollectedCount >= TotalCollectibles;

    private GameFlowController gameFlow;

    public void Setup(GameFlowController flow)
    {
        gameFlow = flow;

        if (targetPoint != null)
            targetPoint.Setup(this, gameFlow);

        foreach (var c in collectibles)
        {
            if (c != null)
                c.Setup(this);
        }
    }

    public void ResetLevel()
    {
        CollectedCount = 0;

        if (targetPoint != null)
            targetPoint.ResetTarget();

        foreach (var c in collectibles)
        {
            if (c != null)
                c.ResetCollectible();
        }
    }

    public void NotifyCollected()
    {
        CollectedCount++;
    }
}