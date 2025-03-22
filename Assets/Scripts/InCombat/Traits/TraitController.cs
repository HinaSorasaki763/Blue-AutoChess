using GameEnum;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TraitController : MonoBehaviour
{
    private List<Traits> currentTraits = new List<Traits>();
    private Dictionary<Traits, CharacterObserverBase> traitObservers = new Dictionary<Traits, CharacterObserverBase>();
    private List<CharacterObserverBase> characterSpecificObservers = new List<CharacterObserverBase>();
    private CharacterCTRL character;

    void Awake()
    {
        character = GetComponent<CharacterCTRL>();
        if (character == null)
        {
            Debug.LogError("TraitController requires CharacterCTRL component.");
        }
    }
    public void TriggerOnEnterBattleField()
    {
        foreach (var item in traitObservers.Values)
        {
            item.OnEnterBattleField(character);
        }
        foreach (var item in characterSpecificObservers)
        {
            item.OnEnterBattleField(character);
        }
    }
    public void TriggerOnLeaveBattleField()
    {
        foreach (var item in traitObservers.Values)
        {
            item.OnLeaveBattleField(character);
        }
        foreach (var item in characterSpecificObservers)
        {
            item.OnLeaveBattleField(character);
        }
    }
    public void AddTrait(Traits trait)
    {
        if (!currentTraits.Contains(trait))
        {
            currentTraits.Add(trait);
        }
    }
    public CharacterObserverBase GetObserverForTrait(Traits trait)
    {
        if (traitObservers.TryGetValue(trait, out var observer))
        {
            return observer;
        }
        CustomLogger.LogError(this, "not found trait");
        return null;
    }

    public void RemoveTrait(Traits trait)
    {
        if (currentTraits.Contains(trait))
        {
            currentTraits.Remove(trait);
            RemoveObserverForTrait(trait);
        }
    }

    public List<Traits> GetCurrentTraits()
    {
        return new List<Traits>(currentTraits);
    }

    public bool HasTrait(Traits trait)
    {
        return currentTraits.Contains(trait);
    }

    public Traits GetAcademy()
    {
        return Utility.IsAcademy(currentTraits);
    }
    public void CreateObserverForTrait(Traits trait,int traitLevel)
    {
        CharacterObserverBase observer = null;

        switch (trait)
        {
            case Traits.Abydos:
                observer = new AbydosObserver(traitLevel, character);
                break;
            case Traits.Gehenna:
                observer = new GehennaObserver(traitLevel, character);
                break;
            case Traits.Hyakkiyako:
                observer = new HyakkiyakoObserver(traitLevel, character);
                break;
            case Traits.Millennium:
                observer = new MillenniumObserver(traitLevel, character);
                break;
            case Traits.Trinity:
                observer = new TrinityObserver(traitLevel, character);
                break;
            case Traits.Supremacy:
                observer = new SupermacyObserver(traitLevel, character);
                break;
            case Traits.Precision:
                observer = new PrecisionObserver(traitLevel, character);
                break;
            case Traits.Barrage:
                observer = new BarrageObserver(traitLevel, character);
                break;
            case Traits.Aegis:
                observer = new AegisObserver(traitLevel, character);
                break;
            case Traits.Healer:
                observer = new HealerObserver(traitLevel, character);
                break;
            case Traits.Disruptor:
                observer = new DisruptorObserver(traitLevel,character);
                break;
            case Traits.RapidFire:
                observer = new RapidfireObserver(traitLevel, character);
                break;
            case Traits.logistic:
                observer = new LogisticObserver(traitLevel, character);
                break;
            case Traits.Mystic:
                observer = new MysticObserver(traitLevel, character);
                break;
            case Traits.Arius:
                observer = new AriusObserver(traitLevel,character);
                break;
            case Traits.SRT:
                observer = new SRTObserver(traitLevel,character);
                break;
            case Traits.None:
            default:
                // 不做任何操作
                break;
        }
        if (observer != null)
        {
            CustomLogger.Log(this, $"observer{observer} = level {traitLevel}");
            traitObservers[trait] = observer;

        }
    }
    public void OnDealtDmg(CharacterCTRL target, int dmg,string detailedSource, bool iscrit)
    {
        Lifesteal(dmg);
        foreach (var item in traitObservers.Values)
        {
            item.OnDamageDealt(character, target, dmg, detailedSource, iscrit);
        }
        foreach (var item in characterSpecificObservers)
        {
            item.OnDamageDealt(character, target, dmg, detailedSource, iscrit);
        }
        character.equipmentManager.OnParentDealtDamage(character, target, dmg, detailedSource,iscrit);
    }
    public void Dodged()
    {
        foreach (var item in traitObservers.Values)
        {
            item.OnDodged(character);
        }
    }
    private void Lifesteal(int dmg)
    {
        float lifesteal = character.GetStat(StatsType.Lifesteal);
        character.Heal((int)(dmg * lifesteal / 100),character);
    }
    public int ModifyDamageTaken(int amount, CharacterCTRL sourceCharacter,string detailedSource,bool iscrit)
    {
        sourceCharacter.traitController.OnDealtDmg(character, amount, detailedSource,iscrit);
        DamageStatisticsManager.Instance.UpdateDamageTaken(character, amount);
        DamageStatisticsManager.Instance.UpdateDamage(sourceCharacter, amount);
        foreach (var observer in traitObservers.Values)
        {
            amount = observer.OnDamageTaken(character, sourceCharacter, amount);

        }
        foreach (var observer in characterSpecificObservers)
        {
            amount = observer.OnDamageTaken(character, sourceCharacter, amount);
        }
        return amount;
    }

    public void OnBattleEnd(bool win)
    {

        if (gameObject.GetComponent<CustomAnimatorController>() != null)
        {
            gameObject.GetComponent<CustomAnimatorController>().SetToIdle();
        }

        foreach (var observer in traitObservers.Values)
        {
            observer.OnBattleEnd(win);
        }
    }
    public void NotifyGetHit(CharacterCTRL character, CharacterCTRL source, float amount, bool isCrit,string detailedSource)
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.GetHit(character, source, amount, isCrit,detailedSource);
        }

    }
    public (float, float) BeforeApplyingNegetiveEffect(float length, float effectiveness)
    {
        float finallength = length;
        float finaleffectiveness = effectiveness;
        foreach (var item in traitObservers.Values)
        {
            (finallength, finaleffectiveness) = item.AdjustNegetiveEffect(finallength, finaleffectiveness);
        }
        return (finallength, finaleffectiveness);
    }

    public void CharacterUpdate()
    {
        foreach (var item in traitObservers.Values)
        {
            item.CharacterUpdate();
        }
    }
    public int ObserverDamageModifier(CharacterCTRL sourceCharacter, CharacterCTRL target, int amount, string detailedSource, bool isCrit)
    {
        foreach (var item in traitObservers.Values)
        {
            amount = item.DamageModifier(sourceCharacter, target, amount, detailedSource, isCrit);
        }
        return amount;
    }
    public int BeforeDealtDamage(CharacterCTRL sourceCharacter,CharacterCTRL target,  int amount, string detailedSource, bool isCrit)
    {
        int reduced = 0;
        foreach (var item in traitObservers.Values)
        {
            reduced += item.BeforeDealtDmg(sourceCharacter, target, amount, detailedSource,isCrit);
        }
        return reduced;
    }
    public int BeforeHealing(CharacterCTRL source ,int amount)
    {
        int final = amount;
        foreach (var item in traitObservers.Values)
        {
            final = item.BeforeHealing(source, amount);
        }
        return final;
    }
    public void NotifyOnKilledEnemy(string detailedSource,CharacterCTRL characterDies)
    {
        character.OnKillEnemy(detailedSource, characterDies);
        foreach (var observer in traitObservers.Values)
        {
            observer.OnKilledEnemy(character, detailedSource, characterDies);
        }
        character.equipmentManager.OnParentKilledEnemy(detailedSource, characterDies);
    }
    public void TriggerCharacterStart()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.CharacterStart(character);
        }
    }
    public void TriggerManualUpdate()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.ManualUpdate(character);
        }
    }
    public void CastedSkill()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.OnCastedSkill(character);
        }
        
    }
    private void RemoveObserverForTrait(Traits trait)
    {
        if (traitObservers.ContainsKey(trait))
        {
            traitObservers.Remove(trait);
        }
    }
    public void Attacking()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.OnAttacking(character);
        }
    }
    public void NotifyOnDying()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.OnDying(character);
        }
    }

    public void NotifyOnCrit()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.OnCrit(character);
        }
    }

    void Update()
    {

    }
}
