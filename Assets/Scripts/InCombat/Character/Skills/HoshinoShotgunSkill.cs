using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class HoshinoShotgunSkill : MonoBehaviour
{
    public SkillContext skillContext;
    public Transform Shield;
    public void HoshinoSingleshot()
    {
        foreach (var item in skillContext.SelectedHex)
        {
            if (item.OccupyingCharacter != null)
            {
                item.OccupyingCharacter.GetHit(10, skillContext.Parent);
            }
        }
    }
    public void DiasableShield()
    {
        Shield.gameObject.SetActive(false);
    }
    public void EnableShield()
    {
        Shield.gameObject.SetActive(true);
    }
}
