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

    [Header("Interactivity Parameters")]
    [SerializeField] private string dragBody;
    [SerializeField] private bool dragRotatesMoon;
    [SerializeField] private bool dragMoonEdgesIsAllowed;
    [SerializeField] private float dragMoonEdgesRanges;

    [Header("Display Parameters")]
    [SerializeField] private bool displayVectorsFromMoonPoints;
    [SerializeField] private float tidalVectorSize;
    [SerializeField] private float vectorTidalLineWidth; // if 0 then vectorGravLineWidth is not used and value is taken from prefab
    [SerializeField] private bool displayMoonOrbit;
    [SerializeField] private bool displayMoonBulgeLine;
    [SerializeField] private bool displayMoonRefSystem;
    [SerializeField] private bool displayMoonMouseVector;

    [Header("   Top Down View")]
    [SerializeField] private TopDownView TopDownView;
    [SerializeField] private BarOnPlot spinSpeedBar;
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


        sim.MoonIsSquashed = moonIsSquashed;
        sim.radiusScale = radiusScale;
        sim.MoonPeriodFactor = moonPeriodFactor;
        sim.MoonSpinSpeed = moonSpinSpeed;

        if (dragBody=="moon") {
            sim.dragMoonIsAllowed = true;
            sim.dragEarthIsAllowed = false;
        } 
        else if (dragBody=="earth") {
            sim.dragMoonIsAllowed = false;
            sim.dragEarthIsAllowed = true;
        }
        else {
            sim.dragMoonIsAllowed = false;
            sim.dragEarthIsAllowed = false;
        }


        sim.dragRotatesMoon = dragRotatesMoon;
        sim.dragMoonEdgesIsAllowed = dragMoonEdgesIsAllowed;

        sim.VectorTidalScale = tidalVectorSize;
        sim.VectorTidalLineWidth = vectorTidalLineWidth;

        sim.ActivationPointsOnMoon = displayVectorsFromMoonPoints;
        sim.ActivationMoonOrbit = displayMoonOrbit;
        sim.ActivationEarthOrbit = displayEarthOrbit;
        sim.ActivationMoonBulgeLine = displayMoonBulgeLine;
        sim.ActivationMoonRefSystem = displayMoonRefSystem;

        // CI:
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
        sim.UseMoonCI = useMoonCI;

        sim.topDownView = TopDownView;
        sim.spinSpeedBar = spinSpeedBar;
        sim.sliderSync = sliderSync;

        sim.DragEdgesRange = dragMoonEdgesRanges;

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
        displayVectorsFromMoonPoints = newBool;
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
        if (spinSpeedBar) {
            spinSpeedBar.SetPosition(value);
        }
    }
}
