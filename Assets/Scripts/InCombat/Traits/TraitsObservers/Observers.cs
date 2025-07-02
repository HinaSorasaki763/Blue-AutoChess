using GameEnum;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Observers : MonoBehaviour
{
    public static Dictionary<Traits, CharacterObserverBase> traitObservers = new Dictionary<Traits, CharacterObserverBase>();
    public static Observers Instance { get; private set; }
    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {

        foreach (Traits t in Enum.GetValues(typeof(Traits)))
        {
            CreateObserverForTrait(t, 0);
        }
    }

    public void CreateObserverForTrait(Traits trait, int traitLevel)
    {
        CharacterObserverBase observer = null;

        switch (trait)
        {
            case Traits.Abydos:
                observer = new AbydosObserver(traitLevel, null);
                break;
            case Traits.Gehenna:
                observer = new GehennaObserver(traitLevel, null);
                break;
            case Traits.Hyakkiyako:
                observer = new HyakkiyakoObserver(traitLevel, null  );
                break;
            case Traits.Millennium:
                observer = new MillenniumObserver(traitLevel, null);
                break;
            case Traits.Trinity:
                observer = new TrinityObserver(traitLevel, null);
                break;
            case Traits.Supremacy:
                observer = new SupermacyObserver(traitLevel, null);
                break;
            case Traits.Precision:
                observer = new PrecisionObserver(traitLevel, null);
                break;
            case Traits.Barrage:
                observer = new BarrageObserver(traitLevel, null);
                break;
            case Traits.Aegis:
                observer = new AegisObserver(traitLevel, null);
                break;
            case Traits.Healer:
                observer = new HealerObserver(traitLevel, null);
                break;
            case Traits.Disruptor:
                observer = new DisruptorObserver(traitLevel, null);
                break;
            case Traits.RapidFire:
                observer = new RapidfireObserver(traitLevel, null);
                break;
            case Traits.logistic:
                observer = new LogisticObserver(traitLevel, null);
                break;
            case Traits.Mystic:
                observer = new MysticObserver(traitLevel, null);
                break;
            case Traits.Arius:
                observer = new AriusObserver(traitLevel, null);
                break;
            case Traits.SRT:
                observer = new SRTObserver(traitLevel, null);
                break;
            case Traits.None:
                observer = new NoneObserver(traitLevel, null);
                break;
            default:
                // 不做任何操作
                break;
        }
        if (observer != null)
        {
            traitObservers[trait] = observer;

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
}
