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
        if (!TryGetBool(animator, newState.ToString(), out bool val)) return;
        if (newState == CharacterState.CastSkill) return;

        base.ChangeState(newState);
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
