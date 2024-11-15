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

    public void AddTrait(Traits trait)
    {
        if (!currentTraits.Contains(trait))
        {
            currentTraits.Add(trait);
        }
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

    public void CreateObserverForTrait(Traits trait)
    {
        int traitLevel = TraitsEffectManager.Instance.GetTraitLevelForCharacter(trait, isEnemy: false);
        CharacterObserverBase observer = null;

        switch (trait)
        {
            case Traits.Millennium:
                observer = new MillenniumObserver(traitLevel, character);
                break;
            case Traits.Abydos:
                observer = new AbydosObserver(traitLevel, character);
                break;
            case Traits.Barrage:
                observer = new BarrageObserver(traitLevel,character);

                break;
            case Traits.Gehenna:
                observer = new GehennaObserver(traitLevel, character);
                break;
            case Traits.Hyakkiyako:
                observer = new HyakkiyakoObserver(traitLevel, character);
                break;
            case Traits.Aegis:
                observer = new AegisObserver(traitLevel, character);
                break;
            case Traits.RapidFire:
                observer = new RapidfireObserver(traitLevel, character);
                CustomLogger.Log(this, $"observer {observer} RapidfireObserver observer on {character.name}");
                break;
            default:
                break;
        }
        if (observer != null)
        {
            traitObservers[trait] = observer;
        }
    }
    public int ModifyDamageTaken(int amount, CharacterCTRL sourceCharacter)
    {
        int modifiedAmount = amount;
        foreach (var observer in traitObservers.Values)
        {
            modifiedAmount = observer.OnDamageTaken(character, sourceCharacter, modifiedAmount);
        }
        foreach (var observer in characterSpecificObservers)
        {
            modifiedAmount = observer.OnDamageTaken(character, sourceCharacter, modifiedAmount);
        }
        return modifiedAmount;
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
