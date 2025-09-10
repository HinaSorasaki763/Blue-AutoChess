using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Panchan_AnimatorCTRL : CustomAnimatorController
{
    public Animator WalkAnimator;
    public Animator AttackAnimator;
    public GameObject WalkForm;
    public GameObject AttackForm;
    public StatsContainer StatsContainer = new StatsContainer();
    public override void Start()
    {
        animationLock = false;
        animator = WalkAnimator;
        character = GetComponent<CharacterCTRL>();
        SwitchForm(true);
    }
    public override void SetToIdle()
    {
        SwitchForm(true);
        base.SetToIdle();

    }
    public override void ChangeState(CharacterState newState)
    {
        if (newState == CharacterState.Attacking) SwitchForm(false);
        if (newState == CharacterState.Moving)
        {
            SwitchForm(true);
            animator.SetBool("Idling", false);
            animator.SetBool("Moving", true);
        }
        if (newState == CharacterState.Idling)
        {
            SwitchForm(true);
            animator.SetBool("Idling", true);
            animator.SetBool("Moving", false);
        }
        if (!TryGetBool(animator, newState.ToString(), out bool val)) return;
        if (newState == CharacterState.CastSkill) return;

        base.ChangeState(newState);
    }
    public StatsContainer GetExtraStats()
    {
        int val = PressureManager.Instance.GetPressure(character.IsAlly);
        StatsContainer stats = new StatsContainer();
        stats.AddValue(StatsType.Attack, val * 0.5f);
        stats.AddValue(StatsType.Health, val * 5f);
        stats.AddValue(StatsType.AttackSpeed, val * 0.01f);
        return stats;
    }
    public void SwitchForm(bool walk)
    {

        AttackForm.SetActive(!walk);
        if (!walk)
        {
            AttackForm.GetComponent<Panchan_AttackCTRL>().parent = character;
        }
        WalkForm.SetActive(walk);
        animator = walk ? WalkAnimator : AttackAnimator;
    }
}
