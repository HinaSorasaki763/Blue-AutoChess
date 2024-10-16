using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TraitsEffectManager : MonoBehaviour
{
    public static TraitsEffectManager Instance { get; private set; }
    public CharacterParent allyParent;
    public CharacterParent enemyParent;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int GetTraitLevelForCharacter(Traits trait, bool isEnemy)
    {
        if (isEnemy && enemyParent.currTraits.ContainsKey(trait))
        {
            return enemyParent.currTraits[trait];
        }
        else if (!isEnemy && allyParent.currTraits.ContainsKey(trait))
        {
            return allyParent.currTraits[trait];
        }

        return 0; // 如果未找到，返回 0 表示未激活
    }
    public Dictionary<Traits, int> CalculateTraitCounts(List<CharacterCTRL> characters)
    {
        Dictionary<Traits, int> traitCounts = new Dictionary<Traits, int>();
        HashSet<int> countedCharacterIds = new HashSet<int>(); // 用來追蹤已經計算過的角色
        StringBuilder sb = new StringBuilder();

        foreach (var character in characters)
        {
            int characterId = character.characterStats.CharacterId;
            if (countedCharacterIds.Contains(characterId))
            {
                continue;
            }

            countedCharacterIds.Add(characterId);
            foreach (var trait in character.traitController.GetCurrentTraits())
            {
                if (!traitCounts.ContainsKey(trait))
                {
                    traitCounts[trait] = 0;
                }

                traitCounts[trait]++;
                sb.AppendLine($"trait {trait} have {traitCounts[trait]}");
            }
        }
        CustomLogger.Log(this, sb.ToString());
        return traitCounts;
    }
}
