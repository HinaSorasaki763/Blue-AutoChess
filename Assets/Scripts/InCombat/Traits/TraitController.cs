using GameEnum;
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
        CustomLogger.LogError(this,"not found trait");
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
    public void CreateObserverForTrait(Traits trait)
    {
        int traitLevel = TraitsEffectManager.Instance.GetTraitLevelForCharacter(trait, isEnemy: false);
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
                // 尚未實作
                break;
            case Traits.Precision:
                // 尚未實作
                break;
            case Traits.Barrage:
                observer = new BarrageObserver(traitLevel, character);
                break;
            case Traits.Aegis:
                observer = new AegisObserver(traitLevel, character);
                break;
            case Traits.Healer:
                // 尚未實作
                break;
            case Traits.Disruptor:
                // 尚未實作
                break;
            case Traits.RapidFire:
                observer = new RapidfireObserver(traitLevel, character);
                break;
            case Traits.logistic:
                // 尚未實作
                break;
            case Traits.Mystic:
                // 尚未實作
                break;
            case Traits.Arius:
                observer = new AriusObserver(character);
                break;
            case Traits.SRT:
                break;
            case Traits.None:
            default:
                // 不做任何操作
                break;
        }
        if (observer != null)
        {
            if (!traitObservers.TryGetValue(trait,out var value))
            {
                traitObservers[trait] = observer;
            }

        }
    }
    public void OnDealtDmg(CharacterCTRL target,int dmg)
    {
        foreach (var item in traitObservers.Values)
        {
            item.OnDamageDealt(character,target,dmg);
        }
        foreach (var item in characterSpecificObservers)
        {
            item.OnDamageDealt(character, target, dmg);
        }
    }
    public int ModifyDamageTaken(int amount, CharacterCTRL sourceCharacter)
    {
        sourceCharacter.traitController.OnDealtDmg(character, amount);
        DamageStatisticsManager.Instance.UpdateDamage(sourceCharacter,amount);
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

    public void Win()
    {
        gameObject.GetComponent<CustomAnimatorController>().SetToIdle();
        foreach (var observer in traitObservers.Values)
        {
            observer.OnBattleEnd(true);
        }
    }

    public void NotifyOnKilledEnemy()
    {
        foreach (var observer in traitObservers.Values)
        {
            observer.OnKilledEnemy(character);
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
        if (character.enterBattle)
        {
            foreach (var observer in traitObservers.Values)
            {
                observer.CharacterUpdate();
            }
        }

    }
}
