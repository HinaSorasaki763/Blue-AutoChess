
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Audio", menuName = "CustomAudio")]
public class AudiosSO : ScriptableObject
{
    public List<AudioClip> ExSkillAudios = new();
    public List<AudioClip> CrowdControlled = new();
    public List<AudioClip> OnDefeated = new();
    public List<AudioClip> OnVictory = new();
    public List<AudioClip> StarUp = new ();
    public List<AudioClip> OnAttack = new ();
    public AudioClip PickedUp;
    public AudioClip Buffed;
    public AudioClip Dodged;
    public AudioClip HpRestored;
    public AudioClip Summoned;

}
