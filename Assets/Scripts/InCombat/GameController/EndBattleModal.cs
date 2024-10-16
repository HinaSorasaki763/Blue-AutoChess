using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
public class EndBattleModal : MonoBehaviour
{
    public static EndBattleModal Instance;
    public TextMeshProUGUI currentHealth;
    public TextMeshProUGUI AddedStacks;
    public TextMeshProUGUI EnemyName;
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
    public void UpdateText()
    {
        EnemyName.text = $"Win! Net win = 1 ";
        AddedStacks.text = $"pressure Added {PressureManager.Instance.GetPressure() - lastPressure}\n data Added {currData-lastData}";
    }
}
