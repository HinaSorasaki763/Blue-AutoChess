using GameEnum;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamEditorUI : MonoBehaviour
{
    [SerializeField] private TraitDescriptionDatabase traitDescriptionDatabase;
    [SerializeField] private Transform traitButtonParent;   // �� Trait/Level ���s��������
    [SerializeField] private Transform traitButtonPrefab;   // Trait ���s�w�m
    [SerializeField] private Transform characterButtonParent; // �񨤦���s��������
    [SerializeField] private Transform characterButtonPrefab; // ������s�w�m
    [SerializeField] private TextMeshProUGUI TraitsName;
    [SerializeField] private TextMeshProUGUI TraitsEffect;

    private void Start()
    {
        CreateTraitButtons();
        CreateLevelButtons();
    }

    void CreateTraitButtons()
    {
        // �ƧǡG�� IsAcademy�A�A�� trait �W�٦r���Ƨ�
        var sorted = traitDescriptionDatabase.traitDescriptions
            .OrderByDescending(td => td.IsAcademy)
            .ThenBy(td => td.trait.ToString())
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

    void CreateLevelButtons()
    {
        for (int lvl = 1; lvl <= 5; lvl++)
        {
            var btn = Instantiate(traitButtonPrefab, traitButtonParent);

            // ��� Level ��r
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"Level {lvl}";

            // ���� Trait �ϼ�
            var traitObj = btn.Find("Trait");
            if (traitObj != null) traitObj.gameObject.SetActive(false);

            // �j�w�ƥ�
            var button = btn.GetComponentInChildren<Button>();
            if (button != null)
            {
                int j = lvl;
                button.onClick.AddListener(() => ShowCharactersWithLevel(j));
            }

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
                var traitImages = traitsRoot.GetComponentsInChildren<Image>(true)
                                            .Where(img => img.transform != traitsRoot)
                                            .ToArray();

                var traitSprites = character.Traits
                    .Select(t => traitDescriptionDatabase.traitDescriptions.FirstOrDefault(td => td.trait == t))
                    .Where(td => td != null)
                    .Select(td => td.sprite)
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

    void ShowCharactersWithLevel(int level)
    {
        TraitsName.text = $"Level {level}";
        TraitsEffect.text = string.Empty;

        foreach (Transform child in characterButtonParent)
            Destroy(child.gameObject);

        var allCharacters = ResourcePool.Instance.Lists.SelectMany(list => list);

        // �L�o�ŦX level ������
        var filtered = allCharacters.Where(c => c.Level == level);

        // �ƧǡG�̨��� traits ���̤p enum �ȱƧ�
        var sorted = filtered.OrderBy(c =>
        {
            return c.Traits != null && c.Traits.Count > 0
                ? c.Traits.Min(t => (int)t)
                : int.MaxValue; // �S�� traits ���ƨ�̫�
        });

        foreach (var character in sorted)
        {
            var charUI = Instantiate(characterButtonPrefab, characterButtonParent);

            // �D���Y��
            var mainImage = charUI.GetComponentInChildren<Image>();
            if (mainImage != null) mainImage.sprite = character.Sprite;

            // Traits �l����
            var traitsRoot = charUI.Find("Traits");
            if (traitsRoot != null)
            {
                var traitImages = traitsRoot.GetComponentsInChildren<Image>(true)
                                            .Where(img => img.transform != traitsRoot)
                                            .ToArray();

                var traitSprites = character.Traits
                    .Select(t => traitDescriptionDatabase.traitDescriptions.FirstOrDefault(td => td.trait == t))
                    .Where(td => td != null)
                    .Select(td => td.sprite)
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
