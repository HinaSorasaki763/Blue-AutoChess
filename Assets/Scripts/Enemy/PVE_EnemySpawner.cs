using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PVE_EnemySpawner : MonoBehaviour
{
    public static PVE_EnemySpawner Instance;
    public List<EnemyWave> enemyWaves;
    private int index = 0;
    public CharacterParent enemyParent;
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
        enemyWaves = Resources.LoadAll<EnemyWave>("PVE_Resources").ToList();
    }
    public void SpawnEnemiesNextStage()
    {
        // 清理上一波敵人
        foreach (var item in enemyParent.childCharacters)
        {
            Destroy(item.GetComponent<CharacterCTRL>().characterBars);
            Destroy(item);
        }

        // 生成新一波敵人
        foreach (var slot in enemyWaves[index].gridSlots)
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
                            isAlly: false
                        );

                        CharacterCTRL characterCtrl = go.GetComponent<CharacterCTRL>();
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
        SpawnLogisticCharacter(enemyWaves[index].logisticSlot1, ResourcePool.Instance.EnemylogisticSlotNode1);
        SpawnLogisticCharacter(enemyWaves[index].logisticSlot2, ResourcePool.Instance.EnemylogisticSlotNode2);

        ResourcePool.Instance.enemy.UpdateTraitEffects();
        index++;
    }

    private void SpawnLogisticCharacter(EnemyWave.GridSlotData logisticSlot, HexNode logisticSlotNode)
    {
        if (logisticSlot.CharacterID != -1)
        {
            Vector3 position = logisticSlotNode.Position;
            Character characterData = ResourcePool.Instance.GetCharacterByID(logisticSlot.CharacterID);
            GameObject characterPrefab = characterData.Model;

            ResourcePool.Instance.SpawnCharacterAtPosition(characterPrefab, position, logisticSlotNode, enemyParent, isAlly: false);
        }
    }
}
