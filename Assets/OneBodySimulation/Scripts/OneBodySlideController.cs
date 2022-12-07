using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OneBodySlideController : SimulationSlideController
{
    [Header("Sim Parameters")]
    [SerializeField] private bool simIsStationary;
    [SerializeField] private bool AnimationInThreeSteps;
    [SerializeField] private bool oscillationMoonRotation;
    [SerializeField] private bool MoonSquashingAnimation;
    [Header("Earth Moon Parameters")]
    [SerializeField] private float radiusScale;
    [SerializeField] private float moonPeriodFactor;
    [SerializeField] private bool moonIsSquashed;
    [Header("Interactivity Parameters")]
    [SerializeField] private bool dragMoonIsAllowed;
    [SerializeField] private bool dragMoonEdgesIsAllowed;

    [Header("Display Parameters")]
    [SerializeField] private bool displayVectorsFromMoonCM;
    [SerializeField] private bool displayVectorsFromMoonLR;
    [SerializeField] private float gravitationalVectorSize;
    [SerializeField] private float vectorGravLineWidth; // if 0 then vectorGravLineWidth is not used and value is taken from prefab
    [SerializeField] private bool displayVectorsFromMoonPoints;
    [SerializeField] private float tidalVectorSize;
    [SerializeField] private float vectorTidalLineWidth; // if 0 then vectorGravLineWidth is not used and value is taken from prefab
    [SerializeField] private bool displayMoonOrbit;
    [SerializeField] private bool displayMoonBulgeLine;
    [SerializeField] private bool displayMoonRefSystem;

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
        sim.MoonIsSquashed = moonIsSquashed;
        sim.simIsStationary = simIsStationary;
        sim.radiusScale = radiusScale;
        sim.MoonPeriodFactor = moonPeriodFactor;
        sim.dragMoonIsAllowed = dragMoonIsAllowed;
        sim.dragMoonEdgesIsAllowed = dragMoonEdgesIsAllowed;

        // We ask if vectors should not be drawn,
        // to be consistent with the toggle interaction in slide 4.
        sim.ActivationVectorsCM = displayVectorsFromMoonCM;
        sim.ActivationVectorsLR = displayVectorsFromMoonLR;

        sim.VectorGravScale = gravitationalVectorSize;
        sim.VectorTidalScale = tidalVectorSize;
        sim.VectorGravLineWidth = vectorGravLineWidth;
        sim.VectorTidalLineWidth = vectorTidalLineWidth;

        sim.ActivationPointsOnMoon = displayVectorsFromMoonPoints;
        sim.ActivationMoonOrbit = displayMoonOrbit;
        sim.ActivationMoonBulgeLine = displayMoonBulgeLine;
        sim.ActivationMoonRefSystem = displayMoonRefSystem;

        // 

        // CI:
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
        sim.UseMoonCI = useMoonCI;

        sim.IsAnimationThreeSteps = AnimationInThreeSteps;
        sim.oscillationMoonRotation = oscillationMoonRotation;
        sim.squashingAnimation = MoonSquashingAnimation;
        //if (AnimationInThreeSteps) {
        //    sim.ResetSimulation();
        //}

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
    }

    public void SetMoonPeriodFactor(float newfactor) {
        // Function useful for the button in the slide 2
        // So when transitioning slide2 to slide 1 and after slide1 to slide2, we keep
        // the moon period according the button configuration and not the initialization by the Slide Controller.
        moonPeriodFactor = newfactor;
    }

    public void SetMoonSquashed(bool newBool) {
        // Keep state of the interaction
        moonIsSquashed = newBool;
    }

    public void SetStationaryFlag(bool newBool) {
        // Keep state of the interaction
        simIsStationary = newBool;
    }

    public void SetActivationMoonVectorLRInverted(bool newBool) {
        // Keep state of the interaction
        displayVectorsFromMoonLR = !newBool;
        sim.ActivationVectorsLR = !newBool;
    }

    public void SetActivationMoonVectorCMInverted(bool newBool) {
        // Keep state of the interaction
        displayVectorsFromMoonCM = !newBool;
        sim.ActivationVectorsCM = !newBool;
    }

    public void SetActivationMoonTidalVectors(bool newBool) {
        // Keep state of the interaction
        displayVectorsFromMoonPoints = newBool;
        sim.ActivationPointsOnMoon = newBool;
    }

    public void SwitchVectorsDisplay(bool newBool) {
        SetActivationMoonVectorLRInverted(newBool);
        SetActivationMoonVectorCMInverted(newBool);
        SetActivationMoonTidalVectors(newBool);
    }
}
