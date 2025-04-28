using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameEnum;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;
    public List<EnemyWave> enemyWaves;
    public CharacterParent enemyParent;
    public List<EnemyWave> selectedEnemyWaves = new List<EnemyWave>();
    private EnemyWave chosenWave;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        LoadEnemyWaves();
    }

    // Step 1: Load all enemy waves from Resources
    private void LoadEnemyWaves()
    {
        enemyWaves = Resources.LoadAll<EnemyWave>("EnemyWaves").ToList();
        if (enemyWaves.Count < 3)
        {
            Debug.LogError("Not enough enemy waves available.");
            return;
        }
    }

    // Step 2: Randomly select three enemy waves
    public void SelectRandomEnemyWaves()
    {
        selectedEnemyWaves = enemyWaves.OrderBy(x => Random.value).Take(3).ToList();
        foreach (var wave in selectedEnemyWaves)
        {
            RevealTwoRandomCharacters(wave);
        }
    }

    // Step 3: Randomly reveal two characters in an enemy wave
    private void RevealTwoRandomCharacters(EnemyWave wave)
    {
        List<EnemyWave.GridSlotData> availableSlots = wave.gridSlots.Where(x => x.CharacterID != -1).ToList();
        if (availableSlots.Count <= 2)
        {
            Debug.LogWarning($"Wave {wave.EnemyName} has less than or equal to two characters, revealing all.");
            // 若角色數量不足兩個，直接揭露所有角色
            foreach (var slot in availableSlots)
            {
                // 假設有一個揭露角色的處理方式，可以是設定角色為顯示狀態
                Debug.Log($"Revealed character: {slot.CharacterID}");
            }
            return;
        }
        var revealedCharacters = availableSlots.OrderBy(x => Random.value).Take(2).ToList();
        foreach (var slot in revealedCharacters)
        {
            
            Debug.Log($"Revealed character: {slot.CharacterID}");
        }
    }

    public void PlayerSelectWave(int waveIndex)
    {
        CustomLogger.Log(this, "PlayerSelectWave");
        if (waveIndex < 0 || waveIndex >= selectedEnemyWaves.Count)
        {
            Debug.LogError("Invalid wave index selected.");
            return;
        }

        chosenWave = selectedEnemyWaves[waveIndex];
        SpawnEnemiesNextStage(chosenWave);
    }

    public void SpawnEnemiesNextStage(EnemyWave enemyWave)
    {
        if (enemyWave == null)
        {
            Debug.LogError("No wave chosen. Please select a wave first.");
            return;
        }

        // 清理上一波敵人
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }

        // 生成新一波敵人
        foreach (var slot in enemyWave.gridSlots)
        {
            if (slot.CharacterID != -1)
            {
                if (SpawnGrid.Instance.indexToCubeKey.TryGetValue(slot.GridIndex, out string cubeKey))
                {
                    if (SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode hexNode))
                    {
                        Vector3 position = hexNode.Position;
                        Character characterData = ResourcePool.Instance.GetCharacterByID(slot.CharacterID);
                        GameObject characterPrefab = characterData.Model;
                        GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
                            characterPrefab,
                            position,
                            hexNode,
                            enemyParent,
                            isAlly: false,
                            slot.Level
                        );

                        CharacterCTRL characterCtrl = go.GetComponent<CharacterCTRL>();
                        if (slot.CharacterID == 999)
                        {
                            StaticObject staticObject = go.GetComponent<StaticObject>();
                            staticObject.InitNoParentDummy(10000,35,false);
                        }
                        if (characterCtrl != null)
                        {
                            // 為該角色裝備裝備
                            for (int i = 0; i < slot.EquipmentIDs.Length; i++)
                            {
                                int equipmentID = slot.EquipmentIDs[i];
                                if (equipmentID != -1)
                                {
                                    IEquipment template = EquipmentManager.Instance.GetEquipmentByID(equipmentID);
                                    if (template != null)
                                    {
                                        // 先 Clone 一份新裝備，再給角色裝備
                                        IEquipment newEquipment = template.Clone();
                                        CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(newEquipment.Id);
                                        if (c != null)
                                        {
                                            newEquipment.Observer = c;
                                        }
                                        bool equipped = characterCtrl.EquipItem(newEquipment);

                                        CustomLogger.Log(
                                            this,
                                            $"Character {characterData.name} equipped with {newEquipment.EquipmentName}: {equipped}"
                                        );
                                    }
                                    else
                                    {
                                        CustomLogger.LogError(this, $"No Equipment found for ID {equipmentID}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            CustomLogger.LogError(this, $"Character {characterData.name} has no CharacterCTRL component.");
                        }

                        CustomLogger.Log(this, $"Character {characterData.name} spawned at {position}");
                    }
                    else
                    {
                        CustomLogger.LogError(this, $"No HexNode found for cube key {cubeKey}");
                    }
                }
                else
                {
                    Debug.LogError($"No cube key found for GridIndex {slot.GridIndex}");
                }
            }
        }

        // 為 Logistic 角色配置裝備 (如果需要)
        SpawnLogisticCharacter(enemyWave.logisticSlot1, ResourcePool.Instance.EnemylogisticSlotNode1);
        SpawnLogisticCharacter(enemyWave.logisticSlot2, ResourcePool.Instance.EnemylogisticSlotNode2);

        ResourcePool.Instance.enemy.UpdateTraitEffects();
    }

    private void SpawnLogisticCharacter(EnemyWave.GridSlotData logisticSlot, HexNode logisticSlotNode)
    {
        if (logisticSlot.CharacterID != -1)
        {
            Vector3 position = logisticSlotNode.Position;
            Character characterData = ResourcePool.Instance.GetCharacterByID(logisticSlot.CharacterID);
            GameObject characterPrefab = characterData.Model;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, logisticSlotNode, enemyParent, isAlly: false,logisticSlot.Level);
        }
    }
    public List<EnemyWave.GridSlotData> GetRevealedCharacters(EnemyWave wave)
    {
        List<EnemyWave.GridSlotData> availableSlots = wave.gridSlots.Where(x => x.CharacterID != -1).ToList();
        if (availableSlots.Count <= 2)
        {
            return availableSlots; // 若角色數量不足兩個，返回所有角色
        }
        return availableSlots.OrderBy(x => Random.value).Take(2).ToList(); // 隨機揭露兩個角色
    }

}
