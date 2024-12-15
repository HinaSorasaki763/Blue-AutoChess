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
                DroneAnimator.SetBool("Exit", true);
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
        var nodes = Utility.GetHexInRange(characterCTRL.CurrentHex, 5);
        var targets = nodes
            .Where(node => node.OccupyingCharacter != null && node.OccupyingCharacter.IsAlly == characterCTRL.IsAlly)
            .Select(node => node.OccupyingCharacter)
            .ToList();

        foreach (var target in targets)
        {
            target.Heal(100, characterCTRL);
        }
    }

    public void GetDrone(float time)
    {
        isCasting = true;
        if (LastedTime > 0)
        {
            LastedTime += time;
            DroneAnimator.SetBool("Idle", true);
        }
        else
        {
            LastedTime = time;
            DroneAnimator.SetBool("Appear", true);
        }
    }
}
