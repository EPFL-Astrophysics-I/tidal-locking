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
        sim.IsAnimationThreeSteps = AnimationInThreeSteps;

        // 

        // CI:
        sim.useMoonCI = useMoonCI;
        sim.angleMoonOrbitInit = angleMoonOrbitInit;
        sim.angleMoonSpinInit = angleMoonSpinInit;
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
}
