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
        // 拿到兩個 RectTransform
        RectTransform statRect = stat.rectTransform;
        RectTransform bgRect = background.GetComponent<RectTransform>();

        // 讀取文字的實際寬度
        float newWidth = statRect.rect.width;

        // 設定背景物件的寬度，維持高度不變
        bgRect.sizeDelta = new Vector2(newWidth, bgRect.sizeDelta.y);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowTooltip(tipText,gameObject);
    }

    // 滑鼠移出
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }

    // 在行動裝置上，如果想用按下顯示、放開隱藏，也可以加這兩段
    public void OnPointerDown(PointerEventData eventData)
    {
        // 可依需求做：TooltipManager.Instance.ShowTooltip(tipText);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 可依需求做：TooltipManager.Instance.HideTooltip();
    }
}
