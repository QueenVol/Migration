using UnityEngine;

public class DeerManager : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GrassManager grassManager;
    public GameObject deerPrefab;

    [Header("Spawn Settings")]
    public int startingDeerCount = 8;
    public float minDeerDistance = 0.35f;

    [Header("Habitat Requirement")]
    public int waterSearchRadius = 35;

    private void Start()
    {
        StartCoroutine(SpawnDeerNextFrame());
    }

    private System.Collections.IEnumerator SpawnDeerNextFrame()
    {
        yield return null;
        yield return null;
        SpawnInitialDeer();
    }

    void SpawnInitialDeer()
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = startingDeerCount * 300;

        while (spawned < startingDeerCount && attempts < maxAttempts)
        {
            attempts++;

            int mapX = Random.Range(0, map.Width);
            int mapY = Random.Range(0, map.Height);

            if (!IsGoodDeerHabitat(mapX, mapY))
                continue;

            Vector3 worldPos = map.MapToWorld(mapX, mapY);

            if (HasNearbyDeer(worldPos))
                continue;

            GameObject deerObj = Instantiate(
                deerPrefab,
                new Vector3(worldPos.x, worldPos.y, -0.6f),
                Quaternion.identity
            );

            Deer deer = deerObj.GetComponent<Deer>();
            deer.map = map;
            deer.grassManager = grassManager;

            spawned++;
        }

        Debug.Log("生成鹿数量：" + spawned);
    }

    bool IsGoodDeerHabitat(int mapX, int mapY)
    {
        if (map.GetTerrain(mapX, mapY) != ProceduralMapGenerator.TerrainType.Grass)
            return false;

        return HasWaterNearby(mapX, mapY);
    }

    bool HasWaterNearby(int mapX, int mapY)
    {
        for (int x = mapX - waterSearchRadius; x <= mapX + waterSearchRadius; x++)
        {
            for (int y = mapY - waterSearchRadius; y <= mapY + waterSearchRadius; y++)
            {
                if (map.GetTerrain(x, y) == ProceduralMapGenerator.TerrainType.River)
                    return true;
            }
        }

        return false;
    }

    bool HasNearbyDeer(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(worldPos, minDeerDistance);
        return hit != null && hit.GetComponent<Deer>() != null;
    }
}
