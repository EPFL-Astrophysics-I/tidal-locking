using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoonPeriodSlider : MonoBehaviour
{
    public TextMeshProUGUI valueTMP;

    private Slider slider;

    public void SetValueText(float value)
    {
        if (valueTMP) valueTMP.text = value.ToString("0.0");
    }

    public void SnapToNearestTenth(float value)
    {
        if (!slider) TryGetComponent(out slider);

        if (slider) slider.value = Mathf.Round(10f * value) / 10f;
    }
}
