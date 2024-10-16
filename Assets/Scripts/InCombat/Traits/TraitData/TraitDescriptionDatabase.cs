using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TraitDescriptionDatabase", menuName = "Traits/TraitDescriptionDatabase")]
public class TraitDescriptionDatabase : ScriptableObject
{
    public List<TraitDescriptionData> traitDescriptions = new List<TraitDescriptionData>();
}
