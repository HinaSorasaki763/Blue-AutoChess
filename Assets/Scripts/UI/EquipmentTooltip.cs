using UnityEngine;
using UnityEngine.UI;

public class EquipmentTooltip : MonoBehaviour
{
    [Header("Tooltip 元件")]
    public GameObject tooltipPanel;    // 實際顯示 Tooltip 資訊的區塊（可設計為子物件）
    public Image equipmentIcon;
    public TMPro.TextMeshProUGUI equipmentNameText;
    public TMPro.TextMeshProUGUI equipmentDescriptionText;

    [Header("背景按鈕")]
    public Button backgroundButton;    // 背景按鈕，覆蓋全螢幕，點擊時隱藏 Tooltip

    void Awake()
    {
        // 一開始隱藏 Tooltip
        HideTooltip();
        if (backgroundButton != null)
        {
            backgroundButton.onClick.RemoveAllListeners();
            backgroundButton.onClick.AddListener(HideTooltip);
        }
    }

    /// <summary>
    /// 顯示傳入裝備的詳細資訊
    /// </summary>
    public void ShowTooltip(EquipmentSO equipment)
    {
        if (equipment == null)
            return;

        if (equipmentIcon != null)
            equipmentIcon.sprite = equipment.Icon;
        if (equipmentNameText != null)
            equipmentNameText.text = equipment.equipmentName;
        if (equipmentDescriptionText != null)
            equipmentDescriptionText.text = equipment.equipmentDescription;

        // 顯示 Tooltip 與背景
        tooltipPanel.SetActive(true);
        backgroundButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏 Tooltip 與背景
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        backgroundButton.gameObject.SetActive(false);
    }
}
