using GameEnum;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamEditorUI : MonoBehaviour
{
    [SerializeField] private TraitDescriptionDatabase traitDescriptionDatabase;
    [SerializeField] private Transform traitButtonParent;   // �� Trait ���s��������
    [SerializeField] private Transform traitButtonPrefab;   // Trait ���s�w�m
    [SerializeField] private Transform characterButtonParent; // �񨤦���s��������
    [SerializeField] private Transform characterButtonPrefab; // ������s�w�m
    [SerializeField] private TextMeshProUGUI TraitsName;
    [SerializeField] private TextMeshProUGUI TraitsEffect;

    private void Start()
    {
        CreateTraitButtons();
    }

    void CreateTraitButtons()
    {
        // �ƧǡG�� IsAcademy�A�A�� trait �W�٦r���Ƨ�
        var sorted = traitDescriptionDatabase.traitDescriptions
            .OrderByDescending(td => td.IsAcademy)   // true �b�e
            .ThenBy(td => td.trait.ToString())       // �A�̦W��
            .ToList();

        foreach (var traitData in sorted)
        {
            var btn = Instantiate(traitButtonPrefab, traitButtonParent);

            // �]�w��r
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = traitData.trait.ToString();

            // Trait �ϼ�
            var traitObj = btn.Find("Trait");
            if (traitObj != null)
            {
                var traitImg = traitObj.GetComponent<Image>();
                if (traitImg != null)
                {
                    traitImg.sprite = traitData.sprite;
                    traitImg.color = traitData.IsAcademy ? Color.white : Color.black;
                }
            }

            // �j�w�ƥ�
            var button = btn.GetComponentInChildren<Button>();
            if (button != null)
                button.onClick.AddListener(() => ShowCharactersWithTrait(traitData));
        }
    }


    void ShowCharactersWithTrait(TraitDescriptionData data)
    {
        TraitsName.text = data.name;
        TraitsEffect.text = TraitDescriptions.Instance.GetTraitDescription(data.trait);
        Traits trait = data.trait;
        foreach (Transform child in characterButtonParent)
            Destroy(child.gameObject);

        var allCharacters = ResourcePool.Instance.Lists.SelectMany(list => list);
        var filtered = allCharacters.Where(c => c.Traits.Contains(trait));

        foreach (var character in filtered)
        {
            var charUI = Instantiate(characterButtonPrefab, characterButtonParent);

            // �D���Y��
            var mainImage = charUI.GetComponentInChildren<Image>();
            if (mainImage != null) mainImage.sprite = character.Sprite;

            // Traits �l����
            var traitsRoot = charUI.Find("Traits");
            if (traitsRoot != null)
            {
                var traitImages = traitsRoot.GetComponentsInChildren<Image>(true);
                traitImages = traitImages.Where(img => img.transform != traitsRoot).ToArray();

                var traitSprites = character.Traits
                    .Select(t => traitDescriptionDatabase.traitDescriptions.FirstOrDefault(td => td.trait == t)?.sprite)
                    .Where(s => s != null)
                    .ToList();

                for (int i = 0; i < traitImages.Length; i++)
                {
                    if (i < traitSprites.Count)
                    {
                        traitImages[i].sprite = traitSprites[i];
                        traitImages[i].enabled = true;
                    }
                    else
                    {
                        traitImages[i].enabled = false;
                    }
                }
            }
        }
    }
}
