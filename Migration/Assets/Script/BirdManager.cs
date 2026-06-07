using System.Collections.Generic;
using UnityEngine;

public class BirdManager : MonoBehaviour
{
    public GameObject birdPrefab;

    [Header("Forest Requirement")]
    public int requiredTreeCount = 8;
    public float forestCheckRadius = 1.5f;
    public float checkInterval = 2f;

    [Header("Bird Spawn")]
    public float birdSpawnOffset = 0.3f;
    public int birdsPerForest = 5;
    public float birdFlockRadius = 0.25f;

    private float timer;
    private List<Vector3> spawnedForestCenters = new List<Vector3>();

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            timer = 0f;
            CheckForestsAndSpawnBirds();
        }
    }

    void CheckForestsAndSpawnBirds()
    {
        Tree[] allTrees = FindObjectsOfType<Tree>();

        foreach (Tree tree in allTrees)
        {
            Vector3 center = tree.transform.position;

            if (AlreadySpawnedNear(center))
                continue;

            int nearbyTreeCount = CountTreesNear(center, allTrees);

            if (nearbyTreeCount >= requiredTreeCount)
            {
                SpawnBirdFlock(center);
                spawnedForestCenters.Add(center);
            }
        }
    }

    int CountTreesNear(Vector3 center, Tree[] allTrees)
    {
        int count = 0;

        foreach (Tree tree in allTrees)
        {
            float distance = Vector3.Distance(center, tree.transform.position);

            if (distance <= forestCheckRadius)
            {
                count++;
            }
        }

        return count;
    }

    bool AlreadySpawnedNear(Vector3 position)
    {
        foreach (Vector3 center in spawnedForestCenters)
        {
            if (Vector3.Distance(center, position) <= forestCheckRadius)
            {
                return true;
            }
        }

        return false;
    }

    void SpawnBirdFlock(Vector3 forestCenter)
    {
        for (int i = 0; i < birdsPerForest; i++)
        {
            Vector2 offset =
                Random.insideUnitCircle * birdFlockRadius;

            Vector3 spawnPos =
                forestCenter +
                new Vector3(
                    offset.x,
                    offset.y,
                    -0.5f
                );

            Instantiate(
                birdPrefab,
                spawnPos,
                Quaternion.identity
            );
        }

        Debug.Log("ÄñÈº³öÏÖ");
    }
}
