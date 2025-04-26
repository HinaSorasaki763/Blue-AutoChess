using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineActiveSkill : MonoBehaviour
{
    public CharacterCTRL Parent;
    public SkillContext Context;
    public HexNode HexNode;
    public int range;
    void Start()
    {
        
    }

    public void HitEnemiesAroundHex()
    {
        bool isAlly = Context.Parent.IsAlly;
        StarLevelStats stats = Parent.ActiveSkill.GetCharacterLevel()[Context.CharacterLevel];
        int duration = stats.Data3;
        var l = Utility.GetCharacterInSet(SpawnGrid.Instance.GetHexNodesWithinRange(HexNode, range), Context.Parent, false);
        foreach (CharacterCTRL Character in l)
        {
            int dmg = Parent.ActiveSkill.GetAttackCoefficient(Context);
            (bool isCrit, int dmg1) = Context.Parent.CalculateCrit(dmg);
            Character.GetHit(dmg1, Context.Parent, "MineEnhancedSkill", isCrit);
            Effect minusAttackEffect = EffectFactory.StatckableStatsEffct(5, "MineEnhancedSkill", -50, StatsType.Attack, Character, false);
            minusAttackEffect.SetActions(
                (character) => character.ModifyStats(StatsType.Attack, minusAttackEffect.Value, minusAttackEffect.Source),
                (character) => character.ModifyStats(StatsType.Attack, -minusAttackEffect.Value, minusAttackEffect.Source)
            );
            Character.effectCTRL.AddEffect(minusAttackEffect,Character);
        }
    }
}
