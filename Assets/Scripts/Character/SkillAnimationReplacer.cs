using System.Collections.Generic;
using UnityEngine;

public class SkillAnimationReplacer : MonoBehaviour
{
    public Animator animator;
    public RuntimeAnimatorController defaultController; // 預設控制器
    public RuntimeAnimatorController enhancedSkillController; // 帶有強化技能狀態機

    // 切換到強化技能整套動畫狀態機
    public void SwitchToEnhancedSkill()
    {
        animator.runtimeAnimatorController = enhancedSkillController;
    }
    public void Start()
    {
        
    }

}
