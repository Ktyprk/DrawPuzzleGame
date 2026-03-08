using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    private LevelController levelController;
    private GameFlowController gameFlow;

    private bool triggered;

    public void Setup(LevelController level, GameFlowController flow)
    {
        levelController = level;
        gameFlow = flow;
        triggered = false;
    }

    public void ResetTarget()
    {
        triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        if (gameFlow != null && levelController != null)
            gameFlow.OnPlayerReachedTarget(levelController);
    }
}