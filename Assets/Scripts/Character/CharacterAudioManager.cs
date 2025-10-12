using System.Collections.Generic;
using UnityEngine;

public class CharacterAudioManager : MonoBehaviour
{
    public AudiosSO Audio;
    private AudioSource audioSource;
    private float lastDodgedPlayTime;
    private const float dodgedCooldownTime = 7f;
    private const float clipCooldownTime = 3f;

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
            lastDodgedPlayTime = currentTime; // 更新最後播放時間
            return true;
        }
        return false;
    }
    public float PlayClip(AudioClip clip,string name)
    {
        if (clip == null) return 0;
        bool approved = GlobalAudioManager.Instance.RequestAudioPlay(gameObject,name, audioSource, clip, 0.3f, clipCooldownTime);
        if (!approved)
        {
            Debug.Log($"播放請求被拒絕：音效 {clip.name} 無法播放（冷卻中或超過同時播放上限）");
        }
        return audioSource.clip != null ? audioSource.clip.length : 0;
    }

    public void PlayDodgedSound()
    {
        if (Audio == null || Audio.Dodged == null) return;
        if (CanPlayDodged())
        {
            PlayClip(Audio.Dodged, "Dodged");
        }
    }

    public void PlayPickedUpSound()
    {
        if (Audio == null || Audio.PickedUp == null) return;
        PlayClip(Audio.PickedUp, "PickedUp");
    }

    public void PlayHpRestoredSound()
    {
        if (Audio == null || Audio.HpRestored == null) return;
        PlayClip(Audio.HpRestored, "HpRestored");
    }

    public void PlayBuffedSound()
    {
        if (Audio == null || Audio.Buffed == null) return;
        PlayClip(Audio.Buffed, "Buffed");
    }

    public void PlaySummonedSound()
    {
        if (Audio == null || Audio.Summoned == null) return;
        PlayClip(Audio.Summoned, "Summoned");
    }

    public void PlayOnStarUp()
    {
        if (Audio == null || Audio.StarUp.Count == 0) return;
        int rand = Random.Range(0, Audio.StarUp.Count);
        PlayClip(Audio.StarUp[rand], "StarUp");
    }

    public void PlayOnVictory()
    {
        if (Audio == null || Audio.OnVictory.Count == 0) return;
        int rand = Random.Range(0, Audio.OnVictory.Count);
        PlayClip(Audio.OnVictory[rand], "OnVictory");
    }
    public void PlayOnAttack()
    {
        if (Audio == null || Audio.OnAttack.Count == 0) return;
        int rand = Random.Range(0, Audio.OnAttack.Count);
        PlayClip(Audio.OnAttack[rand], "OnAttack");
    }
    public void PlayCastExSkillSound()
    {
        if (Audio == null || Audio.ExSkillAudios.Count == 0) return;
        int rand = Random.Range(0, Audio.ExSkillAudios.Count);
        PlayClip(Audio.ExSkillAudios[rand], "ExSkillAudios");
    }

    public void PlayCrowdControlledSound()
    {
        if (Audio == null || Audio.CrowdControlled.Count == 0) return;
        int rand = Random.Range(0, Audio.CrowdControlled.Count);
        PlayClip(Audio.CrowdControlled[rand], "CrowdControlled");
    }

    public float PlayOnDefeatedSound()
    {
        if (Audio == null || Audio.OnDefeated.Count == 0) return 0;
        int rand = Random.Range(0, Audio.OnDefeated.Count);
        return PlayClip(Audio.OnDefeated[rand], "OnDefeated");
    }
}
