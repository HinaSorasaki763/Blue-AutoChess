using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    // 每種類型音效的唯一播放源
    private Dictionary<string, AudioSource> activeGroups = new Dictionary<string, AudioSource>();

    // 用於記錄每個 clip 的冷卻
    private Dictionary<AudioClip, float> cooldowns = new Dictionary<AudioClip, float>();

    private const float defaultCooldownTime = 3f;

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

    /// <summary>
    /// 播放音效，若同類型音效已存在則暫停並取代。
    /// </summary>
    public bool RequestAudioPlay(string groupKey, AudioSource source, AudioClip clip, float volume = 0.5f, float cooldownTime = defaultCooldownTime)
    {
        if (clip == null) return false;
        if (groupKey == "pickedUp") cooldownTime = 0;
        // 冷卻判定
        if (cooldowns.TryGetValue(clip, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < cooldownTime)
            {
                Debug.Log($"音效 {clip.name} 冷卻中，無法播放");
                return false;
            }
        }

        // 若該群組已有音效在播，先暫停舊的
        if (activeGroups.TryGetValue(groupKey, out AudioSource existing))
        {
            if (existing != null && existing.isPlaying)
            {
                existing.Pause();
            }
            activeGroups.Remove(groupKey);
        }

        // 播放新音效
        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();

        cooldowns[clip] = Time.time;
        activeGroups[groupKey] = source;

        StartCoroutine(RemoveWhenFinished(groupKey, source));
        return true;
    }

    private System.Collections.IEnumerator RemoveWhenFinished(string key, AudioSource source)
    {
        while (source != null && source.isPlaying)
            yield return null;

        if (activeGroups.TryGetValue(key, out AudioSource current) && current == source)
        {
            activeGroups.Remove(key);
        }
    }
}
