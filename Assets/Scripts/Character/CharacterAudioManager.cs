using System.Collections.Generic;
using UnityEngine;

public class CharacterAudioManager : MonoBehaviour
{
    public AudiosSO Audio;
    private AudioSource audioSource;
    private float lastDodgedPlayTime;
    private const float dodgedCooldownTime = 7f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lastDodgedPlayTime = -dodgedCooldownTime;
    }

    private bool CanPlayDodged()
    {
        float currentTime = Time.time;
        if (currentTime - lastDodgedPlayTime >= dodgedCooldownTime)
        {
            lastDodgedPlayTime = currentTime; // ��s�̫Ἵ��ɶ�
            return true;
        }
        return false;
    }

    public float PlayClip(AudioClip clip)
    {
        if (clip == null) return 0;

        audioSource.clip = clip;
        audioSource.volume = 0.5f;
        audioSource.loop = false;
        audioSource.Play();
        return audioSource.clip.length;
    }

    // �H�U�O�U�ӱ��Ҫ������k
    public void PlayDodgedSound()
    {
        if (Audio == null || Audio.Dodged == null) return;

        // �u���� Dodged ���Ī��N�o�ɶ��w�L�A�~�|����
        if (CanPlayDodged())
        {
            PlayClip(Audio.Dodged);
        }
    }

    public void PlayPickedUpSound()
    {
        if (Audio == null || Audio.PickedUp == null) return;
        PlayClip(Audio.PickedUp);
    }

    public void PlayHpRestoredSound()
    {
        if (Audio == null || Audio.HpRestored == null) return;
        PlayClip(Audio.HpRestored);
    }

    public void PlayBuffedSound()
    {
        if (Audio == null || Audio.Buffed == null) return;
        PlayClip(Audio.Buffed);
    }

    public void PlaySummonedSound()
    {
        if (Audio == null || Audio.Summoned == null) return;
        PlayClip(Audio.Summoned);
    }

    public void PlayOnStarUp()
    {
        if (Audio == null || Audio.StarUp.Count == 0) return;
        int rand = Random.Range(0, Audio.StarUp.Count);
        PlayClip(Audio.StarUp[rand]);
    }

    public void PlayOnVictory()
    {
        if (Audio == null || Audio.OnVictory.Count == 0) return;
        int rand = Random.Range(0, Audio.OnVictory.Count);
        PlayClip(Audio.OnVictory[rand]);
    }

    public void PlayCastExSkillSound()
    {
        if (Audio == null || Audio.ExSkillAudios.Count == 0) return;
        int rand = Random.Range(0, Audio.ExSkillAudios.Count);
        PlayClip(Audio.ExSkillAudios[rand]);
    }

    public void PlayCrowdControlledSound()
    {
        if (Audio == null || Audio.CrowdControlled.Count == 0) return;
        int rand = Random.Range(0, Audio.CrowdControlled.Count);
        PlayClip(Audio.CrowdControlled[rand]);
    }

    public float PlayOnDefeatedSound()
    {
        if (Audio == null || Audio.OnDefeated.Count == 0) return 0;
        int rand = Random.Range(0, Audio.OnDefeated.Count);
        return PlayClip(Audio.OnDefeated[rand]);
    }
}
