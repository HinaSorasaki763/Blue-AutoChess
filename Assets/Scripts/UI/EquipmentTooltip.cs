using UnityEngine;
using UnityEngine.UI;

public class EquipmentTooltip : MonoBehaviour
{
    [Header("Tooltip ����")]
    public GameObject tooltipPanel;    // ������ Tooltip ��T���϶��]�i�]�p���l����^
    public Image equipmentIcon;
    public TMPro.TextMeshProUGUI equipmentNameText;
    public TMPro.TextMeshProUGUI equipmentDescriptionText;

    [Header("�I�����s")]
    public Button backgroundButton;    // �I�����s�A�л\���ù��A�I�������� Tooltip

    void Awake()
    {
        // �@�}�l���� Tooltip
        HideTooltip();
        if (backgroundButton != null)
        {
            backgroundButton.onClick.RemoveAllListeners();
            backgroundButton.onClick.AddListener(HideTooltip);
        }
    }

    /// <summary>
    /// ��ܶǤJ�˳ƪ��ԲӸ�T
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

        // ��� Tooltip �P�I��
        tooltipPanel.SetActive(true);
        backgroundButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// ���� Tooltip �P�I��
    /// </summary>
    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
        backgroundButton.gameObject.SetActive(false);
    }
}
