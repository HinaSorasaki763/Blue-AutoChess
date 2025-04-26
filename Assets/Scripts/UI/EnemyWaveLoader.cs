using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.iOS.Xcode;
using UnityEngine;
using UnityEngine.UI;

public class EnemyWaveLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject waveButtonPrefab;    // 用於顯示每個檔案的按鈕預製物
    public Transform waveButtonContainer;  // 按鈕容器 (scroll content parent)
    private string snapshotPath;
    private void Awake()
    {
        snapshotPath = Application.persistentDataPath;
    }
    private void Start()
    {
        RefreshFileListUI();  // 預設一開始就讀取所有.json檔
    }

    public void RefreshFileListUI()
    {
        // 先把容器底下舊的按鈕清除，避免重覆
        foreach (Transform child in waveButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // 取得所有 .json
        string[] jsonFiles = Directory.GetFiles(snapshotPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string filePath in jsonFiles)
        {
            // 1) 產生按鈕
            GameObject btnObj = Instantiate(waveButtonPrefab, waveButtonContainer);

            // 2) 取得檔名 (不含路徑與副檔名)
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // 3) 設定按鈕的文字
            TextMeshProUGUI textComponent = btnObj.GetComponent<WaveButtonPrefab>().WaveNameText;
            if (textComponent != null)
            {
                textComponent.text = fileNameWithoutExt;
            }

            // 4) 綁定 onClick 事件：點擊時載入該檔案
            Button button = btnObj.GetComponent<WaveButtonPrefab>().ConfirmButton;
            if (button != null)
            {
                // 用閉包把 path 傳進去
                button.onClick.AddListener(() => OnWaveFileButtonClicked(filePath));
            }
        }
    }

    // 按鈕被點擊後，載入這個 JSON 檔的資料
    private void OnWaveFileButtonClicked(string filePath)
    {
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Battling) return;
        ResetData();
        SpawnGrid.Instance.ResetAll();
        if (!File.Exists(filePath))
        {
            CustomLogger.LogError(this, $"找不到檔案: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        // 解析成 EnemyWaveData
        EnemyWaveData data = JsonUtility.FromJson<EnemyWaveData>(json);

        if (data == null)
        {
            CustomLogger.LogError(this, $"無法解析 JSON: {filePath}");
            return;
        }
        EnemyWave wave = EnemyWaveConverter.FromData(data);
        EnemySpawner.Instance.SpawnEnemiesNextStage(wave);

        CustomLogger.Log(this, $"已載入檔案: {filePath}");
    }
    private void ResetData()
    {
        PressureManager.Instance.ResetPressure();
        PressureManager.Instance.UpdateIndicater();
        DataStackManager.Instance.ResetData();
        DataStackManager.Instance.UpdateIndicator();
        ResourcePool.Instance.ally.ClearAllCharacter();
        ResourcePool.Instance.enemy.ClearAllCharacter();
        DamageStatisticsManager.Instance.ClearAll();
        Dictionary<Traits, int> traitCounts = new Dictionary<Traits, int>();
        TraitUIManager.Instance.UpdateTraitUI(traitCounts);
    }
}
