using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ShirokoTerror_animCTRL : MonoBehaviour
{
    private Animator animator;
    private CharacterCTRL character;
    public GameObject body;
    void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<CharacterCTRL>();
    }
    public void SetBodyFalse()
    {
        body.SetActive(false);
    }
    public void SetBodyActive()
    {
        body.SetActive(true);
    }
    public void Shiroko_Terror_SkillCasted()
    {
        Debug.Log($" [ShirokoTerror_animCTRL] Shiroko_Terror_SkillCasted()");
        animator.SetBool("CastSkill", false);
    }
}
