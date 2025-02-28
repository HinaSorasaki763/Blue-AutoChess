using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameEnum;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class TraitUI : MonoBehaviour
{
    public Traits trait;
    public TextMeshProUGUI descriptionText;
    public GameObject descriptionPanel;
    public Button button;
    public float fixedWidth = 500f;
    public Image Sprite;
    public GameObject CharacterTraitProfilePrefab;
    public Transform ImageParent;
    private readonly List<GameObject> ImageTraits = new();

    void Start()
    {
        TraitPanelManager.Instance.RegisterPanel(descriptionPanel);
        button.onClick.AddListener(OnTraitClicked);
        descriptionPanel.SetActive(false);
    }

    void Update()
    {
        if (descriptionText.rectTransform.sizeDelta.x != fixedWidth)
        {
            descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fixedWidth);
        }
        float preferredHeight = descriptionText.preferredHeight + 200;
        if (descriptionText.rectTransform.sizeDelta.y != preferredHeight)
        {
            descriptionText.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
            descriptionPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
        }
    }

    private void OnTraitClicked()
    {
        descriptionText.text = TraitDescriptions.Instance.GetTraitDescription(trait);
        TraitPanelManager.Instance.OpenPanel(descriptionPanel);

        foreach (var traitImage in ImageTraits)
        {
            Destroy(traitImage);
        }
        ImageTraits.Clear();

        foreach (var character in Utility.GetCharactersWithTrait(trait))
        {
            var obj = Instantiate(CharacterTraitProfilePrefab, ImageParent);
            obj.GetComponent<Image>().sprite = character.Sprite;
            ImageTraits.Add(obj);
        }
    }
}
