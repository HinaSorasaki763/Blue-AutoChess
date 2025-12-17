// Assets/Editor/TempAugmentGenerator.cs
using UnityEditor;
using UnityEngine;
using System.IO;

public class TempAugmentGenerator
{
    private const string TargetFolder = "Assets/Resources/Augments/CommonAugments";
    private const int StartIndex = 1000;
    private const int Count = 51;

    [MenuItem("Tools/Augments/Generate Temp Augments")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(TargetFolder))
        {
            Directory.CreateDirectory(TargetFolder);
            AssetDatabase.Refresh();
        }

        for (int i = 0; i < Count; i++)
        {
            int index = StartIndex + i;
            string assetName = $"TempAugment{index}";
            string assetPath = $"{TargetFolder}/{assetName}.asset";

            if (File.Exists(assetPath))
                continue;

            AugmentConfig config = ScriptableObject.CreateInstance<AugmentConfig>();
            config.augmentName = assetName;
            config.description = assetName;
            config.descriptionEnglish = assetName;
            config.augmentIndex = index;
            config.CharacterSkillEnhanceIndex = -1;
            config.cost = 0;

            AssetDatabase.CreateAsset(config, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
