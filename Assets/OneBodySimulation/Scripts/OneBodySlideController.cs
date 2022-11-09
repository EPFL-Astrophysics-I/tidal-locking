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
}
