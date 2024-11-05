using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public CombinationRouteSO combinationRoute;
    public List<EquipmentSO> availableEquipments = new List<EquipmentSO>(); // 使用 ScriptableObject 型別
    public GameObject equipmentItemPrefab;
    public Transform equipmentArea;
    public List<IEquipment> ownedEquipments = new List<IEquipment>();
    public GameObject parent;
    public Transform UIParent;
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
        itemUI.Setup(equipment, this, UIParent);
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
    public void RemoveEquipmentItem(IEquipment equipment,GameObject obj)
    {
        if (ownedEquipments.Contains(equipment))
        {
            ownedEquipments.Remove(equipment);
            LogItems();
        }
        else
        {
            Debug.LogWarning($"嘗試移除未找到的裝備: {equipment.EquipmentName}");
        }
        obj.SetActive(false);
    }

}
