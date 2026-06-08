using UnityEngine;
using TMPro;

public class EcologyScoreManager : MonoBehaviour
{
    public GrassManager grassManager;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Score Weight")]
    public float treeScore = 1f;
    public float birdScore = 3f;
    public float deerScore = 4f;
    public float healthyGrassScore = 0.05f;

    [Header("Target")]
    public float targetScore = 300f;

    private float currentScore;

    void Update()
    {
        CalculateScore();
        UpdateUI();
    }

    void CalculateScore()
    {
        int treeCount = FindObjectsOfType<Tree>().Length;
        int birdCount = FindObjectsOfType<Bird>().Length;
        int deerCount = FindObjectsOfType<Deer>().Length;

        float healthyGrassCount = 0f;

        if (grassManager != null)
        {
            healthyGrassCount = grassManager.GetHealthyGrassCount();
        }

        currentScore =
            treeCount * treeScore +
            birdCount * birdScore +
            deerCount * deerScore +
            healthyGrassCount * healthyGrassScore;
    }

    void UpdateUI()
    {
        if (scoreText == null)
            return;

        int treeCount = FindObjectsOfType<Tree>().Length;
        int birdCount = FindObjectsOfType<Bird>().Length;
        int deerCount = FindObjectsOfType<Deer>().Length;

        scoreText.text =
            $"生态评分：{currentScore:F0} / {targetScore:F0}\n" +
            $"树木：{treeCount}\n" +
            $"鸟：{birdCount}\n" +
            $"鹿：{deerCount}";

        if (currentScore >= targetScore)
        {
            scoreText.text += "\n生态系统已稳定";
        }
    }
}
