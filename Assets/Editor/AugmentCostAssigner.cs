using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AugmentCostAssigner : EditorWindow
{
    [MenuItem("Tools/Assign Augment Costs")]
    public static void AssignCosts()
    {
        // 掃描各 cost 資料夾
        Dictionary<int, int> characterCostMap = new Dictionary<int, int>();
        for (int cost = 1; cost <= 5; cost++)
        {
            string path = $"{cost}Cost";
            var characters = Resources.LoadAll<Character>(path);
            foreach (var c in characters)
            {
                if (c == null) continue;
                if (c.CharacterId > 100) continue;
                if (!characterCostMap.ContainsKey(c.CharacterId))
                    characterCostMap.Add(c.CharacterId, cost);
            }
        }

        // 掃描 Augments
        string augmentPath = "Resources/Augments/SkillAugments";
        var augments = Resources.LoadAll<AugmentConfig>("Augments/SkillAugments");
        int modified = 0;

        foreach (var augment in augments)
        {
            if (augment == null) continue;
            int charId = augment.CharacterSkillEnhanceIndex;
            if (charId < 0) continue;

            if (characterCostMap.TryGetValue(charId, out int cost))
            {
                if (augment.cost != cost)
                {
                    augment.cost = cost;
                    EditorUtility.SetDirty(augment);
                    modified++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"更新完成，共修改 {modified} 個 AugmentConfig。");
    }
}
