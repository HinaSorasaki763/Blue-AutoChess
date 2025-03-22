using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameEnum;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [SerializeField] private GameObject tooltipObject; // �s��Tooltip��GameObject
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
    /// ��ܴ��ܤ�r
    /// </summary>
    /// <param name="text">�n��ܪ���r</param>
    public void ShowTooltip(string text,GameObject caller)
    {
        tooltipText.text = text;
        Caller = caller;
        tooltipObject.SetActive(true);
    }

    /// <summary>
    /// ���ô��ܤ�r
    /// </summary>
    public void HideTooltip()
    {
        tooltipObject.SetActive(false);
    }

    private void Update()
    {
        if (tooltipObject.activeSelf)
        {
            // ������ RectTransform
            RectTransform statRect = tooltipText.rectTransform;
            RectTransform bgRect = BackGround.GetComponent<RectTransform>();

            // Ū����r����ڼe��
            float newWidth = statRect.rect.width;

            // �]�w�I�����󪺼e�סA�������פ���
            bgRect.sizeDelta = new Vector2(newWidth, bgRect.sizeDelta.y);
            tooltipRect.anchoredPosition = Caller.GetComponent<RectTransform>().anchoredPosition+new Vector2();
        }

    }
}
