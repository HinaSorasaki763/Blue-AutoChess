using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class AriusObserver : CharacterObserverBase
{
    private int traitLevel;
    public CharacterCTRL parent;
    public bool IsSonOfGod;
    public bool isactive = false;
    public AriusObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.parent = character;
    }
    public override void ActivateTrait()
    {
        Effect effect = EffectFactory.CreateAriusEffect();
        parent.effectCTRL.AddEffect(effect,parent);
        isactive = true;
        AriusManager.Instance.GetSelector();
    }
    public override void DeactivateTrait()
    {
        parent.effectCTRL.ClearEffectWithSource("AriusTraitEffect");
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
            {3, new TraitLevelStats(70,15,50)}
        };
        return statsByStarLevel;
    }
    public override void GetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit, string detailedSource, bool recursion = true)
    {
        CustomLogger.Log(this, $"GetHit invoked. Character: {character.name}, Source: {source.name}, Amount: {amount}, IsCrit: {isCrit}, Detail: {detailedSource}");

        if (isactive)
        {
            List<CharacterCTRL> c = AriusManager.Instance.GetOnBoard(false);
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
        if (isactive)
        {
            CharacterCTRL lowest = null;
            int low = int.MaxValue;
            foreach (var item in AriusManager.Instance.GetOnBoard(true))
            {
                if (item.GetStat(StatsType.currHealth) < low)
                {
                    low = (int)item.GetStat(StatsType.currHealth);
                    lowest = item;
                }
            }
            int healAmount = (int)(damage * GetTraitObserverLevel()[traitLevel].Data2 * 0.01f);
            lowest.Heal(healAmount, parent);
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
        AriusManager.Instance.RemoveArius(c);
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        base.OnEnterBattleField(character);
        AriusManager.Instance.AddArius(character);
    }
}
