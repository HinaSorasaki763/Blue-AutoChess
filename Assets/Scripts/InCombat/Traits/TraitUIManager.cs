using GameEnum;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TraitUIManager : MonoBehaviour
{
    public static TraitUIManager Instance { get; private set; }
    public GameObject traitUIPrefab; // �w�s��]�t�ϼЩM��r
    public Transform traitUIContainer; // �Ψө�m UI ��������
    private Dictionary<Traits, GameObject> activeTraitUIs = new Dictionary<Traits, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // �ھڨ���ƶq��s����UI
    public void UpdateTraitUI(Dictionary<Traits, int> traitCounts)
    {
        // �M�Ų{�������
        foreach (Transform child in traitUIContainer)
        {
            Destroy(child.gameObject);
        }
        activeTraitUIs.Clear();
        TraitPanelManager.Instance.ClearAllPanels();
        List<KeyValuePair<Traits, int>> sortedTraits = new List<KeyValuePair<Traits, int>>(traitCounts);
        sortedTraits.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); // ���Ө���ƶq�Ƨ�

        // �ͦ��s������ UI
        foreach (var trait in sortedTraits)
        {
            GameObject traitUI = Instantiate(traitUIPrefab, traitUIContainer);
            TraitUI uiComponent = traitUI.GetComponent<TraitUI>();

            if (uiComponent != null)
            {
                // �]�m trait �ݩʡA�T�O UI ���ع������T������
                uiComponent.trait = trait.Key;
            }

            activeTraitUIs[trait.Key] = traitUI;

            // �]�m�ϼЩM��r
            Image icon = traitUI.transform.Find("Icon").GetComponent<Image>(); // ���]�ϼЬO�R�W�� "Icon" ���l����
            TextMeshProUGUI text = traitUI.transform.Find("Text").GetComponent<TextMeshProUGUI>(); // ���]��r�O�R�W�� "Text" ���l����
            icon.sprite = TraitDescriptions.Instance.GetTraitImage(trait.Key);
            text.text = $"{trait.Key} ({trait.Value})"; // ������̦W�٩M����ƶq
        }
    }
}
