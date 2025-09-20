using GameEnum;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CustomAnimatorController : MonoBehaviour
{
    private CharacterState currentState;
    public Animator animator;
    const string skillIndex = "SkillID";
    public CharacterCTRL character;
    public float animatorSpeed;
    public bool animationLock = false;
    public virtual void Start()
    {
        animationLock = false;
        character = GetComponent<CharacterCTRL>();
        animator = GetComponent<Animator>();

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
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        animatorSpeed = animator.speed;
        if (stateInfo.IsName("Hina_Original_Normal_Attack_Ing"))
        {
            
        }
        else
        {
        //    animatorSpeed = 1f;
        }

    }
    public (string, float) GetAnimationClipInfo(int order)
    {
        if (character.characterStats.CharacterId == 41) return (null, 0);
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
    public virtual void SetToIdle()
    {
        if (animationLock) return;
        animator.SetBool("Idling", true);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("PickedUp", false);
        animator.SetBool("HaveTarget", false);
        animator.SetBool("CastSkill", false);
    }
    public virtual void ChangeState(CharacterState newState)
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
    public void ForceIdle()
    {
        CustomLogger.Log(this, $"character{character} force idle at {Time.time}");
        animationLock = false;
        animator.speed = 1f;
        currentState = CharacterState.Idling;
        
        animator.SetBool("Idling", true);
        animator.SetBool("Moving", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("HaveTarget", false);
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
    public void CheckStasis()
    {
        if (character.InStasis)
        {
            animator.speed = 0;
        }
    }

    public bool TryGetBool(Animator animator, string paramName, out bool value)
    {
        value = false;
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                value = animator.GetBool(paramName);
                return true;
            }
        }
        return false;
    }
}
