using System.Collections.Generic;
using UnityEngine;

public class DeerManager : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GrassManager grassManager;
    public GameObject deerPrefab;

    [Header("Spawn Check")]
    public float spawnCheckInterval = 3f;
    public int grassSearchRadius = 80;
    public int requiredGrassPixels = 30;

    [Header("Herd Spawn")]
    public int deerPerHerd = 5;
    public float herdSpawnRadius = 0.5f;
    public float minDeerDistance = 0.25f;

    [Header("Habitat Requirement")]
    public int closeGrassRadius = 12;


    private float timer;
    private List<Vector3> spawnedWaterCenters = new List<Vector3>();

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnCheckInterval)
        {
            timer = 0f;
            CheckWaterAndSpawnHerd();
        }
    }

    void CheckWaterAndSpawnHerd()
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            int mapX = Random.Range(0, map.Width);
            int mapY = Random.Range(0, map.Height);

            if (map.GetTerrain(mapX, mapY) != ProceduralMapGenerator.TerrainType.River)
                continue;

            Vector3 waterWorldPos = map.MapToWorld(mapX, mapY);

            if (AlreadySpawnedNear(waterWorldPos))
                continue;

            if (!HasCloseGrassAroundWater(mapX, mapY))
                continue;

            List<Vector2Int> grassPositions = GetGrassNearWater(mapX, mapY);

            if (grassPositions.Count < requiredGrassPixels)
                continue;

            SpawnDeerHerd(grassPositions, waterWorldPos);
            spawnedWaterCenters.Add(waterWorldPos);
            return;
        }
    }

    bool HasCloseGrassAroundWater(int waterX, int waterY)
    {
        for (int x = waterX - closeGrassRadius; x <= waterX + closeGrassRadius; x++)
        {
            for (int y = waterY - closeGrassRadius; y <= waterY + closeGrassRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                Vector3 worldPos = map.MapToWorld(x, y);

                if (grassManager != null && !grassManager.IsUsableGrass(worldPos))
                    continue;

                return true;
            }
        }

        return false;
    }

    List<Vector2Int> GetGrassNearWater(int waterX, int waterY)
    {
        List<Vector2Int> grassPositions = new List<Vector2Int>();

        for (int x = waterX - grassSearchRadius; x <= waterX + grassSearchRadius; x++)
        {
            for (int y = waterY - grassSearchRadius; y <= waterY + grassSearchRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                Vector3 worldPos = map.MapToWorld(x, y);

                if (grassManager != null && !grassManager.IsUsableGrass(worldPos))
                    continue;

                grassPositions.Add(new Vector2Int(x, y));
            }
        }

        return grassPositions;
    }

    bool AlreadySpawnedNear(Vector3 waterPos)
    {
        foreach (Vector3 center in spawnedWaterCenters)
        {
            if (Vector3.Distance(center, waterPos) <= 1.5f)
                return true;
        }

        return false;
    }

    void SpawnDeerHerd(List<Vector2Int> grassPositions, Vector3 waterWorldPos)
    {
        int spawned = 0;
        int attempts = deerPerHerd * 30;

        while (spawned < deerPerHerd && attempts > 0)
        {
            attempts--;

            Vector2Int grassPos = grassPositions[Random.Range(0, grassPositions.Count)];
            Vector3 baseWorldPos = map.MapToWorld(grassPos.x, grassPos.y);

            Vector2 offset = Random.insideUnitCircle * herdSpawnRadius;
            Vector3 spawnPos = new Vector3(
                baseWorldPos.x + offset.x,
                baseWorldPos.y + offset.y,
                -0.6f
            );

            if (!IsHealthyGrass(spawnPos))
                continue;

            if (HasNearbyDeer(spawnPos))
                continue;

            GameObject deerObj = Instantiate(deerPrefab, spawnPos, Quaternion.identity);

            Deer deer = deerObj.GetComponent<Deer>();
            deer.map = map;
            deer.grassManager = grassManager;
            deer.deerPrefab = deerPrefab;

            spawned++;
        }

        Debug.Log($"水源附近草地达标，生成鹿群：{spawned}");
    }

    bool IsHealthyGrass(Vector3 worldPos)
    {
        if (grassManager != null)
            return grassManager.IsUsableGrass(worldPos);

        Vector2Int mapPos = map.WorldToMap(worldPos);
        return map.GetTerrain(mapPos.x, mapPos.y) == ProceduralMapGenerator.TerrainType.Grass;
    }

    bool HasNearbyDeer(Vector3 worldPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(worldPos, minDeerDistance);
        return hit != null && hit.GetComponent<Deer>() != null;
    }
}
