using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitSlideController : SimulationSlideController
{

    [Header("Parameters")]

    [SerializeField] private bool body1IsSquashed;
    [SerializeField] private bool body2IsSquashed;

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        OrbitSimulation sim = simulation as OrbitSimulation;
        sim.body1IsSquashed = body1IsSquashed;
        sim.body2IsSquashed = body2IsSquashed;
    }
}
