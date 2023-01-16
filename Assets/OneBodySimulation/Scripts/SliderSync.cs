using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderSync : MonoBehaviour
{
    [SerializeField] OneBodySlideController slideController;
    [SerializeField] Slider slider;

    public enum SliderValueName {
        MoonPeriodFactor,
        MoonSpinSpeed
    };
    [SerializeField] public SliderValueName sliderValueName;

    [Header("UI Paramaters")]
    [SerializeField] TextMeshProUGUI TMPgui;
    [SerializeField] Image fillImage;
    [SerializeField] Image syncImage;
    [SerializeField] TextMeshProUGUI syncLabel;

    [Header("Sync Parameters")]
    [SerializeField] Color syncColor;
    [SerializeField] float syncValue;
    [SerializeField] List<Color> defaultColors;

    public void Start() {
        if (slider)
            slider.onValueChanged.AddListener(delegate {SliderValueChange();});
    }

    public void SliderValueChange() {
        if (sliderValueName==SliderValueName.MoonPeriodFactor) {
            slideController.SetMoonPeriodFactor(slider2sim(slider.value));
            if (TMPgui) {
                float valueLabel = slideController.getMoonPeriod();
                if (valueLabel>2000f) {
                    valueLabel = Mathf.Infinity;
                }
                TMPgui.text = valueLabel.ToString("F1");
            }
        } 
        else {
            float spinSpeed=slider2sim(slider.value);
            slideController.SetMoonSpinSpeed(spinSpeed);
            if (TMPgui) {
                float valueLabel = slideController.getMoonPeriod()/slider.value;
                if (valueLabel>2000f) {
                    valueLabel = Mathf.Infinity;
                }
                TMPgui.text = valueLabel.ToString("F1");
            }
        }
    }

    public void updateValue(float valueLabel, float simValue) {
        if (TMPgui) {
            TMPgui.text = (valueLabel).ToString("F1");
        }
        if (slider) {
            float newValue=sim2slider(simValue);
            slider.value = newValue;
            if (newValue > syncValue-0.05 && newValue < syncValue+0.05) {
                fillImage.color=syncColor;
                syncImage.color=syncColor;
                syncLabel.color=syncColor;
            }
        }
        //Debug.Log("handle: " + handle.transform.position);
    }

    public void resetSlider() {
        if (fillImage) {
            fillImage.color=defaultColors[0];
            syncImage.color=defaultColors[1];
            syncLabel.color=defaultColors[1];
        }
    }

    private float slider2sim(float value) {
        if (sliderValueName==SliderValueName.MoonPeriodFactor) {
            return 1/(Mathf.Pow(2, value)-1);
        }
        else {
            return Mathf.Pow(value, 4.32193f)-1;
        }
    }

    private float sim2slider(float value) {
        if (sliderValueName==SliderValueName.MoonPeriodFactor) {
            return Mathf.Log((1/value)+1 , 2);
        }
        else {
            return Mathf.Pow(value+1, 1/4.32193f);
        }
    }
}
