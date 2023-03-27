using System.Collections;
using UnityEngine;
using static Units;

public class TidalLockingAnimation : Simulation
{
    public CelestialBody earth;
    public CelestialBody moon;
    public Vector moonVector;

    // Factor by which to scale the earth and moon radii
    public float radiusScale = 10;
    // Number of discrete time steps to take per orbit
    public float numTimeSteps = 8;
    // Simulation time scale
    public float timeScale = 1;

    private UnitTime unitTime = UnitTime.Day;
    private UnitLength unitLength = UnitLength.EarthMoonDistance;
    private UnitMass unitMass = UnitMass.EarthMass;

    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);

    // Orbital period
    private float moonDistance = Units.LunarDistance(UnitLength.EarthMoonDistance);
    public float OrbitalPeriod => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / Units.EarthMass(unitMass));

    // Moon rotation period
    public float moonRotationPeriod;

    private void Awake()
    {
        // Compute Newton's constant once at the start
        _newtonG = NewtonG;

        Debug.Log("Orbital Period " + OrbitalPeriod + " " + unitTime.ToString());
    }

    private void Start()
    {
        Reset();
    }

    private void Update()
    {
        if (paused || !earth || !moon) return;

        // Spin the earth on its axis
        float deltaAngleEarthRotation = timeScale * Time.deltaTime * 360;
        earth.IncrementRotation(deltaAngleEarthRotation * Vector3.down);

        // Move the moon in its orbit
        float deltaAngleOrbit = timeScale * Time.deltaTime * 360f / OrbitalPeriod;
        Vector3 moonPosition = moon.Position - earth.Position;
        moon.Position = earth.Position + Quaternion.Euler(deltaAngleOrbit * Vector3.down) * moonPosition;

        // Rotate the moon about its axis
        float deltaAngleMoonRotation = timeScale * Time.deltaTime * 360f / moon.RotationPeriod;
        moon.IncrementRotation(deltaAngleMoonRotation * Vector3.down);

        // Rotate the moon orientation vector
        if (moonVector)
        {
            moonVector.transform.position = moon.Position - 0.5f * moon.transform.localScale.x * moon.transform.right;
            moonVector.components = -moonVector.components.magnitude * moon.transform.right;
            moonVector.Redraw();
        }
    }

    public void StartAnimation()
    {
        Resume();

        // float anglePerStep = 360f / numTimeSteps;
        // StartCoroutine(TakeDiscreteStep(anglePerStep));
    }

    public void Reset()
    {
        if (earth)
        {
            earth.Position = Vector3.zero;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(radiusScale * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
            earth.SetRotation(Vector3.zero);
        }

        if (moon)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(radiusScale * LunarRadius(unitLength));
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.SetRotation(Vector3.zero);

            // moon.RotationPeriod = Period * moonPeriodFactor;
        }

        if (moonVector)
        {
            moonVector.transform.position = moon.Position - 0.5f * moon.transform.localScale.x * moon.transform.right;
            moonVector.components = 0.2f * Vector3.left;
            moonVector.lineWidth = 0.05f;
            moonVector.Redraw();
        }

        Pause();
    }

    public void SetMoonRotationPeriod(float value)
    {
        if (moon) moon.RotationPeriod = value;
    }
}
