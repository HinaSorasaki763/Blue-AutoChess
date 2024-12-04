using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentEventHandler : MonoBehaviour
{
    private List<CharacterObserverBase> observers = new ();
    public static AugmentEventHandler Instance = new AugmentEventHandler ();
    public void Awake()
    {
        Instance = this;
    }
    public void Start()
    {
        CustomLogger.Log(this, "");
    }
    public void Attacking(CharacterCTRL character)
    {
        foreach (var item in observers)
        {
            item.OnAttacking(character);
        }
    }
    public void AddObserver(CharacterObserverBase observer)
    {
        observers.Add(observer);
    }
    public void RemoveObserver(CharacterObserverBase observer) => observers.Remove(observer);
    

}
