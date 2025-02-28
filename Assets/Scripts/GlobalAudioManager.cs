using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private Dictionary<AudioClip, float> cooldowns = new Dictionary<AudioClip, float>();
    private const int maxSimultaneousSounds = 5; // �̤j�P�ɼ��񪺭��ļƶq
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
        // �ˬd�N�o�ɶ�
        if (cooldowns.TryGetValue(clip, out float lastPlayTime))
        {
            if (Time.time - lastPlayTime < cooldownTime)
            {
                Debug.Log($"���� {clip.name} �N�o���A�L�k����");
                return false;
            }
        }

        // �ˬd�P�ɼ���W��
        if (activeAudioSources.Count >= maxSimultaneousSounds)
        {
            Debug.Log($"���ļƶq�w�F�W���A�L�k���� {clip.name}");
            return false;
        }

        // �]�w���Ĩü���
        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();

        // ��s�N�o�ɶ�
        cooldowns[clip] = Time.time;

        // �K�[�쬡�ʭ��ĦC��ö}�l������{
        activeAudioSources.Add(source);
        StartCoroutine(RemoveAudioSourceWhenFinished(source));
        return true;
    }
    private System.Collections.IEnumerator RemoveAudioSourceWhenFinished(AudioSource source)
    {
        // �����ˬd source �O�_���ġA�åB�O�_���b����
        while (source != null && source.isPlaying)
        {
            yield return null; // ���ݤU�@�V
        }

        // �T�O source �|���Q�����A���沾���޿�
        if (source != null)
        {
            activeAudioSources.Remove(source);
        }
    }

}
