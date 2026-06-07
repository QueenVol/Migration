using UnityEngine;

public class TreePlacementSystem : MonoBehaviour
{
    public ProceduralMapGenerator map;
    public ForestManager forestManager;
    public GrassManager grassManager;

    private bool placingTree;

    public void StartPlaceTree()
    {
        placingTree = true;
    }

    private void Update()
    {
        if (!placingTree)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryPlaceForestPatch();
        }
    }

    void TryPlaceForestPatch()
    {
        Vector3 centerWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        centerWorldPos.z = 0;

        int mapX = Mathf.RoundToInt(centerWorldPos.x * 32 + map.Width / 2);
        int mapY = Mathf.RoundToInt(centerWorldPos.y * 32 + map.Height / 2);

        if (!grassManager.HasUsableGrassNear(centerWorldPos, 0.6f))
        {
            Debug.Log("附近没有可用草地");
            return;
        }

        forestManager.CreateForestAt(centerWorldPos);
        placingTree = false;
    }
}
