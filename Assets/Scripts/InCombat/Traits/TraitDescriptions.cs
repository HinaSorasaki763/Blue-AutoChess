using UnityEngine;
using System.Collections.Generic;
using GameEnum;
using UnityEngine.UI;
using System.Linq;

public class TraitDescriptions : MonoBehaviour
{
    public static TraitDescriptions Instance { get; private set; }

    public TraitDescriptionDatabase traitDescriptionDatabase;

    private Dictionary<Traits, string> traitDescriptions;
    private Dictionary<Traits, Sprite> traitImages;
    private Dictionary<Traits, bool> isAcademy;
    public Dictionary<Traits, string> Output;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeTraitDescriptions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeTraitDescriptions()
    {
        traitDescriptions = new Dictionary<Traits, string>();
        traitImages = new Dictionary<Traits, Sprite>();
        isAcademy = new Dictionary<Traits, bool>();
        Output = new Dictionary<Traits, string>();
        // �q�i�s�{���󤤥[���y�z
        foreach (var traitData in traitDescriptionDatabase.traitDescriptions)
        {
            traitDescriptions[traitData.trait] = traitData.descrtipns[PlayerSettings.SelectedDropdownValue];
            traitImages[traitData.trait] = traitData.sprite;
            isAcademy[traitData.trait] = traitData.IsAcademy;

            // �p�G�n�N requiredCount �Ѥp��j�L�� (1/2/3) ���Φ�
            if (traitData.thresholds != null && traitData.thresholds.Count > 0)
            {
                // ���X requiredCount�A�ñƧ�
                List<int> requiredCounts = new();
                foreach (var threshold in traitData.thresholds)
                {
                    if (threshold.requiredCount!=0)
                    {
                        requiredCounts.Add(threshold.requiredCount);
                    }
                }
                requiredCounts.Sort();

                string output = "(" + string.Join("/", requiredCounts) + ")";
                Output[traitData.trait] = output;
                CustomLogger.Log(this, $" {output}");
            }
            else
            {
                CustomLogger.LogWarning(this, $"Trait({traitData.trait}) �S�� thresholds�C");
            }
        }
    }

    public string GetTraitDescription(Traits trait)
    {
        foreach (var item in traitDescriptionDatabase.traitDescriptions)
        {
            traitDescriptions[item.trait] = item.descrtipns[PlayerSettings.SelectedDropdownValue];
        }
        return traitDescriptions.ContainsKey(trait) ? traitDescriptions[trait] : "No description available.";
    }
    public Sprite GetTraitImage(Traits traits)
    {
        return traitImages.ContainsKey(traits) ? traitImages[traits] : null;
    }
    public bool GetTraitIsAcademy(Traits traits)
    {
        return isAcademy[traits];
    }
    

}
