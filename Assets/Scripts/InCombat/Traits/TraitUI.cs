using GameEnum;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public int characterCount;

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
        descriptionText.text = TraitDescriptions.Instance.Output[trait] + $"  ({characterCount})" + "\n" + GetTraitText(trait, characterCount);
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
    string GetTraitText(Traits trait, int characterCount)
    {
        CharacterObserverBase observer = null;

        switch (trait)
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

        TraitDescriptionData data = TraitDescriptions.Instance.traitDescriptionDatabase.traitDescriptions
            .FirstOrDefault(item => item.trait == trait);

        var sortedThresholds = data.thresholds.OrderBy(t => t.requiredCount).ToList();
        TraitThreshold matchedThreshold = null;
        int level = 0;

        foreach (var threshold in sortedThresholds)
        {
            if (characterCount >= threshold.requiredCount)
            {
                matchedThreshold = threshold;
                level = threshold.level;
            }
            else break;
        }

        if (observer == null)
            return TraitDescriptions.Instance.GetTraitDescription(trait);

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
                string strVal = val.ToString();

                // 高亮當前等級，其他灰色
                if (kvp.Key == level)
                {

                }
                else
                    strVal = $"<color=#808080>{strVal}</color>";

                values.Add(strVal);
            }

            replacements[key] = string.Join("/", values);
        }

        string rawDesc = TraitDescriptions.Instance.GetTraitDescription(trait);
        return StringPlaceholderReplacer.ReplacePlaceholders(rawDesc, replacements);
    }

}
