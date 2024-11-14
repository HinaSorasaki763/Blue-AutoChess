using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private const int maxSimultaneousSounds = 5; // �̤j�P�ɼ��񪺭��ļƶq

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
        // �p�G�w�F��̤j�P�ɼ���ƶq�A�ڵ�����ШD
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
        activeAudioSources.Remove(source); // �����w���񧹲�������
    }
}
