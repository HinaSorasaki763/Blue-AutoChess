using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrinityObserver : CharacterObserverBase
{
    
    public TrinityObserver(int level, CharacterCTRL character)
    {

    }
    public override void OnDamageDealt(CharacterCTRL source, CharacterCTRL target, int damage)
    {
        base.OnDamageDealt(source, target, damage);
        CustomLogger.Log(this, $"character {source} dealt {damage} to {target} at {Time.time}");
        TrinityManager.Instance.AddStack(target.transform.position);
    }
}
