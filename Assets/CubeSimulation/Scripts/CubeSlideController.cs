using UnityEngine;

public class CubeSlideController : SimulationSlideController
{
    [Header("Parameters")]
    [SerializeField] private bool cubeIsSpinning;

    public override void InitializeSlide()
    {
        CubeSimulation sim = simulation as CubeSimulation;
        sim.cubeIsSpinning = cubeIsSpinning;
    }
}
