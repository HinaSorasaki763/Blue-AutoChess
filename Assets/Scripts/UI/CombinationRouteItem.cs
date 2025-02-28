using UnityEngine;
using UnityEngine.UI;

public class CombinationRouteItem : MonoBehaviour
{
    public Image equipmentAIcon;  // ��ܰ򥻹D�� A �ϥ�
    public Image equipmentBIcon;  // ��ܰ򥻹D�� B �ϥ�
    public Image resultEquipmentIcon; // ��ܦX�����G�˳ƹϥ�
    public Button resultButton;       // �I����|��ܸԲӪ`��

    // �ǤJ��ƻP Tooltip ���ѷӡA�]�w UI �P�I���ƥ�
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
                // �I�����G�˳ơA��� Tooltip�A�ǤJ���G�˳Ƹ��
                tooltip.ShowTooltip(entry.resultEquipment);
            });
        }
    }
}
