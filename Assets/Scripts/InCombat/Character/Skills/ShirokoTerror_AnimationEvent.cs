using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShirokoTerror_AnimationEvent : MonoBehaviour
{
    public CharacterCTRL parent;
    public void Awake()
    {
        
    }
    public void Appear()
    {
        ResourcePool.Instance.enemy.ClearAllCharacter();
        ResourcePool.Instance.ally.ClearAllCharacter(gameObject);
        parent.IsDragable = false;
        Shiroko_Canva.Instance.parent.SetActive(false);
    }
    public void Release()
    {
        ResourcePool.Instance.ally.Shiroko_Terror_Postponed = false;
        parent.IsDragable = true;
        ResourcePool.Instance.ally.ContinueBattleRoutine();
        ResourcePool.Instance.ally.UpdateTraitEffects();
    }
}
