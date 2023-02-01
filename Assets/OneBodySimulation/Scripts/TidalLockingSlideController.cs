using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TidalLockingSlideController : SimulationSlideController
{
    [Header("Main Simulation Parameters")]
    [SerializeField] private bool simIsStationary;
    [SerializeField] private TidalLockingSimulation.SimulationType simulationType;
    [SerializeField] private float bodyRadiusScale;

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
    [SerializeField] private TidalLockingSimulation.DragBodyName draggableBody;
    [SerializeField] private bool draggableMoonEdges;
    [SerializeField] private float draggableEdgesAngleRange;

    [Header("Display Parameters")]
    [SerializeField] private bool displayMoonEarthLine;
    [SerializeField] private TopDownView TopDownView;
    [SerializeField] private SliderSync sliderSync;
    [SerializeField] private Button resetSliderButton;

    [Header("Initial Condition")]
    [SerializeField] private bool resetEarthPos;
    [SerializeField] private bool useMoonCI;
    [SerializeField] private float angleMoonOrbitInit;
    [SerializeField] private float angleMoonSpinInit;

    [Header("FadeIn/Out UI")]
    [SerializeField] private List<FadeOutUI> fadeOutUIList;


    private TidalLockingSimulation sim;

    public override void InitializeSlide()
    {
        sim = simulation as TidalLockingSimulation;

        // Main Simulation Parameters:
        sim.simIsStationary = simIsStationary;
        sim.simulationType = simulationType;
        if (simulationType == TidalLockingSimulation.SimulationType.MoonSquashing)
        {
            sim.squashingAnimation = true;
        }
        sim.BodyRadiusScale = bodyRadiusScale;

        // Earth Parameters:
        sim.ActivationEarthOrbit = displayEarthOrbit;

        // Moon Parameters:
        sim.MoonIsSquashed = moonIsSquashed;
        sim.MoonPeriodFactor = moonPeriodFactor;
        sim.MoonSpinSpeed = moonSpinSpeed;

        sim.ActivationMoonOrbit = displayMoonOrbit;
        sim.ActivationMoonBulgeLine = displayMoonBulgeLine;
        sim.ActivationMoonRefSystem = displayMoonRefSystem;

        sim.ActivationTidalVectors = displayTidalVector;
        sim.VectorTidalScale = tidalVectorSize;
        sim.VectorTidalLineWidth = tidalVectorLineWidth;

        // Interactivity Parameters:
        sim.dragBodyName = draggableBody;
        sim.dragMoonEdgesIsAllowed = draggableMoonEdges;
        sim.draggableEdgesAngleRange = draggableEdgesAngleRange;
        // Allow or not the ability to change the cursor depending on the interactivity parameters:
        sim.SetBodyMouseCursor();

        // Display Parameters:
        sim.ActivationMoonEarthLine = displayMoonEarthLine;
        sim.topDownView = TopDownView;
        sim.sliderSync = sliderSync;

        // Initial Condition:
        sim.ResetEarthPos = resetEarthPos;
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
        sim.UseMoonCI = useMoonCI;

        fadeOutUIList.ForEach(ui => {
            ui.TriggerReset(0);
            //ui.Reset();
        });
    }

    public void FadeOutUI() {
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

        sim.MoonPeriodFactor = newfactor;
        moonPeriodFactor = newfactor;
    }

    public float getMoonPeriod() {
        if (sim) {
            return sim.getMoonPeriod();
        } else {
            return 27.5f;
        }
    }

    public void SetMoonSquashed(bool newBool) {
        // Keep state of the interaction
        moonIsSquashed = newBool;
    }

    public void SetStationaryFlag(bool newBool) {
        // Keep state of the interaction
        simIsStationary = newBool;
        if (sim) {
            sim.simIsStationary = newBool;
        }
    }

    public void SetActivationMoonTidalVectors(bool newBool) {
        // Keep state of the interaction
        displayTidalVector = newBool;
        if (sim) {
            sim.ActivationTidalVectors = newBool;
        }
    }

    public void SetActivationMoonRefSystem(bool newBool) {
        // Keep state of the interaction
        displayMoonRefSystem = newBool;
        if (sim) {
            sim.ActivationMoonRefSystem = newBool;
        }
    }

    public void SetMoonSpinSpeed(float value) {
        moonSpinSpeed=value;
        if (sim) {
            sim.MoonSpinSpeed=value;
        }
    }
}
