using UnityEngine;
using UnityEditor;

// 自定義的 AssetPostprocessor
public class CustomModelImporter : AssetPostprocessor
{
    // 模型導入前的預處理
    void OnPreprocessModel()
    {
        // 獲取模型導入器
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        // 將單位轉換取消勾選
        modelImporter.useFileUnits = false;

        // 設定材質
        modelImporter.materialLocation = ModelImporterMaterialLocation.External;
        modelImporter.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
    }

    // 動畫導入前的預處理
    void OnPreprocessAnimation()
    {
        // 獲取模型導入器
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        // 設定動畫循環
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clipAnimation in clipAnimations)
        {
            clipAnimation.loopTime = true;
        }
        modelImporter.clipAnimations = clipAnimations;
    }
}

