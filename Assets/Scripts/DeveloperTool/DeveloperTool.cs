using GameEnum;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperTool : MonoBehaviour
{
    public GameObject characterButtonPrefab;
    public GameObject equipmentButtonPrefab;
    public Transform characterContentParent;
    public Transform equipmentContentParent;
    public BenchManager benchManager;
    public EquipmentManager equipmentManager;

    private List<GameObject> characterButtons = new List<GameObject>();
    private List<GameObject> equipmentButtons = new List<GameObject>();
    public void OnEnable()
    {

    }
    public void GenerateCharacterButtons()
    {
        // 获取所有角色的列表
        List<Character> allCharacters = ResourcePool.Instance.GetAllCharacters();

        foreach (var character in allCharacters)
        {
            // 创建角色按钮实例
            GameObject buttonObj = Instantiate(characterButtonPrefab, characterContentParent);
            Button button = buttonObj.GetComponent<Button>();
            Image image = buttonObj.GetComponent<Image>();

            // 设置按钮的头像图片
            if (character.Sprite != null)
            {
                image.sprite = character.Sprite;
                image.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"角色 {character.CharacterName} 缺少头像 Sprite。");
                image.color = new Color(1, 1, 1, 0);
            }

            // 添加点击事件
            Character capturedCharacter = character;

            button.onClick.AddListener(() => SpawnCharacter(capturedCharacter));

            // 将按钮添加到列表
            characterButtons.Add(buttonObj);
        }
    }

    public void GenerateEquipmentButtons()
    {
        List<IEquipment> allEquipments = equipmentManager.availableEquipments;
        StringBuilder sb = new StringBuilder();
        foreach (var equipment in allEquipments)
        {
            // 創建裝備按鈕實例
            sb.AppendLine($"{equipment} description : {equipment.EquipmentDetail}");
            GameObject buttonObj = Instantiate(equipmentButtonPrefab, equipmentContentParent);
            Button button = buttonObj.GetComponent<Button>();
            Image image = buttonObj.GetComponentInChildren<Image>();
            if (equipment.Icon != null)
            {
                image.sprite = equipment.Icon;
                image.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"裝備 {equipment.EquipmentName} 缺少圖標 Sprite。");
                image.color = new Color(1, 1, 1, 0);
            }
            IEquipment capturedEquipment = equipment;
            button.onClick.AddListener(() => SpawnEquipment(capturedEquipment));

            // 將按鈕加入列表
            equipmentButtons.Add(buttonObj);
        }
        CustomLogger.Log(this, sb.ToString());
    }


    private void SpawnCharacter(Character character)
    {
        if (!benchManager.IsBenchFull())
        {
            benchManager.AddToBench(character.Model);

        }
        else
        {
            Debug.Log("备战席已满，无法添加新角色。");
        }
    }

    private void SpawnEquipment(IEquipment equipment)
    {
        EquipmentManager.Instance.AddEquipmentItem(equipment);
    }
}
