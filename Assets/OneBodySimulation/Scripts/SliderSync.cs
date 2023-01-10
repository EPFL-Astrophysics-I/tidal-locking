using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderSync : MonoBehaviour
{
    [Header("UI Paramaters")]
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI TMPgui;
    [SerializeField] Image fillImage;
    [SerializeField] Image syncImage;
    [SerializeField] TextMeshProUGUI syncLabel;

    [Header("Sync Parameters")]
    [SerializeField] Color syncColor;
    [SerializeField] float syncValue;
    [SerializeField] List<Color> defaultColors;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void updateValue(float valueLabel, float value) {
        if (TMPgui) {
            TMPgui.text = (valueLabel).ToString("F1");
        }
        if (slider) {
            slider.value = value;
            if (value > syncValue-0.1 && value < syncValue+0.1) {
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
}
