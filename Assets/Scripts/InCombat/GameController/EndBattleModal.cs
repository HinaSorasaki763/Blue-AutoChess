using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
public class EndBattleModal : MonoBehaviour
{
    public static EndBattleModal Instance;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI AddedStacks;
    public TextMeshProUGUI Title;
    public GameObject HealthParent;
    public Sprite HealthSprite;       // 血量圖示
    public GameObject HealthPrefab;   // 預製體，用於顯示單個血量圖示
    private List<GameObject> healthIcons = new(); // 存儲所有生成的血量圖示
    public int lastPressure;
    public int currPressure;
    public int lastData;
    public int currData;

    public EnemySpawner EnemySpawner;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdateHealth(int health)
    {
        currentHealth.text = $"health {health}";
    }
    public void UpdateText(bool isEnemy)
    {
        StringBuilder sb = new StringBuilder();
        string text = !isEnemy ? $"Lose. LoseStreak = {GameStageManager.Instance.LoseStreak} " : $"Win! WinStreak = {GameStageManager.Instance.WinStreak} ";
        sb.AppendLine(text);
        sb.AppendLine($"If Lose next round ,will lose {GameStageManager.Instance.CalculateDamageTaken(GameStageManager.Instance.currentRound +1)} health");
        Title.text = sb.ToString();
        // 清空舊的圖示
        foreach (var icon in healthIcons)
        {
            Destroy(icon);
        }
        healthIcons.Clear();

        int healthCount = GameStageManager.Instance.PlayerHealth; // 獲取玩家血量數

        for (int i = 0; i < healthCount; i++)
        {
            GameObject healthIcon = new GameObject($"HealthIcon_{i}");
            healthIcon.transform.SetParent(HealthParent.transform, false);
            Image image = healthIcon.AddComponent<Image>();
            image.sprite = HealthSprite;
            healthIcons.Add(healthIcon);
        }
        //AddedStacks.text = $"pressure Added {PressureManager.Instance.GetPressure() - lastPressure}\n data Added {currData-lastData}";
    }
}
