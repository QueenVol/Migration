using UnityEngine;

public class Deer : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GrassManager grassManager;
    public GameObject deerPrefab;

    [Header("Movement")]
    public float moveSpeed = 0.35f;
    public float moveRadius = 1.4f;
    public float arriveDistance = 0.05f;

    [Header("Eating")]
    public float eatInterval = 2f;
    public float eatRadius = 0.18f;
    public float eatAmount = 8f;

    [Header("Life")]
    public float lifeTime = 90f;

    [Header("Reproduction")]
    public float reproduceInterval = 15f;
    public float reproduceChance = 0.35f;
    public float reproduceRadius = 0.35f;
    public float minGrassToReproduce = 30f;

    private Vector3 targetPosition;
    private float eatTimer;
    private float age;
    private float reproduceTimer;

    void Start()
    {
        PickNewTarget();
    }

    void Update()
    {
        UpdateLife();
        Move();
        EatGrass();
        Reproduce();

        if (Vector3.Distance(transform.position, targetPosition) < arriveDistance)
        {
            PickNewTarget();
        }
    }

    void UpdateLife()
    {
        age += Time.deltaTime;

        if (age >= lifeTime)
        {
            Destroy(gameObject);
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

    void Reproduce()
    {
        if (deerPrefab == null || grassManager == null)
            return;

        reproduceTimer += Time.deltaTime;

        if (reproduceTimer < reproduceInterval)
            return;

        reproduceTimer = 0f;

        if (Random.value > reproduceChance)
            return;

        if (grassManager.GetGrassAmount(transform.position) < minGrassToReproduce)
            return;

        Vector2 offset = Random.insideUnitCircle * reproduceRadius;

        Vector3 spawnPos = new Vector3(
            transform.position.x + offset.x,
            transform.position.y + offset.y,
            -0.6f
        );

        if (!IsGrass(spawnPos))
            return;

        GameObject baby = Instantiate(deerPrefab, spawnPos, Quaternion.identity);

        Deer babyDeer = baby.GetComponent<Deer>();
        babyDeer.map = map;
        babyDeer.grassManager = grassManager;
        babyDeer.deerPrefab = deerPrefab;

        Debug.Log("Â¹·±Ö³");
    }

    void PickNewTarget()
    {
        for (int attempt = 0; attempt < 60; attempt++)
        {
            Vector2 offset = Random.insideUnitCircle * moveRadius;
            Vector3 candidate = transform.position + new Vector3(offset.x, offset.y, 0);

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
        if (grassManager != null)
            return grassManager.IsUsableGrass(worldPos);

        Vector2Int mapPos = map.WorldToMap(worldPos);

        return map.GetTerrain(mapPos.x, mapPos.y)
            == ProceduralMapGenerator.TerrainType.Grass;
    }
}
