using System.Collections.Generic;
using UnityEngine;

public class BirdManager : MonoBehaviour
{
    public GameObject birdPrefab;
    public ProceduralMapGenerator map;
    public GameObject treePrefab;

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

            List<Tree> nearbyTrees = GetTreesNear(center, allTrees);

            if (nearbyTrees.Count >= requiredTreeCount)
            {
                SpawnBirdFlock(center, nearbyTrees);
                spawnedForestCenters.Add(center);
            }
        }
    }

    List<Tree> GetTreesNear(Vector3 center, Tree[] allTrees)
    {
        List<Tree> nearbyTrees = new List<Tree>();

        foreach (Tree tree in allTrees)
        {
            float distance = Vector3.Distance(center, tree.transform.position);

            if (distance <= forestCheckRadius)
            {
                nearbyTrees.Add(tree);
            }
        }

        return nearbyTrees;
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

    void SpawnBirdFlock(Vector3 forestCenter, List<Tree> forestTrees)
    {
        for (int i = 0; i < birdsPerForest; i++)
        {
            Tree randomTree = forestTrees[Random.Range(0, forestTrees.Count)];

            Vector2 offset = Random.insideUnitCircle * birdFlockRadius;

            Vector3 spawnPos =
                randomTree.transform.position +
                new Vector3(offset.x, offset.y, -0.5f);

            GameObject birdObj = Instantiate(
                birdPrefab,
                spawnPos,
                Quaternion.identity
            );

            Bird bird = birdObj.GetComponent<Bird>();
            bird.SetForestTrees(forestTrees);
            bird.map = map;
            bird.treePrefab = treePrefab;   
        }

        Debug.Log("ÄñÈº³öÏÖ");
    }
}
