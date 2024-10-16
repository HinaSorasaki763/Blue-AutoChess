using UnityEngine;
using UnityEditor;

// �۩w�q�� AssetPostprocessor
public class CustomModelImporter : AssetPostprocessor
{
    // �ҫ��ɤJ�e���w�B�z
    void OnPreprocessModel()
    {
        // ����ҫ��ɤJ��
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        // �N����ഫ�����Ŀ�
        modelImporter.useFileUnits = false;

        // �]�w����
        modelImporter.materialLocation = ModelImporterMaterialLocation.External;
        modelImporter.materialName = ModelImporterMaterialName.BasedOnModelNameAndMaterialName;
    }

    // �ʵe�ɤJ�e���w�B�z
    void OnPreprocessAnimation()
    {
        // ����ҫ��ɤJ��
        ModelImporter modelImporter = (ModelImporter)assetImporter;

        // �]�w�ʵe�`��
        ModelImporterClipAnimation[] clipAnimations = modelImporter.defaultClipAnimations;
        foreach (ModelImporterClipAnimation clipAnimation in clipAnimations)
        {
            clipAnimation.loopTime = true;
        }
        modelImporter.clipAnimations = clipAnimations;
    }
}

