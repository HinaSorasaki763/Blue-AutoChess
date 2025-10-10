using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    // �C���������Ī��ߤ@����
    private Dictionary<string, AudioSource> activeGroups = new Dictionary<string, AudioSource>();

    // �Ω�O���C�� clip ���N�o
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
    /// ���񭵮ġA�Y�P�������Ĥw�s�b�h�Ȱ��è��N�C
    /// </summary>
    public bool RequestAudioPlay(string groupKey, AudioSource source, AudioClip clip, float volume = 0.5f, float cooldownTime = defaultCooldownTime)
    {
        if (clip == null) return false;
        if (groupKey == "pickedUp") cooldownTime = 0;
        // �N�o�P�w
        if (cooldowns.TryGetValue(clip, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < cooldownTime)
            {
                Debug.Log($"���� {clip.name} �N�o���A�L�k����");
                return false;
            }
        }

        // �Y�Ӹs�դw�����Ħb���A���Ȱ��ª�
        if (activeGroups.TryGetValue(groupKey, out AudioSource existing))
        {
            if (existing != null && existing.isPlaying)
            {
                existing.Pause();
            }
            activeGroups.Remove(groupKey);
        }

        // ����s����
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
