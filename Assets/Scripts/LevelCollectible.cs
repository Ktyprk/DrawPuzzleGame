using UnityEngine;

public class LevelCollectible : MonoBehaviour
{
    private LevelController level;
    public bool collected;

    [Header("Follower Spawn")]
    [SerializeField] private GameObject followerPrefab;
    [SerializeField] private Vector3 localSpawnOffset = new Vector3(0f, -1f, -1.5f);

    private Transform player;

    public void Setup(LevelController controller)
    {
        level = controller;
        collected = false;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
        }
    }

    public void ResetCollectible()
    {
        collected = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        if (level != null)
            level.NotifyCollected();

        SpawnFollower();

        gameObject.SetActive(false);
    }

    private void SpawnFollower()
    {
        if (followerPrefab == null || player == null) return;

        GameObject follower = Instantiate(followerPrefab, player);
        follower.transform.localPosition = localSpawnOffset;
        follower.transform.localRotation = Quaternion.identity;
    }
}