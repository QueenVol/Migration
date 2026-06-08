using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public GrassManager grassManager;

    [Header("Movement")]
    public float moveSpeed = 0.6f;
    public float arriveDistance = 0.03f;
    public float targetOffsetRadius = 0.15f;

    [Header("Life")]
    public float lifeTime = 60f;

    [Header("Reproduction")]
    public GameObject birdPrefab;
    public float reproduceInterval = 12f;
    public float reproduceChance = 0.35f;
    public int minTreesToReproduce = 30;

    [Header("Forest Detection")]
    public float forestSearchRadius = 2.0f;

    [Header("Forest Survival")]
    public int minTreesToSurvive = 10;
    public float forestDeathCheckInterval = 2f;
    public float survivalGraceTime = 5f;

    [Header("Seeding")]
    public GameObject treePrefab;
    public ProceduralMapGenerator map;
    public float seedInterval = 8f;
    public float seedChance = 0.45f;
    public float seedRadius = 0.8f;

    private float seedTimer;

    private float age;
    private float reproduceTimer;

    private List<Tree> forestTrees = new List<Tree>();
    private Vector3 targetPosition;

    private float forestDeathCheckTimer;
    private Vector3 homeForestCenter;
    private bool hasHomeForest;

    public void SetForestTrees(List<Tree> trees)
    {
        forestTrees = trees;

        if (trees != null && trees.Count > 0)
        {
            homeForestCenter = GetForestCenter(trees);
            hasHomeForest = true;
        }

        PickNewTarget();
    }

    void Update()
    {
        UpdateLife();
        UpdateMovement();
        UpdateReproduction();
        UpdateSeeding();
        UpdateForestSurvival();
    }

    void UpdateLife()
    {
        age += Time.deltaTime;

        if (age >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void UpdateMovement()
    {
        if (forestTrees == null || forestTrees.Count == 0)
        {
            RefreshNearbyTrees();

            if (forestTrees == null || forestTrees.Count == 0)
                return;

            PickNewTarget();
        }

        Vector3 nextPos = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        nextPos.z = -0.5f;
        transform.position = nextPos;

        if (Vector3.Distance(transform.position, targetPosition) <= arriveDistance)
        {
            PickNewTarget();
        }
    }

    void UpdateReproduction()
    {
        if (birdPrefab == null)
            return;

        if (forestTrees == null || forestTrees.Count < minTreesToReproduce)
            return;

        reproduceTimer += Time.deltaTime;

        if (reproduceTimer < reproduceInterval)
            return;

        reproduceTimer = 0f;

        if (Random.value > reproduceChance)
            return;

        int nearbyBirds = CountNearbyBirds();

        SpawnBabyBird();
    }

    int CountNearbyBirds()
    {
        int count = 0;
        Bird[] birds = FindObjectsOfType<Bird>();

        foreach (Bird bird in birds)
        {
            if (Vector3.Distance(transform.position, bird.transform.position) <= 1.8f)
            {
                count++;
            }
        }

        return count;
    }

    void SpawnBabyBird()
    {
        Vector2 offset = Random.insideUnitCircle * 0.15f;

        Vector3 spawnPos = new Vector3(
            transform.position.x + offset.x,
            transform.position.y + offset.y,
            -0.5f
        );

        GameObject baby = Instantiate(
            birdPrefab,
            spawnPos,
            Quaternion.identity
        );

        Bird babyBird = baby.GetComponent<Bird>();
        babyBird.SetForestTrees(forestTrees);
        babyBird.birdPrefab = birdPrefab;
        babyBird.treePrefab = treePrefab;
        babyBird.map = map;
        babyBird.grassManager = grassManager;
        babyBird.homeForestCenter = homeForestCenter;
        babyBird.hasHomeForest = hasHomeForest;
    }

    void PickNewTarget()
    {
        RefreshNearbyTrees();

        if (forestTrees == null || forestTrees.Count == 0)
        {
            targetPosition = transform.position;
            return;
        }

        Tree randomTree = forestTrees[Random.Range(0, forestTrees.Count)];

        if (randomTree == null)
        {
            targetPosition = transform.position;
            return;
        }

        Vector2 offset = Random.insideUnitCircle * targetOffsetRadius;

        targetPosition = new Vector3(
            randomTree.transform.position.x + offset.x,
            randomTree.transform.position.y + offset.y,
            -0.5f
        );
    }

    void RefreshNearbyTrees()
    {
        Tree[] allTrees = FindObjectsOfType<Tree>();
        List<Tree> nearbyTrees = new List<Tree>();

        Vector3 searchCenter = hasHomeForest ? homeForestCenter : transform.position;

        foreach (Tree tree in allTrees)
        {
            if (tree == null)
                continue;

            float distance = Vector3.Distance(
                searchCenter,
                tree.transform.position
            );

            if (distance <= forestSearchRadius)
            {
                nearbyTrees.Add(tree);
            }
        }

        forestTrees = nearbyTrees;

        if (forestTrees.Count > 0)
        {
            homeForestCenter = GetForestCenter(forestTrees);
            hasHomeForest = true;
        }
    }

    void UpdateSeeding()
    {
        if (treePrefab == null || map == null)
            return;

        seedTimer += Time.deltaTime;

        if (seedTimer < seedInterval)
            return;

        seedTimer = 0f;

        if (Random.value > seedChance)
            return;

        TryPlantSeed();
    }

    void UpdateForestSurvival()
    {
        if (age < survivalGraceTime)
            return;

        forestDeathCheckTimer += Time.deltaTime;

        if (forestDeathCheckTimer < forestDeathCheckInterval)
            return;

        forestDeathCheckTimer = 0f;

        RefreshNearbyTrees();

        Debug.Log("鸟附近树数量：" + forestTrees.Count);

        if (forestTrees == null || forestTrees.Count < minTreesToSurvive)
        {
            Destroy(gameObject);
        }
    }

    void TryPlantSeed()
    {
        int attempts = 30;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * seedRadius;

            Vector3 seedPos = new Vector3(
                transform.position.x + offset.x,
                transform.position.y + offset.y,
                -1f
            );

            if (!IsGrass(seedPos))
                continue;

            if (HasNearbyTree(seedPos))
                continue;

            GameObject treeObj = Instantiate(
                treePrefab,
                seedPos,
                Quaternion.identity
            );

            Tree tree = treeObj.GetComponent<Tree>();
            tree.grassManager = grassManager;

            Debug.Log("���֣���������");
            return;
        }
    }

    bool IsGrass(Vector3 worldPos)
    {
        int mapX = Mathf.RoundToInt(worldPos.x * 32 + map.Width / 2);
        int mapY = Mathf.RoundToInt(worldPos.y * 32 + map.Height / 2);

        return map.GetTerrain(mapX, mapY) == ProceduralMapGenerator.TerrainType.Grass;
    }

    bool HasNearbyTree(Vector3 pos)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, 0.18f);
        return hit != null;
    }

    Vector3 GetForestCenter(List<Tree> trees)
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (Tree tree in trees)
        {
            if (tree == null)
                continue;

            sum += tree.transform.position;
            count++;
        }

        if (count == 0)
            return transform.position;

        return sum / count;
    }
}
