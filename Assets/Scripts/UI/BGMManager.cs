using UnityEngine;
using System.Collections.Generic;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;  // 用於全域存取

    private AudioSource audioSource;
    public List<AudioClip> bgmClips;    // 將 BGM 音樂拖曳進來
    private bool isPlayingBGM = false;  // 用來記錄目前是否正在播放中
    private int currentIndex = -1;      // 記錄目前播放的 BGM 索引

    private void Awake()
    {
        // 確保此物件在場景切換時不被銷毀，且保持單例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 假設不循環播放 (loop = false) ，當前音樂結束後 isPlaying 會變為 false
        // 檢查：如果正在播放狀態標誌 isPlayingBGM 為 true，但實際上沒有音樂在播 (isPlaying=false)
        if (isPlayingBGM && !audioSource.isPlaying)
        {
            // BGM 播放結束，選擇下一首隨機 BGM 播放
            PlayRandomBGM();
        }
    }

    public void PlayBGM(int index, bool loop = false)
    {
        if (index < 0 || index >= bgmClips.Count) return;

        currentIndex = index;
        audioSource.clip = bgmClips[index];
        audioSource.loop = loop;
        audioSource.Play();
        isPlayingBGM = true;
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
            // 如果目前沒有播放音樂，那就播放第一首（或隨機首）
            if (bgmClips.Count > 0)
            {
                PlayBGM(0, false); // 預設播放第0首，不想固定就換成 PlayRandomBGM();
            }
        }
        else
        {
            // 如果目前有在播，按下後就停止
            StopBGM();
        }
    }

    public void PlayRandomBGM()
    {
        if (bgmClips.Count == 0) return;

        int randomIndex = Random.Range(0, bgmClips.Count);

        // 若不想重複播放同一首，可檢查是否與 currentIndex 相同，若相同就再隨機一次：
        while (randomIndex == currentIndex && bgmClips.Count > 1)
        {
             randomIndex = Random.Range(0, bgmClips.Count);
        }

        PlayBGM(randomIndex, false);
    }

    

}
