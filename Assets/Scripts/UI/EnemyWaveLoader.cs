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
    public GameObject waveButtonPrefab;    // �Ω���ܨC���ɮת����s�w�s��
    public Transform waveButtonContainer;  // ���s�e�� (scroll content parent)
    private string snapshotPath;
    private void Awake()
    {
        snapshotPath = Application.persistentDataPath;
    }
    private void Start()
    {
        RefreshFileListUI();  // �w�]�@�}�l�NŪ���Ҧ�.json��
    }

    public void RefreshFileListUI()
    {
        // ����e�����U�ª����s�M���A�קK����
        foreach (Transform child in waveButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // ���o�Ҧ� .json
        string[] jsonFiles = Directory.GetFiles(snapshotPath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (string filePath in jsonFiles)
        {
            // 1) ���ͫ��s
            GameObject btnObj = Instantiate(waveButtonPrefab, waveButtonContainer);

            // 2) ���o�ɦW (���t���|�P���ɦW)
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

            // 3) �]�w���s����r
            TextMeshProUGUI textComponent = btnObj.GetComponent<WaveButtonPrefab>().WaveNameText;
            if (textComponent != null)
            {
                textComponent.text = fileNameWithoutExt;
            }

            // 4) �j�w onClick �ƥ�G�I���ɸ��J���ɮ�
            Button button = btnObj.GetComponent<WaveButtonPrefab>().ConfirmButton;
            if (button != null)
            {
                // �γ��]�� path �Ƕi�h
                button.onClick.AddListener(() => OnWaveFileButtonClicked(filePath));
            }
        }
    }

    // ���s�Q�I����A���J�o�� JSON �ɪ����
    private void OnWaveFileButtonClicked(string filePath)
    {
        if (GameStageManager.Instance.CurrGamePhase == GamePhase.Battling) return;
        ResetData();
        SpawnGrid.Instance.ResetAll();
        if (!File.Exists(filePath))
        {
            CustomLogger.LogError(this, $"�䤣���ɮ�: {filePath}");
            return;
        }

        string json = File.ReadAllText(filePath);
        // �ѪR�� EnemyWaveData
        EnemyWaveData data = JsonUtility.FromJson<EnemyWaveData>(json);

        if (data == null)
        {
            CustomLogger.LogError(this, $"�L�k�ѪR JSON: {filePath}");
            return;
        }
        EnemyWave wave = EnemyWaveConverter.FromData(data);
        EnemySpawner.Instance.SpawnEnemiesNextStage(wave);

        CustomLogger.Log(this, $"�w���J�ɮ�: {filePath}");
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
