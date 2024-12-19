using GameEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AriusObserver : CharacterObserverBase
{
    public CharacterCTRL parent;
    public bool IsGodOfSon;
    public AriusObserver(CharacterCTRL parent)
    {
        this.parent = parent;
    }
    public void SetGodOfSon(bool b)
    {
        CustomLogger.Log(this,$"set {parent} to {b}");
        IsGodOfSon = b;
        var observer = parent.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;
        CustomLogger.Log(this, $" {parent} god of son : {IsGodOfSon} , get from traitctrl {observer.IsGodOfSon}");
        if (b)
        {
            parent.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
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
        AriusManager.Instance.ReChoose();
    }
    public override void OnEnterBattleField(CharacterCTRL character)
    {
        base.OnEnterBattleField(character);
        AriusManager.Instance.AddArius(character);
    }
}
