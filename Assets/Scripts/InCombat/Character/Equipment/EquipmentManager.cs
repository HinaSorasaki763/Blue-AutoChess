using GameEnum;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    public CombinationRouteSO combinationRoute;
    public List<IEquipment> availableEquipments = new List<IEquipment>();
    public GameObject equipmentItemPrefab;
    public Transform equipmentArea;
    public List<IEquipment> ownedEquipments = new List<IEquipment>();
    public GameObject parent;
    public Transform UIParent;
    public void Awake()
    {
        Instance = this;
    }
    public void Update()
    {

    }
    public void Start()
    {
        EquipmentSO[] equipments = Resources.LoadAll<EquipmentSO>("Equipments");
        foreach (var equipment in equipments)
        {
            if (equipment.isSpecial)
            {
                // 如果是特殊裝備，初始化為 SpecialEquipment
                SpecialEquipment specialEquipment = new (equipment);
                availableEquipments.Add(specialEquipment);
            }
            else if(equipment.IsConsumable)
            {
                ConsumableItem consumableItem = new (equipment);
                availableEquipments.Add(consumableItem);
            }
            else
            {
                // 如果是普通裝備，直接加入到列表中
                availableEquipments.Add(equipment);
            }
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
