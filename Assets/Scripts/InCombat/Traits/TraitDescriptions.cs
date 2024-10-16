using UnityEngine;
using System.Collections.Generic;
using GameEnum;

public class TraitDescriptions : MonoBehaviour
{
    public static TraitDescriptions Instance { get; private set; }

    public TraitDescriptionDatabase traitDescriptionDatabase; // �ޥ� ScriptableObject

    private Dictionary<Traits, string> traitDescriptions;

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

        // �q�i�s�{���󤤥[���y�z
        foreach (var traitData in traitDescriptionDatabase.traitDescriptions)
        {
            traitDescriptions[traitData.trait] = traitData.descrtipns[1];
        }
    }

    public string GetTraitDescription(Traits trait)
    {
        return traitDescriptions.ContainsKey(trait) ? traitDescriptions[trait] : "No description available.";
    }
}
