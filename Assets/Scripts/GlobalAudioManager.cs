using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private const int maxSimultaneousSounds = 5; // 最大同時播放的音效數量

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

    public bool RequestAudioPlay(AudioSource source, AudioClip clip, float volume = 0.5f)
    {
        // 如果已達到最大同時播放數量，拒絕播放請求
        if (activeAudioSources.Count >= maxSimultaneousSounds)
        {
            return false;
        }
        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();
        activeAudioSources.Add(source);
        StartCoroutine(RemoveAudioSourceWhenFinished(source));
        return true;
    }

    private System.Collections.IEnumerator RemoveAudioSourceWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        activeAudioSources.Remove(source); // 移除已播放完畢的音效
    }
}
