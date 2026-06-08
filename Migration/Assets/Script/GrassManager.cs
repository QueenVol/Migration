using UnityEngine;

public class GrassManager : MonoBehaviour
{
    public ProceduralMapGenerator map;

    [Header("Grass Amount")]
    public float maxGrass = 100f;
    public float regrowRate = 1.5f;

    [Header("Visual Threshold")]
    public float weakGrassThreshold = 35f;

    [Header("Colors")]
    public Color healthyGrassColor = new Color(0.55f, 0.85f, 0.35f);
    public Color weakGrassColor = new Color(0.72f, 0.72f, 0.25f);
    public Color barrenColor = new Color(0.62f, 0.45f, 0.25f);

    [Header("Planting")]
    public float plantableGrassThreshold = 5f;

    [Header("Lake Growth")]
    public float lakeGrowthInterval = 5f;
    public int lakeGrowthRadius = 10;
    public float lakeGrassChance = 0.2f;

    private float lakeGrowthTimer;

    private float[,] grassAmount;

    private void Start()
    {
        StartCoroutine(InitNextFrame());
    }

    private System.Collections.IEnumerator InitNextFrame()
    {
        yield return null;
        InitGrass();
    }

    void InitGrass()
    {
        grassAmount = new float[map.Width, map.Height];

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                grassAmount[x, y] =
                    map.GetTerrain(x, y) == ProceduralMapGenerator.TerrainType.Grass
                        ? maxGrass
                        : 0f;
            }
        }
    }

    private void Update()
    {
        if (grassAmount == null)
            return;

        lakeGrowthTimer += Time.deltaTime;

        if (lakeGrowthTimer >= lakeGrowthInterval)
        {
            lakeGrowthTimer = 0f;
            GrowGrassAroundLakes();
        }
    }

    void GrowGrassAroundLakes()
    {
        int grassCreated = 0;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.River)
                    continue;

                TryGrowGrassNearLake(x, y, ref grassCreated);
            }
        }

        if (grassCreated > 0)
        {
            map.ApplyTexture();
            Debug.Log($"���������ݵأ����� {grassCreated} ���");
        }
    }

    void TryGrowGrassNearLake(int lakeX, int lakeY, ref int grassCreated)
    {
        for (int attempt = 0; attempt < 8; attempt++)
        {
            int x = lakeX + Random.Range(-lakeGrowthRadius, lakeGrowthRadius + 1);
            int y = lakeY + Random.Range(-lakeGrowthRadius, lakeGrowthRadius + 1);

            if (!map.InBoundsPublic(x, y))
                continue;

            float dist = Vector2.Distance(
                new Vector2(lakeX, lakeY),
                new Vector2(x, y)
            );

            if (dist > lakeGrowthRadius)
                continue;

            if (Random.value > lakeGrassChance)
                continue;

            var terrain = map.GetTerrain(x, y);

            if (terrain == ProceduralMapGenerator.TerrainType.Mountain ||
                terrain == ProceduralMapGenerator.TerrainType.River)
                continue;

            if (terrain == ProceduralMapGenerator.TerrainType.Barren)
            {
                map.SetTerrain(x, y, ProceduralMapGenerator.TerrainType.Grass);
                grassAmount[x, y] = maxGrass;
                grassCreated++;
            }
            else if (terrain == ProceduralMapGenerator.TerrainType.Grass)
            {
                if (grassAmount[x, y] < maxGrass)
                {
                    grassAmount[x, y] += maxGrass * 0.25f;
                    grassAmount[x, y] = Mathf.Clamp(grassAmount[x, y], 0f, maxGrass);

                    UpdateGrassVisual(x, y);
                    grassCreated++;
                }
            }
        }
    }

    public bool EatGrass(Vector3 worldPos, float eatRadius, float eatAmount)
    {
        if (grassAmount == null)
        {
            Debug.LogWarning("GrassManager: grassAmount ��û��ʼ��");
            return false;
        }

        Vector2Int center = map.WorldToMap(worldPos);
        int pixelRadius = Mathf.RoundToInt(eatRadius * 32f);

        bool ateSomething = false;
        int eatenPixelCount = 0;
        float totalEatAmount = 0f;
        float minGrassAfterEat = maxGrass;

        for (int x = center.x - pixelRadius; x <= center.x + pixelRadius; x++)
        {
            for (int y = center.y - pixelRadius; y <= center.y + pixelRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                float dist = Vector2.Distance(
                    new Vector2(center.x, center.y),
                    new Vector2(x, y)
                );

                if (dist > pixelRadius)
                    continue;

                if (grassAmount[x, y] <= 0f)
                    continue;

                float before = grassAmount[x, y];

                grassAmount[x, y] -= eatAmount;
                grassAmount[x, y] = Mathf.Clamp(grassAmount[x, y], 0f, maxGrass);

                float eaten = before - grassAmount[x, y];

                eatenPixelCount++;
                totalEatAmount += eaten;
                minGrassAfterEat = Mathf.Min(minGrassAfterEat, grassAmount[x, y]);

                UpdateGrassVisual(x, y);
                ateSomething = true;
            }
        }

        if (ateSomething)
        {
            map.ApplyMapTexture();

            Debug.Log(
                $"¹�Բݳɹ���λ�� {center}, ������ {eatenPixelCount}, ������ {totalEatAmount:F1}, ��Ͳ��� {minGrassAfterEat:F1}"
            );
        }
        else
        {
            Debug.Log(
                $"¹û�Ե��ݣ�λ�� {center}, �뾶 {pixelRadius}"
            );
        }

        return ateSomething;
    }

    public float GetGrassAmount(Vector3 worldPos)
    {
        if (grassAmount == null)
            return 0f;

        Vector2Int mapPos = map.WorldToMap(worldPos);

        if (!map.InBoundsPublic(mapPos.x, mapPos.y))
            return 0f;

        return grassAmount[mapPos.x, mapPos.y];
    }

    void UpdateGrassVisual(int x, int y)
    {
        if (grassAmount[x, y] <= 1f)
            map.SetPixelColor(x, y, barrenColor);
        else if (grassAmount[x, y] < weakGrassThreshold)
            map.SetPixelColor(x, y, weakGrassColor);
        else
            map.SetPixelColor(x, y, healthyGrassColor);
    }

    public bool IsUsableGrass(Vector3 worldPos)
    {
        if (grassAmount == null)
            return false;

        Vector2Int mapPos = map.WorldToMap(worldPos);

        if (!map.InBoundsPublic(mapPos.x, mapPos.y))
            return false;

        if (map.GetTerrain(mapPos.x, mapPos.y) != ProceduralMapGenerator.TerrainType.Grass)
            return false;

        return grassAmount[mapPos.x, mapPos.y] > plantableGrassThreshold;
    }   

    public bool HasUsableGrassNear(Vector3 worldPos, float worldRadius)
    {
        if (grassAmount == null)
            return false;

        Vector2Int center = map.WorldToMap(worldPos);
        int pixelRadius = Mathf.RoundToInt(worldRadius * 32f);

        for (int x = center.x - pixelRadius; x <= center.x + pixelRadius; x++)
        {
            for (int y = center.y - pixelRadius; y <= center.y + pixelRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                if (grassAmount[x, y] > plantableGrassThreshold)
                    return true;
            }
        }

        return false;
    }

    public void SetGrassAmountAtMapPosition(int x, int y, float amount)
    {
        if (grassAmount == null)
            return;

        if (!map.InBoundsPublic(x, y))
            return;

        grassAmount[x, y] = Mathf.Clamp(amount, 0f, maxGrass);
    }

    public bool HasEnoughGrassNearby(Vector3 worldPos, float radius, int requiredGrassPixels)
    {
        if (grassAmount == null)
            return false;

        Vector2Int center = map.WorldToMap(worldPos);
        int pixelRadius = Mathf.RoundToInt(radius * 32f);

        int grassCount = 0;

        for (int x = center.x - pixelRadius; x <= center.x + pixelRadius; x++)
        {
            for (int y = center.y - pixelRadius; y <= center.y + pixelRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                float dist = Vector2.Distance(
                    new Vector2(center.x, center.y),
                    new Vector2(x, y)
                );

                if (dist > pixelRadius)
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                if (grassAmount[x, y] <= plantableGrassThreshold)
                    continue;

                grassCount++;
            }
        }

        return grassCount >= requiredGrassPixels;
    }

    public int GetHealthyGrassCount()
    {
        if (grassAmount == null)
            return 0;

        int count = 0;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                if (grassAmount[x, y] > weakGrassThreshold)
                {
                    count++;
                }
            }
        }

        return count;
    }
}
