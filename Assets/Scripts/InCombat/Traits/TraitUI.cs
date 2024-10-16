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
    public float fixedWidth = 500f;     // �T�w���e��
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
        // �T�w TextMeshPro ���e��
        descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedWidth);

        // ���o TextMeshPro ���e���u������
        float preferredHeight = descriptionText.preferredHeight;

        // �վ� TextMeshPro ������
        descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

        // �Ϥ����󪺰��פ]���H�ܤơA���e�׫O���T�w
        descriptionPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
    }

    private void OnTraitClicked()
    {
        // ��������̪��y�z�����
        string description = TraitDescriptions.Instance.GetTraitDescription(trait);
        descriptionText.text = description;
        TraitPanelManager.Instance.OpenPanel(descriptionPanel);
    }
}
