using GameEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Atsuko_Drone : MonoBehaviour
{
    public GameObject DroneRef;
    public bool isCasting;
    public float LastedTime;
    public Animator DroneAnimator;
    public Transform parent;
    public CharacterCTRL characterCTRL;
    private float counter = 0f;
    public enum AnimatorType
    {
        Idle,
        Appear,
        Exit
        
    }
    public void Awake()
    {

    }
    public void Start()
    {
        SetAnimatorClip(AnimatorType.Idle);
        DroneRef.SetActive(false);
    }
    public void Update()
    {
        if (isCasting)
        {
            if (LastedTime > 0)
            {
                LastedTime -= Time.deltaTime;
                counter -= Time.deltaTime;
                if (counter < 0f)
                {
                    Trigger();
                    counter = 1;
                }
                SetAnimatorClip(AnimatorType.Idle);
            }
            else
            {
                LastedTime = 0;
                counter = 0;
                SetAnimatorClip(AnimatorType.Exit);
                isCasting = false;
            }
        }

    }
    public void SetAnimatorClip(AnimatorType animatorType)
    {
        DroneAnimator.SetBool("Idle", false);
        DroneAnimator.SetBool("Appear", false);
        DroneAnimator.SetBool("Exit", false);
        switch (animatorType)
        {
            case AnimatorType.Idle:
                DroneAnimator.SetBool("Idle", true);
                break;
            case AnimatorType.Appear:
                DroneAnimator.SetBool("Appear", true);
                break;
            case AnimatorType.Exit:
                DroneAnimator.SetBool("Exit", true);
                break;
            default:
                DroneAnimator.SetBool("Idle", false);
                CustomLogger.LogWhenThingShouldntHappened(this);
                break;
        }
    }
    public void Trigger()
    {
        CustomLogger.Log(this, "Trigger()");
        var characters = Utility.GetCharacterInrange(characterCTRL.CurrentHex,5, characterCTRL, true);
        int healAmount = characterCTRL.ActiveSkill.GetAttackCoefficient(characterCTRL.GetSkillContext());
        foreach (var target in characters)
        {
            CustomLogger.Log(this, "Heal Target: " + target.name);
            if (GameController.Instance.CheckCharacterEnhance(12, characterCTRL.IsAlly))
            {
                int amount = (int)(characterCTRL.GetStat(StatsType.DodgeChance) * 0.0025f);//25%
                Effect effect = EffectFactory.UnStatckableStatsEffct(1.5f, "AtsukoEnhancedSkill", amount, StatsType.DodgeChance, characterCTRL, false);
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.DodgeChance, effect.Value, effect.Source),
                    (character) => character.ModifyStats(StatsType.DodgeChance, -effect.Value, effect.Source)
                );
            }
            target.Heal(healAmount, characterCTRL);
        }
    }

    public void GetDrone(float time)
    {
        isCasting = true;
        if (LastedTime > 0)
        {
            DroneRef.SetActive(true);
            LastedTime += time;
            DroneAnimator.SetBool("Idle", true);
        }
        else
        {
            DroneRef.SetActive(true);
            LastedTime = time;
            DroneAnimator.SetBool("Appear", true);
        }
    }
}
