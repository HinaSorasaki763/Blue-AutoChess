using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameEnum;
using Firebase.Firestore;
using System;
using System.Threading.Tasks;
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
        selectedEnemyWaves = enemyWaves.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
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
        var revealedCharacters = availableSlots.OrderBy(x => UnityEngine.Random.value).Take(2).ToList();
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
    public void SpawnOpponentTeam(DocumentSnapshot opponentDoc)
    {
        if (opponentDoc == null)
        {
            Debug.LogError("Opponent doc is null.");
            return;
        }
        var data = opponentDoc.ToDictionary();
        StatsContainer statsContainer = new StatsContainer();
        if (data.TryGetValue("stats", out object statsObj) && statsObj is Dictionary<string, object> statsDict)
        {
            statsContainer = StatsContainer.FromDict(statsDict);
        }
        else
        {
            Debug.LogWarning("Opponent doc has no valid stats, using empty StatsContainer.");
            statsContainer = new StatsContainer();
        }
        BattlingProperties.Instance.SetSRTStats(statsContainer, false);
        if (data.TryGetValue("SelectedAugments", out object augmentsObj) && augmentsObj is List<object> augList)
        {
            List<int> selectedAugments = augList.Select(a => Convert.ToInt32(a)).ToList();
            SelectedAugments.Instance.enemySelectedAugments.Clear();
            SelectedAugments.Instance.enemySelectedAugments = selectedAugments;
        }
        else
        {
            Debug.LogWarning("Opponent doc has no valid SelectedAugments.");
        }

        if (!data.TryGetValue("slots", out object slotsObj) || !(slotsObj is List<object> slotsList))
        {
            Debug.LogError("Opponent doc has no valid slots.");
            return;
        }
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }
        foreach (var slotObj in slotsList)
        {
            var slotDict = slotObj as Dictionary<string, object>;
            if (slotDict == null) continue;
            int charId = Convert.ToInt32(slotDict["CharacterID"]);
            int gridIndex = Convert.ToInt32(slotDict["GridIndex"]);
            int star = Convert.ToInt32(slotDict["Star"]);

            if (!SpawnGrid.Instance.indexToCubeKey.TryGetValue(gridIndex, out string cubeKey))
            {
                Debug.LogError($"No cube key for GridIndex={gridIndex}");
                continue;
            }
            if (!SpawnGrid.Instance.hexNodes.TryGetValue(cubeKey, out HexNode hexNode))
            {
                Debug.LogError($"No HexNode for cubeKey={cubeKey}");
                continue;
            }
            Character characterData = ResourcePool.Instance.GetCharacterByID(charId);
            if (characterData == null)
            {
                Debug.LogError($"No Character found for ID={charId}");
                continue;
            }
            GameObject go = ResourcePool.Instance.SpawnCharacterAtPosition(
                characterData.Model,
                hexNode.Position,
                hexNode,
                enemyParent,
                isAlly: false,
                star
            );
            CharacterCTRL ctrl = go.GetComponent<CharacterCTRL>();
            if (ctrl == null)
            {
                Debug.LogError($"Spawned character {charId} has no CharacterCTRL.");
                continue;
            }
            if (slotDict.TryGetValue("EquipmentID", out object equipObj) && equipObj is List<object> equipList)
            {
                foreach (var e in equipList)
                {
                    int equipmentId = Convert.ToInt32(e);
                    if (equipmentId == -1) continue;
                    IEquipment template = EquipmentManager.Instance.GetEquipmentByID(equipmentId);
                    if (template != null)
                    {
                        IEquipment newEquipment = template.Clone();
                        CharacterObserverBase observer = ItemObserverFactory.GetObserverByIndex(newEquipment.Id);
                        if (observer != null)
                            newEquipment.Observer = observer;

                        ctrl.EquipItem(newEquipment);
                    }
                    else
                    {
                        Debug.LogWarning($"No Equipment found for ID={equipmentId}");
                    }
                }
            }
            Debug.Log($"Opponent CharID={charId} spawned at {hexNode.Position}");
        }
        ResourcePool.Instance.enemy.UpdateTraitEffects();
        ResourcePool.Instance.ally.UpdateTraitEffects();
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
        ResourcePool.Instance.ally.UpdateTraitEffects();
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
        return availableSlots.OrderBy(x => UnityEngine.Random.value).Take(2).ToList(); // 隨機揭露兩個角色
    }

}
