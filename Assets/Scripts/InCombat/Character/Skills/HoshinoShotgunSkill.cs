using GameEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class HoshinoShotgunSkill : MonoBehaviour
{
    public SkillContext skillContext;
    public Transform Shield;
    public void HoshinoSingleshot()
    {
        int dmg = (int)skillContext.Parent.GetStat(GameEnum.StatsType.Attack);
        (bool,int) tuple = skillContext.Parent.CalculateCrit(dmg);
        foreach (var item in skillContext.SelectedHex)
        {
            if (item.OccupyingCharacter != null)
            {
                item.OccupyingCharacter.GetHit(tuple.Item2, skillContext.Parent, "HoshinoSingleshot()", tuple.Item1);
                Effect effect = EffectFactory.CreateStunEffect(1, skillContext.Parent);
                item.OccupyingCharacter.effectCTRL.AddEffect(effect);
            }
        }
    }
    public void DiasableShield()
    {
        //Shield.gameObject.SetActive(false);
    }
    public void EnableShield()
    {
        Shield.gameObject.SetActive(true);
    }
}
