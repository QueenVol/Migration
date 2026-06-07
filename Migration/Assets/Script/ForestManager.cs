using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public GameObject treePrefab;
    public GrassManager grassManager;

    [Header("Initial Forest")]
    public int startingTreeCount = 30;
    public int treesPerPatch = 10;
    public float patchRadius = 0.8f;
    public float minTreeDistance = 0.18f;

    private void Start()
    {
        StartCoroutine(GenerateInitialForestsNextFrame());
    }

    private System.Collections.IEnumerator GenerateInitialForestsNextFrame()
    {
        yield return null;
        GenerateInitialForests();
    }

    void GenerateInitialForests()
    {
        for (int i = 0; i < startingTreeCount; i++)
        {
            SpawnSingleTreeOnGrass();
        }
    }

    void SpawnSingleTreeOnGrass()
    {
        for (int attempt = 0; attempt < 300; attempt++)
        {
            int mapX = Random.Range(0, map.Width);
            int mapY = Random.Range(0, map.Height);

            if (map.GetTerrain(mapX, mapY) != ProceduralMapGenerator.TerrainType.Grass)
                continue;

            Vector3 worldPos = MapToWorld(mapX, mapY);

            if (HasNearbyTree(worldPos))
                continue;

            Instantiate(
                treePrefab,
                new Vector3(worldPos.x, worldPos.y, -1),
                Quaternion.identity
            );

            return;
        }
    }

    public void CreateForestAt(Vector3 centerWorldPos)
    {
        int placed = 0;
        int attempts = 0;
        int maxAttempts = treesPerPatch * 80;

        float currentRadius = patchRadius;

        while (placed < treesPerPatch && attempts < maxAttempts)
        {
            attempts++;

            // 每隔一段尝试，扩大一圈
            if (attempts % 80 == 0)
            {
                currentRadius += 0.35f;
            }

            Vector2 randomOffset = Random.insideUnitCircle * currentRadius;
            Vector3 treePos = centerWorldPos + new Vector3(randomOffset.x, randomOffset.y, 0);

            if (!IsGrass(treePos))
                continue;

            if (HasNearbyTree(treePos))
                continue;

            Instantiate(
                treePrefab,
                new Vector3(treePos.x, treePos.y, -1),
                Quaternion.identity
            );

            placed++;
        }
    }

    bool IsGrass(Vector3 worldPos)
    {
        if (grassManager != null)
            return grassManager.IsUsableGrass(worldPos);

        int mapX = Mathf.RoundToInt(worldPos.x * 32 + map.Width / 2);
        int mapY = Mathf.RoundToInt(worldPos.y * 32 + map.Height / 2);

        return map.GetTerrain(mapX, mapY) == ProceduralMapGenerator.TerrainType.Grass;
    }

    bool HasNearbyTree(Vector3 pos)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, minTreeDistance);
        return hit != null;
    }

    Vector3 MapToWorld(int mapX, int mapY)
    {
        float worldX = (mapX - map.Width / 2f) / 32f;
        float worldY = (mapY - map.Height / 2f) / 32f;

        return new Vector3(worldX, worldY, 0);
    }
}
