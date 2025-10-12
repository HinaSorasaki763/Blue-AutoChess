using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;
    private Dictionary<GameObject, Dictionary<string, AudioSource>> activeGroups
        = new Dictionary<GameObject, Dictionary<string, AudioSource>>();


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
    public bool RequestAudioPlay(GameObject owner, string groupKey, AudioSource source, AudioClip clip, float volume = 0.5f, float cooldownTime = defaultCooldownTime)
    {
        if (clip == null) return false;
        if (groupKey == "PickedUp") cooldownTime = 0;

        if (cooldowns.TryGetValue(clip, out float lastPlayTime))
            if (Time.time - lastPlayTime < cooldownTime) return false;

        if (!activeGroups.TryGetValue(owner, out var groupDict))
            activeGroups[owner] = groupDict = new Dictionary<string, AudioSource>();

        if (groupDict.TryGetValue(groupKey, out AudioSource existing) && existing != null && existing.isPlaying)
        {
            existing.Pause();
            groupDict.Remove(groupKey);
        }

        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();

        cooldowns[clip] = Time.time;
        groupDict[groupKey] = source;

        StartCoroutine(RemoveWhenFinished(owner, groupKey, source));
        return true;
    }

    private System.Collections.IEnumerator RemoveWhenFinished(GameObject owner, string key, AudioSource source)
    {
        while (source != null && source.isPlaying)
            yield return null;

        if (activeGroups.TryGetValue(owner, out var groupDict))
            if (groupDict.TryGetValue(key, out var current) && current == source)
                groupDict.Remove(key);
    }
}
