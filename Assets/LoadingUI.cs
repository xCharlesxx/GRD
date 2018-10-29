using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private Slider slider;

    [SerializeField] private Text sliderText;


    // Use this for initialization
    void Start()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }


    public void SetSliderAndText(int _magicCount, bool downloading)
    {
        slider.maxValue = _magicCount;
        slider.value = 1;
        string text = "";
        if (downloading)
        {
            text = "DOWNLOADING FILES... ";
        }
        else
        {
            text = "LOADING FILES... ";
        }

        sliderText.text = text + slider.value + " OF " + slider.maxValue;
    }


    public void UpdateSlider(int _o, bool downloading)
    {
        slider.value = _o;
        string text = "";
        if (downloading)
        {
            text = "DOWNLOADING FILES... ";
        }
        else
        {
            text = "LOADING FILES... ";
        }

        sliderText.text = text + slider.value + " OF " + slider.maxValue;
    }
}