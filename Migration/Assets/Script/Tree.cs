using UnityEngine;

public class Tree : MonoBehaviour
{
    public GrassManager grassManager;

    [Header("Life")]
    public float deathDelay = 12f;

    [Header("Grass Requirement")]
    public float grassCheckRadius = 0.35f;
    public int requiredGrassPixels = 20;

    private float noGrassTimer;

    void Update()
    {
        if (grassManager == null)
        {
            Debug.LogWarning("这棵树没有 GrassManager：" + gameObject.name);
            return;
        }

        bool hasEnoughGrass = grassManager.HasEnoughGrassNearby(
            transform.position,
            grassCheckRadius,
            requiredGrassPixels
        );

        if (hasEnoughGrass)
        {
            noGrassTimer = 0f;
        }
        else
        {
            noGrassTimer += Time.deltaTime;

            if (noGrassTimer >= deathDelay)
            {
                Debug.Log("树因为周围没有足够草而死亡");
                Destroy(gameObject);
            }
        }
    }
}
