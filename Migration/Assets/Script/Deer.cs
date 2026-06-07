using UnityEngine;

public class Deer : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GrassManager grassManager;

    [Header("Movement")]
    public float moveSpeed = 0.35f;
    public float moveRadius = 1.4f;
    public float arriveDistance = 0.05f;

    [Header("Eating")]
    public float eatInterval = 2f;
    public float eatRadius = 0.18f;
    public float eatAmount = 8f;

    private Vector3 targetPosition;
    private float eatTimer;

    void Start()
    {
        PickNewTarget();
    }

    void Update()
    {
        Move();
        EatGrass();

        if (Vector3.Distance(transform.position, targetPosition) < arriveDistance)
        {
            PickNewTarget();
        }
    }

    void Move()
    {
        Vector3 nextPos = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        nextPos.z = -0.6f;
        transform.position = nextPos;
    }

    void EatGrass()
    {
        if (grassManager == null)
            return;

        eatTimer += Time.deltaTime;

        if (eatTimer < eatInterval)
            return;

        eatTimer = 0f;

        bool ate = grassManager.EatGrass(transform.position, eatRadius, eatAmount);

        if (!ate)
        {
            PickNewTarget();
        }
    }

    void PickNewTarget()
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * moveRadius;

            Vector3 candidate =
                transform.position +
                new Vector3(offset.x, offset.y, 0);

            if (!IsGrass(candidate))
                continue;

            if (grassManager != null && grassManager.GetGrassAmount(candidate) <= 5f)
                continue;

            targetPosition = candidate;
            return;
        }

        targetPosition = transform.position;
    }

    bool IsGrass(Vector3 worldPos)
    {
        Vector2Int mapPos = map.WorldToMap(worldPos);

        return map.GetTerrain(mapPos.x, mapPos.y)
            == ProceduralMapGenerator.TerrainType.Grass;
    }
}
