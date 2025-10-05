// CatalogBootstrap.cs
using UnityEngine;

public class CatalogBootstrap : MonoBehaviour
{
    [SerializeField] BackgroundCatalog backgroundCatalog;
    [SerializeField] CharacterCatalog characterCatalog;
    [SerializeField] string backgroundsResourcesFolder = "IDCard/Backgrounds";
    [SerializeField] string charactersResourcesFolder = "IDCard/Characters";

    void Start()
    {
        if (backgroundCatalog != null) backgroundCatalog.LoadFromResources(backgroundsResourcesFolder);
        if (characterCatalog != null) characterCatalog.LoadFromResources(charactersResourcesFolder);
    }
}
