using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodySlideController : SimulationSlideController
{
    [Header("Parameters")]
    [SerializeField] private float radiusScale;
    [SerializeField] private bool moonIsSquashed;
    [SerializeField] private bool simIsStationary;
    [SerializeField] private float moonPeriodFactor;

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        OneBodySimulation sim = simulation as OneBodySimulation;
        sim.MoonIsSquashed = moonIsSquashed;
        sim.simIsStationary = simIsStationary;
        sim.radiusScale = radiusScale;
        sim.MoonPeriodFactor = moonPeriodFactor;
    }

    public void SetMoonPeriodFactor(float newfactor) {
        // Function useful for the button in the slide 2
        // So when transitioning slide2 to slide 1 and after slide1 to slide2, we keep
        // the moon period according the button configuration and not the initialization by the Slide Controller.
        moonPeriodFactor = newfactor;
    }
}
