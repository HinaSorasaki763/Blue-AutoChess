using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    public static PlayerSettings Instance;
    public static int SelectedDropdownValue;


    [SerializeField] private TMPro.TMP_Dropdown LanguageDropdown;
    private List<string> dropdownOptions = new List<string> { "¤¤¤å", "English" };

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
            return;
        }
    }

    private void Start()
    {
        LanguageDropdown.ClearOptions();
        LanguageDropdown.AddOptions(dropdownOptions);
        int savedValue = PlayerPrefs.GetInt("MyDropdownSelection", 0);
        SelectedDropdownValue = savedValue;
        LanguageDropdown.value = savedValue;
    }

    public void OnDropdownValueChanged(int newValue)
    {
        SelectedDropdownValue = newValue;
        PlayerPrefs.SetInt("MyDropdownSelection", newValue);
        PlayerPrefs.Save();
    }

}
