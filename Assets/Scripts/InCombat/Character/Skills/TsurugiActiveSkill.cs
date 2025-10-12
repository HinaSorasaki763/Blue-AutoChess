using GameEnum;
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
        var observer = Parent.traitController.GetObserverForTrait(Traits.Barrage) as BarrageObserver;
        float val = observer.CastTimes * 20;
        Effect Attackeffect = EffectFactory.StatckableStatsEffct(0, "TsurugiBarrage", val, StatsType.Attack, Parent, true);
        Attackeffect.SetActions(
            (character) => character.ModifyStats(StatsType.Attack, Attackeffect.Value, Attackeffect.Source),
            (character) => character.ModifyStats(StatsType.Attack, -Attackeffect.Value, Attackeffect.Source)
        );
        Enhancing = true;
        Animator.SetBool(attackType, true);
        Parent.ManaLock = true;
        Effect effect = EffectFactory.StatckableStatsEffct(0, "TsurugiSkill", 50, StatsType.DamageIncrease, Parent, true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.DamageIncrease, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.DamageIncrease, -effect.Value, effect.Source)
        );
        Parent.effectCTRL.AddEffect(effect, Parent);
    }
    public void ResetAttackType()
    {
        Enhancing = false;
        Animator.SetBool(attackType, false);
        Parent.ManaLock = false;
        Parent.effectCTRL.ClearEffectWithSource("TsurugiSkill");
    }
}
