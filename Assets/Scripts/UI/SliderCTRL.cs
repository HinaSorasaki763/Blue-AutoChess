using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderCTRL : MonoBehaviour
{
    public Slider sliderComponent;
    public bool isInteractable = false;

    private void Start()
    {
        InitializeSlider();
    }
    public void InitializeSlider()
    {
        sliderComponent.interactable = isInteractable;
    }
    public void SetMaxValue(float maxValue)
    {
        sliderComponent.maxValue = maxValue;
    }
    public void SetMinValue(float minValue)
    {
        sliderComponent.minValue = minValue;
    }
    public void UpdateValue(float newValue)
    {
        sliderComponent.value = newValue;
    }
}
