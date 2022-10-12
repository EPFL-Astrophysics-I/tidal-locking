using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitSlideController : SimulationSlideController
{

    // Start is called before the first frame update
    public override void InitializeSlide()
    {
        OrbitSimulation sim = simulation as OrbitSimulation;
    }
}
