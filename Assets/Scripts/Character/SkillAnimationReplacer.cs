using System.Collections.Generic;
using UnityEngine;

public class SkillAnimationReplacer : MonoBehaviour
{
    public Animator animator;
    public RuntimeAnimatorController defaultController; // �w�]���
    public RuntimeAnimatorController enhancedSkillController; // �a���j�Ƨޯબ�A��

    // ������j�Ƨޯ��M�ʵe���A��
    public void SwitchToEnhancedSkill()
    {
        animator.runtimeAnimatorController = enhancedSkillController;
    }
    public void Start()
    {
        
    }

}
