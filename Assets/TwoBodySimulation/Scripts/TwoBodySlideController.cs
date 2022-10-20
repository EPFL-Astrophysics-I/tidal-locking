using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoBodySlideController : SimulationSlideController
{
    public override void InitializeSlide()
    {
        TwoBodySimulation sim = simulation as TwoBodySimulation;
    }
}
