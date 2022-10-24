using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragMoonSlideController : SimulationSlideController
{
    [Header("Parameters")]

    [SerializeField] private bool body1IsSquashed;
    [SerializeField] private bool body2IsSquashed;

    [SerializeField] private bool stationary;

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        DragMoonSimulation sim = simulation as DragMoonSimulation;
        sim.body1IsSquashed = body1IsSquashed;
        sim.body2IsSquashed = body2IsSquashed;
        sim.stationary = stationary;
    }
}
