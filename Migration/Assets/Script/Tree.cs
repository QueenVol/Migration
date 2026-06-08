using UnityEngine;

public class Tree : MonoBehaviour
{
    public GrassManager grassManager;

    [Header("Tree Survival")]
    public float deathDelay = 15f;
    public float checkRadius = 0.35f;
    public int requiredGrassPixels = 20;

    private float noGrassTimer;

    void Update()
    {
        CheckGrassSupport();
    }

    void CheckGrassSupport()
    {
        if (grassManager == null)
            return;

        bool hasEnoughGrass =
            grassManager.HasEnoughGrassNearby(
                transform.position,
                checkRadius,
                requiredGrassPixels
            );

        if (hasEnoughGrass)
        {
            noGrassTimer = 0f;
            return;
        }

        noGrassTimer += Time.deltaTime;

        if (noGrassTimer >= deathDelay)
        {
            Destroy(gameObject);
        }
    }
}
