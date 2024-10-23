using GameEnum;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class CustomAnimatorController : MonoBehaviour
{
    private CharacterState currentState;
    public Animator animator;
    const string skillIndex = "SkillID";
    private CharacterCTRL character;
    void Start()
    {
        animator = GetComponent<Animator>();
        character =GetComponent<CharacterCTRL>();
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

    }
    public (string, float) GetAnimationClipInfo(int order)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;

        List<AnimationClip> clips = new List<AnimationClip>();
        if (controller is AnimatorOverrideController overrideController)
        {
            // 如果有Animator Override Controller，获取覆盖后的动画剪辑
            List<KeyValuePair<AnimationClip, AnimationClip>> overrideClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrideClips);
            foreach (var clipPair in overrideClips)
            {
                clips.Add(clipPair.Value ?? clipPair.Key);
            }
        }
        else
        {
            // 如果没有Animator Override Controller，直接获取Animator Controller的动画剪辑
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
    public void ExitBattle()
    {
        animator.SetBool("HaveTarget",false);
    }
    public void HaveTarget(bool havetarget)
    {
        animator.SetBool("HaveTarget", havetarget);
    }
    public void AfterCastSkill()
    {
        animator.SetBool("CastSkill", false);
        if (character.isShirokoTerror)
        {
            animator.SetInteger(skillIndex, 0);
            return;
        }

    }
    public void SetToIdle()
    {
        animator.SetBool("Idling", true);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);
        animator.SetBool("HaveTarget", false);
        animator.SetBool("CastSkill", false);
    }
    public void ChangeState(CharacterState newState)
    {
        CharacterState oldState = currentState;
        if (oldState == newState) return;
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
                animator.SetBool("Attacking", true);
                animator.SetBool("HaveTarget", true);
                break;
            case CharacterState.PickedUp:
                animator.SetBool("PickedUp", true);
                break;
            case CharacterState.CastSkill:
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
    public (CharacterState,bool) GetState()
    {
        bool canAttack = false;
        if (currentState == CharacterState.Attacking)
        {
            canAttack = true;
        }
        return (currentState,canAttack);
    }
}
