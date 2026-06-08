using UnityEngine;

[System.Serializable]
public class LakeData
{
    public Vector2Int center;
    public int radius;
    public float waterAmount;

    public LakeData(Vector2Int center, int radius, float waterAmount)
    {
        this.center = center;
        this.radius = radius;
        this.waterAmount = waterAmount;
    }
}