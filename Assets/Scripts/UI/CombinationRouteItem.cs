using UnityEngine;
using UnityEngine.UI;

public class CombinationRouteItem : MonoBehaviour
{
    public Image equipmentAIcon;  // 顯示基本道具 A 圖示
    public Image equipmentBIcon;  // 顯示基本道具 B 圖示
    public Image resultEquipmentIcon; // 顯示合成結果裝備圖示
    public Button resultButton;       // 點擊後會顯示詳細注釋

    // 傳入資料與 Tooltip 的參照，設定 UI 與點擊事件
    public void Setup(CombinationRouteSO.CombinationEntry entry, EquipmentTooltip tooltip)
    {
        if (entry.equipmentA != null && equipmentAIcon != null)
            equipmentAIcon.sprite = entry.equipmentA.Icon;
        if (entry.equipmentB != null && equipmentBIcon != null)
            equipmentBIcon.sprite = entry.equipmentB.Icon;
        if (entry.resultEquipment != null && resultEquipmentIcon != null)
            resultEquipmentIcon.sprite = entry.resultEquipment.Icon;

        if (resultButton != null)
        {
            resultButton.onClick.RemoveAllListeners();
            resultButton.onClick.AddListener(() =>
            {
                // 點擊結果裝備，顯示 Tooltip，傳入結果裝備資料
                tooltip.ShowTooltip(entry.resultEquipment);
            });
        }
    }
}
