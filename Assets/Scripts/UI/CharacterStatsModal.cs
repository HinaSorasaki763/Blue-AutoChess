using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterStatsModal : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public TMPro.TextMeshProUGUI stat;
    public Image background;
    public string tipText;
    void Update()
    {
        // ������ RectTransform
        RectTransform statRect = stat.rectTransform;
        RectTransform bgRect = background.GetComponent<RectTransform>();

        // Ū����r����ڼe��
        float newWidth = statRect.rect.width;

        // �]�w�I�����󪺼e�סA�������פ���
        bgRect.sizeDelta = new Vector2(newWidth, bgRect.sizeDelta.y);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowTooltip(tipText,gameObject);
    }

    // �ƹ����X
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }

    // �b��ʸ˸m�W�A�p�G�Q�Ϋ��U��ܡB��}���áA�]�i�H�[�o��q
    public void OnPointerDown(PointerEventData eventData)
    {
        // �i�̻ݨD���GTooltipManager.Instance.ShowTooltip(tipText);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // �i�̻ݨD���GTooltipManager.Instance.HideTooltip();
    }
}
