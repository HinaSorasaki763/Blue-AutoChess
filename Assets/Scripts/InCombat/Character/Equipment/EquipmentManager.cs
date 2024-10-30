using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public List<EquipmentSO> availableEquipments = new List<EquipmentSO>(); // 使用 ScriptableObject 型別
    public GameObject equipmentItemPrefab;
    public Transform equipmentArea;
    public List<IEquipment> ownedEquipments = new List<IEquipment>();
    public void Start()
    {
        // 載入所有裝備 ScriptableObject
        EquipmentSO[] equipments = Resources.LoadAll<EquipmentSO>("Equipments");
        foreach (var equipment in equipments)
        {
            availableEquipments.Add(equipment);
        }
    }

    // 新增裝備方法
    public void AddEquipmentItem(IEquipment equipment)
    {
        ownedEquipments.Add(equipment);
        LogItems();
        GameObject item = Instantiate(equipmentItemPrefab, equipmentArea);
        EquipmentItemUI itemUI = item.GetComponent<EquipmentItemUI>();
        itemUI.Setup(equipment, this);
    }
    public void LogItems()
    {
        StringBuilder sb = new StringBuilder();
        int i = 0;
        sb.AppendLine();
        foreach (var eq in ownedEquipments)
        {

            sb.AppendLine($"eq {i} : {eq.EquipmentName}");
            i++;
        }
        CustomLogger.Log(this, sb.ToString());
    }
    // 移除裝備方法
    public void RemoveEquipmentItem(IEquipment equipment)
    {
        // 若找到對應裝備，從列表中移除
        if (ownedEquipments.Contains(equipment as EquipmentSO))
        {
            ownedEquipments.Remove(equipment as EquipmentSO);
        }
        LogItems();
        // 在這裡可能還需要摧毀對應的 UI 元素
        /*foreach (Transform child in equipmentArea)
        {
            EquipmentItemUI itemUI = child.GetComponent<EquipmentItemUI>();
            if (itemUI != null && itemUI.GetEquipment() == equipment)
            {
                Destroy(child.gameObject);
                break;
            }
        }*/
    }
}
