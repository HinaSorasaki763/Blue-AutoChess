using GameEnum;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamEditorUI : MonoBehaviour
{
    [SerializeField] private TraitDescriptionDatabase traitDescriptionDatabase;
    [SerializeField] private Transform traitButtonParent;   // 放 Trait/Level 按鈕的父物件
    [SerializeField] private Transform traitButtonPrefab;   // Trait 按鈕預置
    [SerializeField] private Transform characterButtonParent; // 放角色按鈕的父物件
    [SerializeField] private Transform characterButtonPrefab; // 角色按鈕預置
    [SerializeField] private TextMeshProUGUI TraitsName;
    [SerializeField] private TextMeshProUGUI TraitsEffect;

    private void Start()
    {
        CreateTraitButtons();
        CreateLevelButtons();
    }

    void CreateTraitButtons()
    {
        // 排序：先 IsAcademy，再依 trait 名稱字母排序
        var sorted = traitDescriptionDatabase.traitDescriptions
            .OrderByDescending(td => td.IsAcademy)
            .ThenBy(td => td.trait.ToString())
            .ToList();

        foreach (var traitData in sorted)
        {
            var btn = Instantiate(traitButtonPrefab, traitButtonParent);

            // 設定文字
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = traitData.trait.ToString();

            // Trait 圖標
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

            // 綁定事件
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

            // 顯示 Level 文字
            var text = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = $"Level {lvl}";

            // 移除 Trait 圖標
            var traitObj = btn.Find("Trait");
            if (traitObj != null) traitObj.gameObject.SetActive(false);

            // 綁定事件
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
        CharacterObserverBase observer = null;
        switch (data.trait)
        {
            case Traits.Abydos: observer = new AbydosObserver(0, null); break;
            case Traits.Gehenna: observer = new GehennaObserver(0, null); break;
            case Traits.Hyakkiyako: observer = new HyakkiyakoObserver(0, null); break;
            case Traits.Millennium: observer = new MillenniumObserver(0, null); break;
            case Traits.Trinity: observer = new TrinityObserver(0, null); break;
            case Traits.Supremacy: observer = new SupermacyObserver(0, null); break;
            case Traits.Precision: observer = new PrecisionObserver(0, null); break;
            case Traits.Barrage: observer = new BarrageObserver(0, null); break;
            case Traits.Aegis: observer = new AegisObserver(0, null); break;
            case Traits.Healer: observer = new HealerObserver(0, null); break;
            case Traits.Disruptor: observer = new DisruptorObserver(0, null); break;
            case Traits.RapidFire: observer = new RapidfireObserver(0, null); break;
            case Traits.logistic: observer = new LogisticObserver(0, null); break;
            case Traits.Mystic: observer = new MysticObserver(0, null); break;
            case Traits.Arius: observer = new AriusObserver(0, null); break;
            case Traits.SRT: observer = new SRTObserver(0, null); break;
            case Traits.None: observer = new NoneObserver(0, null); break;
            default: break;
        }

        if (observer == null)
        {
            TraitsEffect.text = TraitDescriptions.Instance.GetTraitDescription(data.trait);
            return;
        }

        Dictionary<int, TraitLevelStats> statsByLevel = observer.GetTraitObserverLevel();
        var replacements = new Dictionary<string, string>();
        foreach (int i in Enumerable.Range(1, 5))
        {
            string key = $"data{i}";

            List<string> values = new List<string>();

            foreach (var kvp in statsByLevel.OrderBy(k => k.Key))
            {
                if (kvp.Key == 0) continue;
                TraitLevelStats stats = kvp.Value;
                object val = typeof(TraitLevelStats).GetField($"Data{i}").GetValue(stats);
                values.Add(val.ToString());
            }

            replacements[key] = string.Join("/", values);
        }

        string rawDesc = TraitDescriptions.Instance.GetTraitDescription(data.trait);
        TraitsEffect.text = StringPlaceholderReplacer.ReplacePlaceholders(rawDesc, replacements);
        Traits trait = data.trait;

        foreach (Transform child in characterButtonParent)
            Destroy(child.gameObject);

        var allCharacters = ResourcePool.Instance.Lists.SelectMany(list => list);
        var filtered = allCharacters.Where(c => c.Traits.Contains(trait));

        foreach (var character in filtered)
        {
            var charUI = Instantiate(characterButtonPrefab, characterButtonParent);
            var mainImage = charUI.GetComponentInChildren<Image>();
            if (mainImage != null) mainImage.sprite = character.Sprite;

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

        // 過濾符合 level 的角色
        var filtered = allCharacters.Where(c => c.Level == level);

        // 排序：依角色 traits 的最小 enum 值排序
        var sorted = filtered.OrderBy(c =>
        {
            return c.Traits != null && c.Traits.Count > 0
                ? c.Traits.Min(t => (int)t)
                : int.MaxValue; // 沒有 traits 的排到最後
        });

        foreach (var character in sorted)
        {
            var charUI = Instantiate(characterButtonPrefab, characterButtonParent);

            // 主角頭像
            var mainImage = charUI.GetComponentInChildren<Image>();
            if (mainImage != null) mainImage.sprite = character.Sprite;

            // Traits 子物件
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
