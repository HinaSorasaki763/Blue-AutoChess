using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class AriusObserver : CharacterObserverBase
{
    public int traitLevel;
    public CharacterCTRL parent;
    public bool IsSonOfGod;
    public bool isactive = false;
    public AriusObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.traitLevel = level;
        this.parent = character;
    }
    public override void ActivateTrait()
    {
        if (SelectedAugments.Instance.CheckAugmetExist(107))
        {
            CharacterParent c = ResourcePool.Instance.ally;
            List<CharacterCTRL> Arius = c.GetCharacterWithTraits(Traits.Arius);
            List<CharacterCTRL> SRT = c.GetCharacterWithTraits(Traits.SRT);
            if (Utility.CompareTwoGroups(SRT, Arius))
            {
                parent.effectCTRL.ClearEffectWithSource("AriusResistanceBuff");
                return;
            }
        }
        int amount = 100 - GetTraitObserverLevel()[traitLevel].Data1;
        Effect effect = EffectFactory.UnStatckableStatsEffct(20, "AriusResistanceBuff", amount, StatsType.PercentageResistence, parent, true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.PercentageResistence, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.PercentageResistence, -effect.Value, effect.Source)
        );
        parent.effectCTRL.AddEffect(effect, parent);
        CharacterParent Characterparent = parent.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        if (SelectedAugments.Instance.CheckAugmetExist(125))
        {

        }
        foreach (var item in Characterparent.GetCharacterWithTraits(Traits.Trinity))
        {
            item.effectCTRL.AddEffect(effect, item);
        }
        isactive = true;
        if (!(SelectedAugments.Instance.CheckAugmetExist(100) || SelectedAugments.Instance.CheckAugmetExist(125)))
        {
            AriusManager.Instance.GetSelector();
        }

    }
    public override void DeactivateTrait()
    {
        parent.effectCTRL.ClearEffectWithSource("AriusResistanceBuff");
        isactive = false;
        AriusManager.Instance.RemoveSelector();
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//傷害減免，造成傷害回復生命，聖子的生命/攻擊力/攻擊速度加成
            {0, new TraitLevelStats(100,0,0)},
            {1, new TraitLevelStats(90,5,30)},
            {2, new TraitLevelStats(80,10,40)},
            {3, new TraitLevelStats(70,15,50)},
            {4, new TraitLevelStats(60,15,50)},
        };
        return statsByStarLevel;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion = true)
    {
        CustomLogger.Log(this, $"GetHit invoked. Character: {character.name}, Source: {source.name}, Amount: {amount}, IsCrit: {isCrit}, Detail: {detailedSource}");


        if (isactive)
        {
            List<CharacterCTRL> c = AriusManager.Instance.GetOnBoard(false, character.IsAlly);
            int count = c.Count;
            float FinalAmount = amount * GetTraitObserverLevel()[traitLevel].Data1 * 0.01f / count;

            CustomLogger.Log(this, $"Arius Observer activated. Calculated percent: {GetTraitObserverLevel()[traitLevel].Data1}, OnBoard count: {count}, Final damage amount: {FinalAmount}");

            foreach (var item in c)
            {
                if (item != AriusManager.Instance.SonOfGod)
                {
                    CustomLogger.Log(this, $"Applying damage of {(int)FinalAmount} to {item.name} via Arius Observer.");
                    item.RemoveHealth((int)FinalAmount, "Arius Observer");
                }
            }
        }
        else
        {
            CustomLogger.Log(this, "Arius Observer not active. GetHit execution bypassed.");
        }
    }

    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage, string detailedSource, bool iscrit)
    {
        bool active = false;
        if (SelectedAugments.Instance.CheckAugmetExist(108))
        {
            CharacterCTRL god = AriusManager.Instance.SonOfGod;
            int healAmount = (int)(damage * GetTraitObserverLevel()[traitLevel].Data2 * 0.01f);
            god.Heal(healAmount, parent);
            return;
        }
        else if (SelectedAugments.Instance.CheckAugmetExist(100))
        {
            CharacterParent characterParent = source.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            CharacterCTRL god = characterParent.GetStrongestCharacterByID(26);
            if (god != null)
            {
                active = true;
                int healAmount = (int)(damage * GetTraitObserverLevel()[traitLevel].Data2 * 0.01f);
                god.Heal(healAmount, parent);
            }
        }
        if (isactive && !active)
        {
            CharacterCTRL lowest = null;
            int low = int.MaxValue;
            foreach (var item in AriusManager.Instance.GetOnBoard(true, source.IsAlly))
            {
                if (item.GetStat(StatsType.currHealth) < low)
                {
                    low = (int)item.GetStat(StatsType.currHealth);
                    lowest = item;
                }
            }
            int healamount = (int)(damage * GetTraitObserverLevel()[traitLevel].Data2 * 0.01f);
            lowest.Heal(healamount, parent);
        }
        if (SelectedAugments.Instance.CheckAugmetExist(120))
        {
            if (Utility.CheckExecuted(target, source, GetTraitObserverLevel()[traitLevel].Data1 * 0.01f, detailedSource))
            {
                DataStackManager.Instance.AddDataStack(10);
            }
        }
    }
    public void SetGodOfSon(bool b)
    {
        CustomLogger.Log(this, $"set {parent} to {b}");
        IsSonOfGod = b;
        var observer = parent.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
        if (!observer.isactive)
        {
            PopupManager.Instance.CreatePopup($"Arius Trait not activating!", 2);
        }
        CustomLogger.Log(this, $" {parent} god of son : {IsSonOfGod} , get from traitctrl {observer.IsSonOfGod}");
        if (b)
        {
            AriusManager.Instance.SonOfGod = parent;
            parent.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
        else
        {
            parent.transform.localScale = Vector3.one;
        }
    }
    public override void OnLeaveBattleField(CharacterCTRL c)
    {
        SetGodOfSon(false);
        base.OnLeaveBattleField(c);
        DeactivateTrait();
        AriusManager.Instance.RemoveArius(c);
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        AriusManager.Instance.AddArius(character);
    }
    public override void OnKilledEnemy(CharacterCTRL character, string detailedSource, CharacterCTRL characterDies)
    {

    }
    public override void CharacterStart(CharacterCTRL character)
    {
        if (!SelectedAugments.Instance.CheckAugmetExist(108)) return;
        if (parent == AriusManager.Instance.SonOfGod)
        {
            CustomLogger.Log(this, $"{character.name} is son of god ,108 activated");
            character.SetStasis();
            character.SetStat(StatsType.currHealth, 1);
            character.customAnimator.ChangeState(CharacterState.Dying);
        }
    }
    public override void CharacterUpdate()
    {
        if (!SelectedAugments.Instance.CheckAugmetExist(108) || !parent.InStasis) return;
        if (Condition())
        {
            parent.customAnimator.animator.Update(0);
            parent.customAnimator.animator.Play("Hina_Original_Normal_Idle", 0, 0f);
            parent.RemoveStasis();
            Effect effect = EffectFactory.StatckableStatsEffct(0, "Augment108", parent.GetHealthPercentage() * 100, StatsType.DamageIncrease, parent, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.DamageIncrease, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.DamageIncrease, -effect.Value, effect.Source)
            );
            parent.effectCTRL.AddEffect(effect, parent);
            CustomLogger.Log(this, $"{parent.name} is son of god ,108 activated");
        }
    }
    private bool Condition()
    {
        if (parent != AriusManager.Instance.SonOfGod)
            return false;
        bool conditionA = false;
        CharacterParent characterParent = parent.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        List<CharacterCTRL> characters = characterParent.GetBattleFieldCharacter();
        characters.Remove(parent);
        conditionA = characters.Count <= 0;

        if (parent.GetHealthPercentage() >= 0.95f || conditionA)

            return true;

        return false;
    }

}
