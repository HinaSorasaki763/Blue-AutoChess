using UnityEngine;
using System.Collections.Generic;
using GameEnum;
using UnityEngine.UI;

public class TraitDescriptions : MonoBehaviour
{
    public static TraitDescriptions Instance { get; private set; }

    public TraitDescriptionDatabase traitDescriptionDatabase;

    private Dictionary<Traits, string> traitDescriptions;
    private Dictionary<Traits, Sprite> traitImages;
    private Dictionary<Traits, bool> isAcademy;

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
        traitImages = new();
        isAcademy = new();
        // 從可編程物件中加載描述
        foreach (var traitData in traitDescriptionDatabase.traitDescriptions)
        {
            traitDescriptions[traitData.trait] = traitData.descrtipns[PlayerSettings.SelectedDropdownValue];
            traitImages[traitData.trait] = traitData.sprite;
            isAcademy[traitData.trait] = traitData.IsAcademy;
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
