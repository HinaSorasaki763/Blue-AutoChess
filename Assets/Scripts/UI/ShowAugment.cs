using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowAugment : MonoBehaviour , IPointerClickHandler
{
    public GameObject Augments;
    void Start()
    {
        Augments.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Augments.gameObject.SetActive(!Augments.activeInHierarchy);
    }
}
