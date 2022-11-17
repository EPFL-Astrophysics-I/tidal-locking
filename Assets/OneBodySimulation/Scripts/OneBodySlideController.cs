using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodySlideController : SimulationSlideController
{
    [Header("Parameters")]
    [SerializeField] private float radiusScale;
    [SerializeField] private float moonPeriodFactor;
    [SerializeField] private bool moonIsSquashed;
    [SerializeField] private bool simIsStationary;
    [SerializeField] private bool dragMoonIsAllowed;
    [SerializeField] private bool NotDisplayVectorsFromMoonCM;
    [SerializeField] private bool NotDisplayVectorsFromMoonLR;
    [SerializeField] private bool displayVectorsFromMoonPoints;
    [SerializeField] private bool AnimationInThreeSteps;

    [Header("Initial Condition")]
    [SerializeField] private bool useMoonCI;
    [SerializeField] private float angleMoonOrbitInit;
    [SerializeField] private float angleMoonSpinInit;

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        OneBodySimulation sim = simulation as OneBodySimulation;
        sim.MoonIsSquashed = moonIsSquashed;
        sim.simIsStationary = simIsStationary;
        sim.radiusScale = radiusScale;
        sim.MoonPeriodFactor = moonPeriodFactor;
        sim.dragMoonIsAllowed = dragMoonIsAllowed;

        // We ask if vectors should not be drawn,
        // to be consistent with the toggle interaction in slide 4.
        sim.ActivationVectorsCM = NotDisplayVectorsFromMoonCM;
        sim.ActivationVectorsLR = NotDisplayVectorsFromMoonLR;

        sim.ActivationPointsOnMoon = displayVectorsFromMoonPoints;

        // 

        // CI:
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
        sim.UseMoonCI = useMoonCI;

        sim.IsAnimationThreeSteps = AnimationInThreeSteps;
        //if (AnimationInThreeSteps) {
        //    sim.ResetSimulation();
        //}
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

    public void SetActivationMoonVectorLR(bool newBool) {
        // Keep state of the interaction
        NotDisplayVectorsFromMoonLR = newBool;
    }

    public void SetActivationMoonVectorCM(bool newBool) {
        // Keep state of the interaction
        NotDisplayVectorsFromMoonCM = newBool;
    }
}
