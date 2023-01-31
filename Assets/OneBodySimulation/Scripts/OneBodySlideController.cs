using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneBodySlideController : SimulationSlideController
{
    [Header("Main Simulation Parameters")]
    [SerializeField] private bool simIsStationary;
    [SerializeField] private OneBodySimulation.OneBodySimType simulationType;
    [SerializeField] private float radiusScale;

    [Header("Earth Parameters")]
    [SerializeField] private bool displayEarthOrbit;

    [Header("Moon Parameters")]
    [SerializeField] private float moonPeriodFactor;
    [SerializeField] private bool moonIsSquashed;
    [SerializeField] private float moonSpinSpeed=0; // Default Speed is 0
    [SerializeField] private bool displayMoonOrbit;
    [SerializeField] private bool displayMoonBulgeLine;
    [SerializeField] private bool displayMoonRefSystem;
    [SerializeField] private bool displayTidalVector;
    [SerializeField] private float tidalVectorSize;
    [SerializeField] private float tidalVectorLineWidth; // if 0 then vectorGravLineWidth is not used and value is taken from prefab

    [Header("Interactivity Parameters")]

    [SerializeField] private OneBodySimulation.DragBodyName draggableBody;
    [SerializeField] private bool draggableMoonEdges;
    [SerializeField] private float draggableEdgesAngleRange;

    [Header("Display Parameters")]
    [SerializeField] private TopDownView TopDownView;
    [SerializeField] private SliderSync sliderSync;
    [SerializeField] private Button resetSliderButton;

    [Header("Initial Condition")]
    [SerializeField] private bool useMoonCI;
    [SerializeField] private float angleMoonOrbitInit;
    [SerializeField] private float angleMoonSpinInit;

    [Header("FadeIn/Out UI")]

    [SerializeField] private List<FadeOutUI> fadeOutUIList;


    private OneBodySimulation sim;

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        sim = simulation as OneBodySimulation;

        // Main Simulation Parameters:
        sim.simIsStationary = simIsStationary;
        sim.simulationType = simulationType;
        if (simulationType == OneBodySimulation.OneBodySimType.MoonSquashing)
        {
            sim.squashingAnimation = true;
        }
        sim.radiusScale = radiusScale;

        // Earth Parameters:
        sim.ActivationEarthOrbit = displayEarthOrbit;

        // Moon Parameters:
        sim.MoonIsSquashed = moonIsSquashed;
        sim.MoonPeriodFactor = moonPeriodFactor;
        sim.MoonSpinSpeed = moonSpinSpeed;

        sim.ActivationMoonOrbit = displayMoonOrbit;
        sim.ActivationMoonBulgeLine = displayMoonBulgeLine;
        sim.ActivationMoonRefSystem = displayMoonRefSystem;

        sim.ActivationPointsOnMoon = displayTidalVector;
        sim.VectorTidalScale = tidalVectorSize;
        sim.VectorTidalLineWidth = tidalVectorLineWidth;

        // Interactivity Parameters:
        sim.dragBodyName = draggableBody;
        sim.dragMoonEdgesIsAllowed = draggableMoonEdges;
        sim.draggableEdgesAngleRange = draggableEdgesAngleRange;

        // Display Parameters:
        sim.topDownView = TopDownView;
        sim.sliderSync = sliderSync;

        // Initial Condition:
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
        sim.UseMoonCI = useMoonCI;

        fadeOutUIList.ForEach(ui => {
            //ui.TriggerReset(0);
            //ui.Reset();
        });

        fadeOutUIList.ForEach(ui => {
            ui.TriggerFadeOut();
        });
    }

    private void OnDisable() {
        StopAllCoroutines();
        if (resetSliderButton) {
            resetSliderButton.interactable=true;
            resetSliderButton.onClick.Invoke();
        }
    }

    public void SetMoonPeriodFactor(float newfactor) {
        // Function useful for the button in the slide 2
        // So when transitioning slide2 to slide 1 and after slide1 to slide2, we keep
        // the moon period according the button configuration and not the initialization by the Slide Controller.


        // if MoonPeriodFactor = 0.5 => Moon rotates twice faster
        // if MoonPeriodFactor = 2 => Moon rotates twice slower

        //float value = 1/(Mathf.Log10(newfactor)*(1/Mathf.Log10(2)));

        //float value = 1/newfactor;

        //float value = 1/(Mathf.Pow(2, newfactor)-1);

        sim.MoonPeriodFactor = newfactor;
        moonPeriodFactor = newfactor;
    }

    public float getMoonPeriod() {
        return sim.getMoonPeriod();
    }

    public void SetMoonSquashed(bool newBool) {
        // Keep state of the interaction
        moonIsSquashed = newBool;
    }

    public void SetStationaryFlag(bool newBool) {
        // Keep state of the interaction
        simIsStationary = newBool;
        sim.simIsStationary = newBool;
    }

    public void SetActivationMoonTidalVectors(bool newBool) {
        // Keep state of the interaction
        displayTidalVector = newBool;
        sim.ActivationPointsOnMoon = newBool;
    }

    public void SetActivationMoonRefSystem(bool newBool) {
        // Keep state of the interaction
        displayMoonRefSystem = newBool;
        sim.ActivationMoonRefSystem = newBool;
    }

    public void SetMoonSpinSpeed(float value) {
        moonSpinSpeed=value;
        if (sim)
            sim.MoonSpinSpeed=value;
    }
}
