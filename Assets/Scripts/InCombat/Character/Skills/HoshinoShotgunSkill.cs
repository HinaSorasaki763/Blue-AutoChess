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
        int dmg = skillContext.Parent.ActiveSkill.GetAttackCoefficient(skillContext);
        (bool,int) tuple = skillContext.Parent.CalculateCrit(dmg);
        foreach (var item in skillContext.SelectedHex)
        {
            if (item.OccupyingCharacter != null)
            {
                item.OccupyingCharacter.GetHit(tuple.Item2, skillContext.Parent, "HoshinoSingleshot()", tuple.Item1);

                if (GameController.Instance.CheckCharacterEnhance(26,true))
                {
                    float duration = skillContext.Parent.ActiveSkill.GetCharacterLevel()[skillContext.CharacterLevel].Data5;
                    Effect stunEffect = EffectFactory.CreateStunEffect(duration, item.OccupyingCharacter);
                    item.OccupyingCharacter.effectCTRL.AddEffect(stunEffect,item.OccupyingCharacter);
                }
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
