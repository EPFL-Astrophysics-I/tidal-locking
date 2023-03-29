using UnityEngine;

public class DraggableSimSlideController : SimulationSlideController
{
    [Header("Options")]
    public DraggableSimulation.DraggableBody draggableBody = default;
    public bool showOrbit;
    public bool showOrbitalRadius;

    private DraggableSimulation sim;

    public override void InitializeSlide()
    {
        Debug.Log("DraggableSimSlideController > InitializeSlide");

        // Get reference to the specific simulation
        sim = simulation as DraggableSimulation;
        if (sim == null)
        {
            Debug.Log("No simulation assigned in AnimationSlideController");
            return;
        }

        sim.draggableBody = draggableBody;

        sim.showOrbit = showOrbit;
        sim.showOrbitalRadius = showOrbitalRadius;

        Reset();
    }

    public void Reset()
    {
        // Return the earth and moon to their starting positions
        if (sim)
        {
            sim.Reset();
        }
    }
}
