using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSession : MonoBehaviour
{
    public static PlayerSession Instance;

    public PlayerData Data { get; private set; }

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

    public void SetData(string uid, string name)
    {
        Data = new PlayerData { Uid = uid, Name = name };
        SaveLocal();
    }

    public void SaveLocal()
    {
        PlayerPrefs.SetString("player_uid", Data.Uid);
        PlayerPrefs.SetString("player_name", Data.Name);
        PlayerPrefs.Save();
    }

    public void LoadLocal()
    {
        if (PlayerPrefs.HasKey("player_uid"))
        {
            Data = new PlayerData
            {
                Uid = PlayerPrefs.GetString("player_uid"),
                Name = PlayerPrefs.GetString("player_name")
            };
        }
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey("player_uid");
        PlayerPrefs.DeleteKey("player_name");
        Data = null;
    }
}
public class PlayerData
{
    public string Uid;
    public string Name;
}
