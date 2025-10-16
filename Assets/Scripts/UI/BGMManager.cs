using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource audioSource;
    public List<AudioClip> bgmClips = new();

    public Slider VolumeSlider;
    public TMP_Dropdown MusicList;
    public Toggle RandomModeToggle;

    private bool isPlayingBGM = false;
    private int currentIndex = -1;
    private bool firstSceneLoaded = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 若初始場景 UI 已存在，直接綁定
        if (VolumeSlider && MusicList && RandomModeToggle)
        {
            InitialBindUI();
        }
        else
        {
            // 若一開始 UI 是動態啟用的，啟動協程等待
            StartCoroutine(WaitAndBindUI());
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 第一個場景已在 Start 綁定，不重複
        if (firstSceneLoaded)
        {
            firstSceneLoaded = false;
            return;
        }

        StopAllCoroutines();
        StartCoroutine(WaitAndBindUI());
    }

    private IEnumerator WaitAndBindUI()
    {
        GameObject parent = null, volumeObj = null, dropdownObj = null, toggleObj = null;

        for (float t = 0; t < 5f; t += 0.5f)
        {
            parent = GameObject.Find("Canvas/HelpPanel/VolumeSetting");
            volumeObj = GameObject.Find("Canvas/HelpPanel/VolumeSetting/Volume");
            dropdownObj = GameObject.Find("Canvas/HelpPanel/VolumeSetting/Dropdown");
            toggleObj = GameObject.Find("Canvas/HelpPanel/VolumeSetting/Toggle");

            if (volumeObj && dropdownObj && toggleObj)
                break;

            yield return new WaitForSeconds(0.5f);
        }

        if (volumeObj && dropdownObj && toggleObj)
        {
            VolumeSlider = volumeObj.GetComponent<Slider>();
            MusicList = dropdownObj.GetComponent<TMP_Dropdown>();
            RandomModeToggle = toggleObj.GetComponent<Toggle>();

            InitialBindUI();
            parent.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[BGMManager] 未能找到 VolumeSetting UI，請確認物件啟用時機。");
        }
    }

    private void InitialBindUI()
    {
        VolumeSlider.onValueChanged.RemoveAllListeners();
        MusicList.onValueChanged.RemoveAllListeners();
        RandomModeToggle.onValueChanged.RemoveAllListeners();

        VolumeSlider.onValueChanged.AddListener(_ => VolumeSetting());
        MusicList.onValueChanged.AddListener(OnMusicSelected);
        RandomModeToggle.onValueChanged.AddListener(_ => ApplyPlayMode());

        VolumeSlider.value = 0.75f;
        audioSource.volume = VolumeSlider.value * 0.1f;

        ApplyPlayMode();
        RefreshDropdown();
    }

    private void Update()
    {
        if (!isPlayingBGM) return;
        if (!audioSource.isPlaying && RandomModeToggle && RandomModeToggle.isOn)
            PlayRandomBGM();
    }

    public void RefreshDropdown()
    {
        if (MusicList == null) return;
        MusicList.ClearOptions();

        var names = new List<string> { "" }; // 第一項空白
        foreach (var c in bgmClips) names.Add(c ? c.name : "Unknown");

        MusicList.AddOptions(names);
        MusicList.value = 0;
        MusicList.RefreshShownValue();
    }

    public void VolumeSetting()
    {
        audioSource.volume = VolumeSlider.value * 0.1f;
    }

    private void OnMusicSelected(int index)
    {
        // 第一項空白：不播放
        if (index <= 0)
        {
            StopBGM();
            return;
        }

        int realIndex = index - 1;
        if (realIndex < 0 || realIndex >= bgmClips.Count) return;

        PlayBGM(realIndex, loop: !RandomModeToggle.isOn);
    }

    private void ApplyPlayMode()
    {
        if (RandomModeToggle == null) return;
        audioSource.loop = !RandomModeToggle.isOn;
    }

    public void PlayBGM(int index, bool loop = false)
    {
        if (index < 0 || index >= bgmClips.Count) return;

        currentIndex = index;
        audioSource.clip = bgmClips[index];
        audioSource.loop = loop;
        audioSource.Play();
        isPlayingBGM = true;

        // Dropdown 對應實際項目 +1
        if (MusicList && MusicList.value != index + 1)
        {
            MusicList.value = index + 1;
            MusicList.RefreshShownValue();
        }
    }

    public void StopBGM()
    {
        audioSource.Stop();
        isPlayingBGM = false;
        currentIndex = -1;
    }

    public void ToggleBGM()
    {
        if (!isPlayingBGM)
        {
            if (bgmClips.Count > 0) PlayBGM(0, loop: !RandomModeToggle.isOn);
        }
        else
        {
            StopBGM();
        }
    }

    public void PlayRandomBGM()
    {
        if (bgmClips.Count == 0) return;

        int randomIndex = Random.Range(0, bgmClips.Count);
        while (randomIndex == currentIndex && bgmClips.Count > 1)
            randomIndex = Random.Range(0, bgmClips.Count);

        PlayBGM(randomIndex, loop: false);
    }
}
