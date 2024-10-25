using GameEnum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticObject : CharacterCTRL
{
    public override void OnEnable()
    {
        isObj = true;
        stats = characterStats.Stats.Clone();
        effectCTRL = GetComponent<EffectCTRL>();
        modifierCTRL = GetComponent<ModifierCTRL>();
        ResetStats();
    }
    public override void Update()
    {

    }
    public override void GetHit(int amount, CharacterCTRL sourceCharacter)
    {
        base.GetHit(amount, sourceCharacter);
    }
    public override IEnumerator Die()
    {
        Debug.Log($"{gameObject.name} Die()");
        SpawnGrid.Instance.RemoveCenterPoint(CurrentHex);
        CurrentHex.OccupyingCharacter = null;
        CurrentHex.HardRelease();
        gameObject.SetActive(false);
        return null;
    }
}
