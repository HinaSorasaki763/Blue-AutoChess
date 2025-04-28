using GameEnum;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyWaveLoader : MonoBehaviour
{
    [Header("UI References")]
    public GameObject waveButtonPrefab;
    public Transform waveButtonContainer;
    private string snapshotPath;
    private void Awake()
    {
        snapshotPath = Application.persistentDataPath;
    }
    private void Start()
    {
        RefreshFileListUI();
    }

    public void RefreshFileListUI()
    {
        foreach (Transform child in waveButtonContainer)
        {
            Destroy(child.gameObject);
        }
        string[] jsonFiles = Directory.GetFiles(snapshotPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string filePath in jsonFiles)
        {
            GameObject btnObj = Instantiate(waveButtonPrefab, waveButtonContainer);
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            TextMeshProUGUI textComponent = btnObj.GetComponent<WaveButtonPrefab>().WaveNameText;
            if (textComponent != null)
            {
                textComponent.text = fileNameWithoutExt;
            }
            Button button = btnObj.GetComponent<WaveButtonPrefab>().ConfirmButton;
            if (button != null)
            {
                button.onClick.AddListener(() => OnWaveFileButtonClicked(filePath));
            }
        }
    }
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
    public void DeleteAllSnapshots()
    {
        // 刪除所有 .json 檔案
        string[] jsonFiles = Directory.GetFiles(snapshotPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string filePath in jsonFiles)
        {
            try
            {
                File.Delete(filePath);
                CustomLogger.Log(this, $"已刪除檔案: {filePath}");
            }
            catch (System.Exception ex)
            {
                CustomLogger.LogError(this, $"刪除檔案失敗: {filePath}, 錯誤訊息: {ex.Message}");
            }
        }
        RefreshFileListUI();
    }

    private void ResetData()
    {
        PressureManager.Instance.ResetPressure();
        PressureManager.Instance.UpdateIndicater();
        DataStackManager.Instance.ResetData();
        DataStackManager.Instance.UpdateIndicator();
        ResourcePool.Instance.enemy.ClearAllCharacter();
        DamageStatisticsManager.Instance.ClearAll();
        Dictionary<Traits, int> traitCounts = new Dictionary<Traits, int>();
        TraitUIManager.Instance.UpdateTraitUI(traitCounts);
    }
}
