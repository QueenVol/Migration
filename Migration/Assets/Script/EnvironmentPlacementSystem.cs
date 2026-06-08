using UnityEngine;

public class EnvironmentPlacementSystem : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GrassManager grassManager;
    public float lakeWaterAmount = 500f;

    [Header("Grass")]
    public float grassPatchRadius = 0.7f;

    [Header("Lake")]
    public float lakeRadius = 0.45f;

    private enum PlacementMode
    {
        None,
        Grass,
        Lake
    }

    private PlacementMode currentMode = PlacementMode.None;

    public void StartPlaceGrass()
    {
        currentMode = PlacementMode.Grass;
    }

    public void StartPlaceLake()
    {
        currentMode = PlacementMode.Lake;
    }

    void Update()
    {
        if (currentMode == PlacementMode.None)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;

            if (currentMode == PlacementMode.Grass)
            {
                PlaceGrassPatch(worldPos);
            }
            else if (currentMode == PlacementMode.Lake)
            {
                PlaceLake(worldPos);
            }

            currentMode = PlacementMode.None;
        }
    }

    void PlaceGrassPatch(Vector3 centerWorldPos)
    {
        if (!TryFindNearestValidPoint(
            centerWorldPos,
            ProceduralMapGenerator.TerrainType.Grass,
            out centerWorldPos
        ))
        {
            Debug.Log("����û�п��ֲݵ�λ��");
            return;
        }

        Vector2Int center = map.WorldToMap(centerWorldPos);
        int pixelRadius = Mathf.RoundToInt(grassPatchRadius * 32f);

        for (int x = center.x - pixelRadius; x <= center.x + pixelRadius; x++)
        {
            for (int y = center.y - pixelRadius; y <= center.y + pixelRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                if (map.GetTerrain(x, y) != ProceduralMapGenerator.TerrainType.Barren)
                    continue;

                float dist = Vector2.Distance(
                    new Vector2(center.x, center.y),
                    new Vector2(x, y)
                );

                if (dist > pixelRadius)
                    continue;

                map.SetTerrain(x, y, ProceduralMapGenerator.TerrainType.Grass);

                if (grassManager != null)
                {
                    grassManager.SetGrassAmountAtMapPosition(
                        x,
                        y,
                        grassManager.maxGrass
                    );
                }
            }
        }

        map.ApplyTexture();
        Debug.Log("�ֲ����");
    }

    void PlaceLake(Vector3 centerWorldPos)
    {
        if (!TryFindNearestValidPoint(
            centerWorldPos,
            ProceduralMapGenerator.TerrainType.River,
            out centerWorldPos
        ))
        {
            Debug.Log("����û�пɽ�����λ��");
            return;
        }

        Vector2Int center = map.WorldToMap(centerWorldPos);
        int pixelRadius = Mathf.RoundToInt(lakeRadius * 32f);

        for (int x = center.x - pixelRadius; x <= center.x + pixelRadius; x++)
        {
            for (int y = center.y - pixelRadius; y <= center.y + pixelRadius; y++)
            {
                if (!map.InBoundsPublic(x, y))
                    continue;

                var terrain = map.GetTerrain(x, y);

                if (terrain == ProceduralMapGenerator.TerrainType.Mountain ||
                    terrain == ProceduralMapGenerator.TerrainType.River)
                    continue;

                float dist = Vector2.Distance(
                    new Vector2(center.x, center.y),
                    new Vector2(x, y)
                );

                if (dist > pixelRadius)
                    continue;

                map.SetTerrain(x, y, ProceduralMapGenerator.TerrainType.River);

                if (grassManager != null)
                {
                    grassManager.SetGrassAmountAtMapPosition(x, y, 0f);
                }
            }
        }

        map.ApplyTexture();
        Debug.Log("人工湖建造完成");

        if (grassManager != null)
        {
            grassManager.RegisterLake(center, pixelRadius, lakeWaterAmount);
        }
    }

    bool TryFindNearestValidPoint(
    Vector3 clickWorldPos,
    ProceduralMapGenerator.TerrainType targetType,
    out Vector3 validWorldPos
)
    {
        Vector2Int clickMapPos = map.WorldToMap(clickWorldPos);

        int maxSearchRadius = 80;

        for (int radius = 0; radius <= maxSearchRadius; radius++)
        {
            for (int x = clickMapPos.x - radius; x <= clickMapPos.x + radius; x++)
            {
                for (int y = clickMapPos.y - radius; y <= clickMapPos.y + radius; y++)
                {
                    if (!map.InBoundsPublic(x, y))
                        continue;

                    if (Vector2Int.Distance(clickMapPos, new Vector2Int(x, y)) > radius)
                        continue;

                    if (!IsValidPlacementTerrain(x, y, targetType))
                        continue;

                    validWorldPos = map.MapToWorld(x, y);
                    return true;
                }
            }
        }

        validWorldPos = clickWorldPos;
        return false;
    }

    bool IsValidPlacementTerrain(
    int x,
    int y,
    ProceduralMapGenerator.TerrainType targetType
)
    {
        ProceduralMapGenerator.TerrainType current = map.GetTerrain(x, y);

        if (current == ProceduralMapGenerator.TerrainType.Mountain)
            return false;

        if (targetType == ProceduralMapGenerator.TerrainType.Grass)
        {
            return current == ProceduralMapGenerator.TerrainType.Barren;
        }

        if (targetType == ProceduralMapGenerator.TerrainType.River)
        {
            return current == ProceduralMapGenerator.TerrainType.Barren
                || current == ProceduralMapGenerator.TerrainType.Grass;
        }

        return false;
    }
}
