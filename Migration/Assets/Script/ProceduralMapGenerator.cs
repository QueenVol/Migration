using System.Collections.Generic;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    public int width = 512;
    public int height = 512;

    [Header("Map Shape")]
    public int borderSize = 30;
    public int riverWidth = 8;

    [Header("Colors")]
    public Color mountainColor = new Color(0.18f, 0.18f, 0.18f);
    public Color riverColor = new Color(0.08f, 0.35f, 0.9f);
    public Color grassColor = new Color(0.55f, 0.85f, 0.35f);
    public Color forestColor = new Color(0.05f, 0.35f, 0.12f);
    public Color barrenColor = new Color(0.62f, 0.45f, 0.25f);

    private TerrainType[,] terrainMap;

    enum TerrainType
    {
        Barren,
        Grass,
        Forest,
        River,
        Mountain
    }

    void Start()
    {
        GenerateFixedDemoMap();
    }

    void GenerateFixedDemoMap()
    {
        terrainMap = new TerrainType[width, height];

        FillBarren();
        DrawMountainBorder();
        DrawRiver();
        DrawGrassPatches();
        DrawSparseTrees();

        DrawTexture();
    }

    void FillBarren()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                terrainMap[x, y] = TerrainType.Barren;
            }
        }
    }

    void DrawMountainBorder()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int distToEdge = Mathf.Min(x, y, width - 1 - x, height - 1 - y);

                float noise = Mathf.PerlinNoise(x * 0.04f, y * 0.04f);
                int roughBorder = borderSize + Mathf.RoundToInt(noise * 12f);

                if (distToEdge < roughBorder)
                {
                    terrainMap[x, y] = TerrainType.Mountain;
                }
            }
        }
    }

    void DrawRiver()
    {
        for (int y = borderSize; y < height - borderSize; y++)
        {
            float t = y / (float)height;

            int centerX = Mathf.RoundToInt(
                width * 0.5f
                + Mathf.Sin(t * Mathf.PI * 3f) * 70f
                + Mathf.Sin(t * Mathf.PI * 8f) * 18f
            );

            PaintCircle(centerX, y, riverWidth, TerrainType.River);
        }
    }

    void DrawGrassPatches()
    {
        PaintSolidOrganicGrassPatch(width / 2, height / 2, 190, 135);
    }

    void PaintSolidOrganicGrassPatch(int cx, int cy, int radiusX, int radiusY)
    {
        float seed = Random.Range(0f, 10000f);

        for (int x = cx - radiusX - 30; x <= cx + radiusX + 30; x++)
        {
            for (int y = cy - radiusY - 30; y <= cy + radiusY + 30; y++)
            {
                if (!InBounds(x, y))
                    continue;

                if (terrainMap[x, y] == TerrainType.Mountain || terrainMap[x, y] == TerrainType.River)
                    continue;

                float angle = Mathf.Atan2(y - cy, x - cx);

                // 根据角度生成边缘扰动，只影响外轮廓
                float noise =
                    Mathf.PerlinNoise(
                        seed + Mathf.Cos(angle) * 2.5f,
                        seed + Mathf.Sin(angle) * 2.5f
                    );

                float edgeOffset = Mathf.Lerp(-28f, 28f, noise);

                float finalRadiusX = radiusX + edgeOffset;
                float finalRadiusY = radiusY + edgeOffset * 0.65f;

                float dx = (x - cx) / finalRadiusX;
                float dy = (y - cy) / finalRadiusY;

                // 只要在不规则轮廓内部，就全部填成草地
                if (dx * dx + dy * dy <= 1f)
                {
                    terrainMap[x, y] = TerrainType.Grass;
                }
            }
        }
    }

    void DrawSparseTrees()
    {
        PlaceSingleTreesOnGrass(32);
    }

    void PlaceSingleTreesOnGrass(int treeCount)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = treeCount * 80;

        while (placed < treeCount && attempts < maxAttempts)
        {
            attempts++;

            int x = Random.Range(borderSize + 10, width - borderSize - 10);
            int y = Random.Range(borderSize + 10, height - borderSize - 10);

            if (terrainMap[x, y] != TerrainType.Grass)
                continue;

            PaintCircle(x, y, Random.Range(2, 4), TerrainType.Forest);
            placed++;
        }
    }

    void PaintCircle(int cx, int cy, int radius, TerrainType type)
    {
        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (!InBounds(x, y))
                    continue;

                if (terrainMap[x, y] == TerrainType.Mountain)
                    continue;

                float dist = Vector2.Distance(new Vector2(cx, cy), new Vector2(x, y));

                if (dist <= radius)
                {
                    terrainMap[x, y] = type;
                }
            }
        }
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void DrawTexture()
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixels[y * width + x] = GetColor(terrainMap[x, y]);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            32f
        );

        GameObject mapObject = new GameObject("Generated Map");
        SpriteRenderer sr = mapObject.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;

        CameraDragController cameraDrag = Camera.main.GetComponent<CameraDragController>();
        if (cameraDrag != null)
        {
            cameraDrag.mapRenderer = sr;
        }
    }

    Color GetColor(TerrainType type)
    {
        switch (type)
        {
            case TerrainType.Mountain:
                return mountainColor;
            case TerrainType.River:
                return riverColor;
            case TerrainType.Grass:
                return grassColor;
            case TerrainType.Forest:
                return forestColor;
            case TerrainType.Barren:
                return barrenColor;
            default:
                return Color.magenta;
        }
    }
}
