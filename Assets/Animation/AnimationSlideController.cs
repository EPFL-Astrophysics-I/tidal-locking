using UnityEngine;
using UnityEngine.UI;

public class AnimationSlideController : SimulationSlideController
{
    public Slider moonPeriodSlider;  // Rotation period
    public RectTransform tidalLockingLabel;
    public Button playButton;
    public Button resetButton;
    public RectTransform sliderCoverPanel;

    private TidalLockingAnimation sim;

    private float moonDefaultPeriod;  // Rotation period when synchronized

    public override void InitializeSlide()
    {
        Debug.Log("AnimationSlideController > InitializeSlide");

        // Get reference to the specific simulation
        sim = simulation as TidalLockingAnimation;
        if (sim == null)
        {
            Debug.Log("No simulation assigned in AnimationSlideController");
            return;
        }

        // Return the earth and moon to their starting positions
        sim.Reset();

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
    }

    public void StartAnimation()
    {
        if (sim) sim.StartAnimation();
    }

    public void Reset()
    {
        if (sim) sim.Reset();
        if (moonPeriodSlider) moonPeriodSlider.value = moonDefaultPeriod;
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

    public void SetMoonRotationPeriod(float value)
    {
        if (sim) sim.SetMoonRotationPeriod(value);
    }
}
