using UnityEngine;
using UnityEditor;

public class CustomModelImporter : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        modelImporter.useFileUnits = false;
        modelImporter.materialLocation = ModelImporterMaterialLocation.External;
        modelImporter.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
    }

    void OnPreprocessAnimation()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clipAnimation in clipAnimations)
        {
            clipAnimation.loopTime = true;
        }
        modelImporter.clipAnimations = clipAnimations;
    }

    void OnPreprocessTexture()
    {
        CustomLogger.Log(this, $"OnPreprocessTexture() - {assetPath}");
        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
    }
}
