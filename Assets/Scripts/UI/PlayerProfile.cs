// PlayerProfile.cs
using System;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public string PlayerName;
    public string PlayerId;
    public string FavoriteLineupId;
    public string FavoriteBackgroundId;
    public string FavoriteCharacterId;

    const string Key = "IDCARD_PROFILE_V1";

    public static PlayerProfile LoadOrCreate()
    {
        if (PlayerPrefs.HasKey(Key))
        {
            var json = PlayerPrefs.GetString(Key);
            return JsonUtility.FromJson<PlayerProfile>(json);
        }
        var p = new PlayerProfile
        {
            PlayerName = "Player",
            PlayerId = GenerateId(),
            FavoriteLineupId = "",
            FavoriteBackgroundId = "",
            FavoriteCharacterId = ""
        };
        Save(p);
        return p;
    }

    public static void Save(PlayerProfile p)
    {
        var json = JsonUtility.ToJson(p);
        PlayerPrefs.SetString(Key, json);
        PlayerPrefs.Save();
    }

    static string GenerateId()
    {
        var g = Guid.NewGuid().ToString("N");
        return g[..12].ToUpperInvariant();
    }
}
