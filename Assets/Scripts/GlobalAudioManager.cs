using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private Dictionary<AudioClip, float> cooldowns = new Dictionary<AudioClip, float>();
    private const int maxSimultaneousSounds = 5; // 最大同時播放的音效數量
    private const float defaultCooldownTime = 1f;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool RequestAudioPlay(AudioSource source, AudioClip clip, float volume = 0.5f, float cooldownTime = defaultCooldownTime)
    {
        // 檢查冷卻時間
        if (cooldowns.TryGetValue(clip, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < cooldownTime)
            {
                Debug.Log($"音效 {clip.name} 冷卻中，無法播放");
                return false;
            }
        }

        // 檢查同時播放上限
        if (activeAudioSources.Count >= maxSimultaneousSounds)
        {
            Debug.Log($"音效數量已達上限，無法播放 {clip.name}");
            return false;
        }

        // 設定音效並播放
        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();

        // 更新冷卻時間
        cooldowns[clip] = Time.time;

        // 添加到活動音效列表並開始移除協程
        activeAudioSources.Add(source);
        StartCoroutine(RemoveAudioSourceWhenFinished(source));
        return true;
    }
    private System.Collections.IEnumerator RemoveAudioSourceWhenFinished(AudioSource source)
    {
        // 持續檢查 source 是否有效，並且是否仍在播放
        while (source != null && source.isPlaying)
        {
            yield return null; // 等待下一幀
        }

        // 確保 source 尚未被移除再執行移除邏輯
        if (source != null)
        {
            activeAudioSources.Remove(source);
        }
    }

}
