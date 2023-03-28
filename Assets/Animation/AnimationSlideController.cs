using UnityEngine;
using UnityEngine.UI;

public class AnimationSlideController : SimulationSlideController
{
    [Header("UI")]
    public Slider moonPeriodSlider;  // Rotation period
    public Toggle moonRotationToggle;  // Rotation rate
    public RectTransform tidalLockingLabel;
    public Button playButton;
    public Button resetButton;
    public RectTransform sliderCoverPanel;

    [Header("Options")]
    public bool useDiscreteSteps;
    public int numSteps = 8;
    public float maxStepAngle = 30;
    public float timeScale = 1;

    private TidalLockingAnimation sim;

    private float moonDefaultPeriod;  // Rotation period when synchronized

    private void OnEnable()
    {
        TidalLockingAnimation.OnUpdateMoonRotationPeriod += HandleMoonRotationPeriodChanged;
        TidalLockingAnimation.OnDiscreteTidalLocking += HandleDiscreteTidalLocking;
    }

    private void OnDisable()
    {
        TidalLockingAnimation.OnUpdateMoonRotationPeriod -= HandleMoonRotationPeriodChanged;
        TidalLockingAnimation.OnDiscreteTidalLocking -= HandleDiscreteTidalLocking;
    }

    public override void InitializeSlide()
    {
        // Debug.Log("AnimationSlideController > InitializeSlide");

        // Get reference to the specific simulation
        sim = simulation as TidalLockingAnimation;
        if (sim == null)
        {
            Debug.Log("No simulation assigned in AnimationSlideController");
            return;
        }

        // Store the moon's orbital period to initialize the rotation period slider
        moonDefaultPeriod = sim.OrbitalPeriod;

        // Reset UI components
        if (moonPeriodSlider)
        {
            moonPeriodSlider.interactable = true;
            moonPeriodSlider.value = moonDefaultPeriod;
            if (moonPeriodSlider.TryGetComponent(out MouseOverEvent mouseOver))
            {
                mouseOver.EnablePointerHandler = true;
            }
        }
        if (playButton)
        {
            playButton.interactable = true;
            if (playButton.TryGetComponent(out MouseOverEvent mouseOver))
            {
                mouseOver.EnablePointerHandler = true;
            }
        }
        if (resetButton)
        {
            resetButton.interactable = false;
            if (resetButton.TryGetComponent(out MouseOverEvent mouseOver))
            {
                mouseOver.EnablePointerHandler = false;
            }
        }
        if (sliderCoverPanel) sliderCoverPanel.gameObject.SetActive(false);

        Reset();
    }

    public void HandleMoonRotationPeriodChanged(float newRotationPeriod)
    {
        if (moonPeriodSlider) moonPeriodSlider.value = newRotationPeriod;
    }

    public void StartAnimation()
    {
        if (sim)
        {
            float sign = Mathf.Sign(sim.MoonRotationPeriod - sim.OrbitalPeriod);
            sim.StartAnimation(sign, useDiscreteSteps, maxStepAngle);
        }
    }

    public void Reset()
    {
        // Return the earth and moon to their starting positions
        if (sim)
        {
            sim.Reset();
            sim.SetTimeScale(timeScale);
            sim.useDiscreteSteps = useDiscreteSteps;
            sim.numSteps = numSteps;
        }

        if (useDiscreteSteps && moonRotationToggle)
        {
            SetMoonRotationPeriod(moonRotationToggle.isOn);
        }

        if (moonPeriodSlider) moonPeriodSlider.value = moonDefaultPeriod;

        if (useDiscreteSteps && tidalLockingLabel)
        {
            tidalLockingLabel.gameObject.SetActive(false);
        }
    }

    public void CheckForTidalLocking()
    {
        if (moonPeriodSlider && tidalLockingLabel)
        {
            // The slider's value is rounded to the nearest 0.1
            bool isTidallyLocked = Mathf.Abs(moonPeriodSlider.value - moonDefaultPeriod) < 0.1f;
            tidalLockingLabel.gameObject.SetActive(isTidallyLocked);
        }
    }

    public void HandleDiscreteTidalLocking()
    {
        if (tidalLockingLabel) tidalLockingLabel.gameObject.SetActive(true);
    }

    public void SetMoonRotationPeriod(float value)
    {
        if (sim) sim.SetMoonRotationPeriod(value);
    }

    public void SetMoonRotationPeriod(bool rateIsFaster)
    {
        if (sim)
        {
            float rotationPeriod = rateIsFaster ? 15 : 150;
            sim.SetMoonRotationPeriod(rotationPeriod);
        }
    }
}
