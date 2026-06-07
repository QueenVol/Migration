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

        //RegrowGrass();
    }

    void RegrowGrass()
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Grass)
                    continue;

                if (grassAmount[x, y] >= maxGrass)
                    continue;

                grassAmount[x, y] += regrowRate * Time.deltaTime;
                grassAmount[x, y] = Mathf.Clamp(grassAmount[x, y], 0f, maxGrass);

                UpdateGrassVisual(x, y);
            }
        }
    }

    public bool EatGrass(Vector3 worldPos, float eatRadius, float eatAmount)
    {
        if (grassAmount == null)
        {
            Debug.LogWarning("GrassManager: grassAmount 还没初始化");
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
                $"鹿吃草成功：位置 {center}, 像素数 {eatenPixelCount}, 总消耗 {totalEatAmount:F1}, 最低草量 {minGrassAfterEat:F1}"
            );
        }
        else
        {
            Debug.Log(
                $"鹿没吃到草：位置 {center}, 半径 {pixelRadius}"
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
}
