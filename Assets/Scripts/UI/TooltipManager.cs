using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameEnum;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [SerializeField] private GameObject tooltipObject; // 連到Tooltip的GameObject
    [SerializeField] private Image BackGround;
    [SerializeField] private TextMeshProUGUI tooltipText;
    private GameObject Caller;
    private RectTransform tooltipRect;
    public Canvas canvas;

    private void Awake()
    {
        Instance = this;
        tooltipRect = tooltipObject.GetComponent<RectTransform>();
        tooltipObject.SetActive(false);
    }
    /// <summary>
    /// 顯示提示文字
    /// </summary>
    /// <param name="text">要顯示的文字</param>
    public void ShowTooltip(string text,GameObject caller)
    {
        tooltipText.text = text;
        Caller = caller;
        tooltipObject.SetActive(true);
    }

    /// <summary>
    /// 隱藏提示文字
    /// </summary>
    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }

    private void Update()
    {
        if (tooltipObject.activeSelf)
        {
            // 拿到兩個 RectTransform
            RectTransform statRect = tooltipText.rectTransform;
            RectTransform bgRect = BackGround.GetComponent<RectTransform>();

            // 讀取文字的實際寬度
            float newWidth = statRect.rect.width;

            // 設定背景物件的寬度，維持高度不變
            bgRect.sizeDelta = new Vector2(newWidth, bgRect.sizeDelta.y);
            tooltipRect.anchoredPosition = Caller.GetComponent<RectTransform>().anchoredPosition+new Vector2();
        }

    }
}
