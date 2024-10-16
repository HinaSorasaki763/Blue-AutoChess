using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameEnum;
using UnityEngine.EventSystems;

public class TraitUI : MonoBehaviour
{
    public Traits trait;
    public TextMeshProUGUI descriptionText;
    public GameObject descriptionPanel;
    public Button button;
    public float fixedWidth = 500f;     // 固定的寬度
    void Start()
    {
        TraitPanelManager.Instance.RegisterPanel(descriptionPanel);
        button.onClick.AddListener(OnTraitClicked);
        descriptionPanel.SetActive(false);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        TraitPanelManager.Instance.CloseAllPanels();
        TraitPanelManager.Instance.OpenPanel(descriptionPanel);
    }
    void Update()
    {
        // 固定 TextMeshPro 的寬度
        descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedWidth);

        // 取得 TextMeshPro 內容的優先高度
        float preferredHeight = descriptionText.preferredHeight;

        // 調整 TextMeshPro 的高度
        descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        // 使父物件的高度也跟隨變化，但寬度保持固定
        descriptionPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }

    private void OnTraitClicked()
    {
        // 獲取該羈絆的描述並顯示
        string description = TraitDescriptions.Instance.GetTraitDescription(trait);
        descriptionText.text = description;
        TraitPanelManager.Instance.OpenPanel(descriptionPanel);
    }
}
