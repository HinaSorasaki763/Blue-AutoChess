using GameEnum;
using System.Collections.Generic;
using System.Linq;
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
                IConsumableEffect effect = CreateConsumableEffect(equipment);
                CustomLogger.Log(this, $"equuipment {equipment} added {effect} effect");
                ConsumableItem consumableItem = new (equipment,effect,equipment.Id);
                availableEquipments.Add(consumableItem);
            }
            else if(equipment.Index >=6)
            {
                CombinedEquipment combinedEquipment = new(equipment);
                CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(equipment.Index);
                if (c != null)
                {
                    combinedEquipment.observer = c;
                }
                availableEquipments.Add(combinedEquipment);
            }
            else if(equipment.Index < 6)
            {
                BasicEquipment basicEquipment = new(equipment);
                availableEquipments.Add(basicEquipment);
            }

        }
        AriusManager.Instance.Initiate();
    }
    public IConsumableEffect CreateConsumableEffect(EquipmentSO equipmentSO)
    {
        switch (equipmentSO.effectType)
        {
            case ConsumableEffectType.None:
                return null;
            case ConsumableEffectType.Remover:
                return new Remover();
            case ConsumableEffectType.AriusSelector:
                return new AriusSelector();
            case ConsumableEffectType.Duplicator:
                return new Duplicator();
            default:
                return null;
        }
    }
    // 新增裝備方法
    public GameObject AddEquipmentItem(IEquipment equipmentTemplate)
    {
        // 1. 先 Clone 出一個全新的裝備實例
        IEquipment newEquipment = equipmentTemplate.Clone();

        // 2. 由工廠為「新裝備」生成對應的 Observer
        CharacterObserverBase c = ItemObserverFactory.GetObserverByIndex(newEquipment.Id);
        newEquipment.Observer = c;

        // 3. 把新裝備加入 ownedEquipments
        ownedEquipments.Add(newEquipment);
        LogItems();

        // 4. 建立 UI 物件並設定
        GameObject item = Instantiate(equipmentItemPrefab, equipmentArea);
        EquipmentItemUI itemUI = item.GetComponent<EquipmentItemUI>();
        itemUI.Setup(newEquipment, this, UIParent);
        return item;
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
    public IEquipment GetEquipmentByID(int equipmentID)
    {
        StringBuilder sb = new();
        foreach (var item in availableEquipments)
        {
            sb.AppendLine($"item{item.EquipmentName} =  {item.GetType()}");
            if (item is CombinedEquipment e)
            {
                sb.AppendLine($"item {e.id} = {e.EquipmentName}");
            }

        }
        CustomLogger.Log(this,sb.ToString());
        return availableEquipments.FirstOrDefault(e => (e is CombinedEquipment eq) && eq.id == equipmentID);
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
