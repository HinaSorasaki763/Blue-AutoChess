using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class ShirokoActiveSkill : MonoBehaviour
{
    public GameObject droneRef;
    public Shiroko_Terror_DroneCTRL droneCTRL;
    public GameObject Drone;
    public void Start()
    {
        
    }
    public GameObject GetDrone(SkillContext skillContext)
    {
        return Instantiate(Drone, skillContext.Parent.transform.position, Quaternion.identity);
    }
}