// BackgroundCatalog.cs
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundCatalog", menuName = "Catalogs/BackgroundCatalog")]
public class BackgroundCatalog : ScriptableObject
{
    [System.Serializable]
    public struct Entry { public string id; public Sprite sprite; }

    [SerializeField] List<Entry> entries = new();
    public IReadOnlyList<Entry> Entries => entries;

    public void LoadFromResources(string folder)
    {
        Debug.Log($"Loading from {folder}");
        entries.Clear();
        var sprites = Resources.LoadAll<Sprite>(folder);
        foreach (var s in sprites)
            entries.Add(new Entry { id = s.name, sprite = s });

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }


    public Sprite GetSprite(string id)
    {
        if (string.IsNullOrEmpty(id)) return entries.Count > 0 ? entries[0].sprite : null;
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == id) return entries[i].sprite;
        return entries.Count > 0 ? entries[0].sprite : null;
    }

    public int IndexOf(string id)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == id) return i;
        return 0;
    }
}
