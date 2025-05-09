using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TsurugiActiveSkill : MonoBehaviour
{
    public Animator Animator;
    private readonly string attackType = "Attack_Special";
    public int SpecialAttackCount = 0;
    public bool Enhancing;
    public CharacterCTRL Parent;
    public void ChangeToSpecialAttack()
    {
        Enhancing = true;
        Animator.SetBool(attackType, true);
        Parent.ManaLock = true;
        if (Parent.characterObserver is TsurugiObserver tsurugi)
        {
            tsurugi.DamageIncrease += 50;
        }
    }
    public void ResetAttackType()
    {
        Enhancing = false;
        Animator.SetBool(attackType, false);
        Parent.ManaLock = false;
        if (Parent.characterObserver is TsurugiObserver tsurugi)
        {
            tsurugi.DamageIncrease -= 50;
        }
    }
}
