using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CustomAnimatorController : MonoBehaviour
{
    private CharacterState currentState;
    public Animator animator;
    const string skillIndex = "SkillID";
    private CharacterCTRL character;
    public float animatorSpeed;
    public bool animationLock = false;
    void Start()
    {
        animationLock = false;
        animator = GetComponent<Animator>();
        character = GetComponent<CharacterCTRL>();
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        int i = 0;
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var clip in controller.animationClips)
        {

            stringBuilder.AppendLine($"Clip{i}: {clip.name}, Length: {clip.length}");
            i++;
        }
        CustomLogger.Log(this, stringBuilder.ToString());
    }
    // Update is called once per frame
    void Update()
    {
        animatorSpeed = animator.speed;
    }
    public (string, float) GetAnimationClipInfo(int order)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        List<AnimationClip> clips = new List<AnimationClip>();
        if (controller is AnimatorOverrideController overrideController)
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrideClips);
            foreach (var clipPair in overrideClips)
            {
                clips.Add(clipPair.Value ?? clipPair.Key);
            }
        }
        else
        {
            clips.AddRange(controller.animationClips);
        }

        if (order >= 0 && order < clips.Count)
        {
            AnimationClip clip = clips[order];
            return (clip.name, clip.length);
        }
        else
        {
            Debug.LogError("指定的动画顺序超出范围");
            return (null, 0);
        }
    }
    public List<(string, float)> GetAllClipInfos()
    {
        List<(string, float)> results = new List<(string, float)>();

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        List<AnimationClip> clips = new List<AnimationClip>();

        if (controller is AnimatorOverrideController overrideController)
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrideClips);
            foreach (var clipPair in overrideClips)
            {
                clips.Add(clipPair.Value ?? clipPair.Key);
            }
        }
        else
        {
            clips.AddRange(controller.animationClips);
        }

        foreach (var clip in clips)
        {
            results.Add((clip.name, clip.length));
        }

        return results;
    }
    public void ExitBattle()
    {
        if (animationLock) return;
        animator.SetBool("HaveTarget", false);
    }
    public void HaveTarget(bool havetarget)
    {
        if (animationLock) return;
        animator.SetBool("HaveTarget", havetarget);
    }
    public void AfterCastSkill()
    {
        if (animationLock) return;
        animator.SetBool("CastSkill", false);
        if (character.isShirokoTerror)
        {
            animator.SetInteger(skillIndex, 0);
            return;
        }

    }
    public void SetToIdle()
    {
        if (animationLock) return;
        animator.SetBool("Idling", true);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);
        animator.SetBool("HaveTarget", false);
        animator.SetBool("CastSkill", false);
    }
    public void ChangeState(CharacterState newState)
    {
        if (animationLock) return;
        CharacterState oldState = currentState;
        if (oldState == newState) return;
        CustomLogger.Log(this, $"character{character} from state {currentState} change to {newState} at {Time.time}");
        animator.speed = 1f;
        animator.SetBool("Idling", false);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);

        switch (newState)
        {
            case CharacterState.Idling:
                animator.SetBool("Idling", true);
                break;
            case CharacterState.Moving:
                animator.SetBool("Moving", true);
                break;
            case CharacterState.Attacking:
                animator.speed = character.GetStat(StatsType.AttackSpeed);
                animator.SetBool("Attacking", true);
                animator.SetBool("HaveTarget", true);
                break;
            case CharacterState.PickedUp:
                animator.SetBool("PickedUp", true);
                break;
            case CharacterState.CastSkill:
                if (character.characterStats.CharacterId == 16) animator.speed = character.GetStat(StatsType.AttackSpeed);
                animator.SetBool("CastSkill", true);
                break;
            case CharacterState.HaveTarget:
                animator.SetBool("HaveTarget", true);
                break;
            case CharacterState.Stunned:
                animator.SetTrigger("StunTrigger");
                break;
            case CharacterState.Dying:
                animator.SetTrigger("DyingTrigger");
                break;
        }
        currentState = newState; // 更新當前狀態
    }
    public void ForceIdle()
    {
        animationLock = false;
        animator.speed = 1f;
        currentState = CharacterState.Idling;
        animator.SetBool("Idling", true);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);
    }    
    public void ForceDying()
    {
        animationLock = true;
        animator.speed = 1f;
        animator.SetBool("Idling", false);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);
        animator.SetTrigger("DyingTrigger");
    }
    public (CharacterState, bool) GetState()
    {
        bool canAttack = false;
        if (currentState == CharacterState.Attacking)
        {
            canAttack = true;
        }
        return (currentState, canAttack);
    }
    public void SetCharacterFalse()
    {
        PathRequestManager.Instance.ReleaseCharacterReservations(character);
        character.gameObject.SetActive(false);
    }
}
