using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Units;

public class OneBodySimulation : Simulation
{
    [Header("Simulation Properties")]
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    private UnitTime unitTime = UnitTime.Day;

    // Earth Radius to big
    // Moon Radius ?
    private UnitLength unitLength = UnitLength.EarthRadius;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;

    [Header("Earth Parameters")]
    public bool earthIsRotating = false;
    public bool earthIsDeforming = false;
    private Vector3 initEarthPosition = Vector3.zero;
    private float earthRadius = 1;
    private float earthMass = 1;
    private CelestialBody earth;

    [Header("Moon Parameters")]
    public bool moonIsRotating = true;
    public bool moonIsDeforming = false;
    private Vector3 initMoonPosition;
    private float moonDistance;
    private CelestialBody moon;

    [Header("Prefabs")]
    public GameObject earthPrefab = default;
    public GameObject moonPrefab = default;

    // Timer for resetting the simulation after one orbital period
    private float resetTimer;

    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);

    // Orbital period
    public float Period => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / earthMass);

    private void Awake()
    {
        // Awake function because you need to recompute some values
        // if you change units parameters for exemple in the slideController.
        //      If unitTime, unitLength, unitMass we can put this in the Start function.

        // Create CelestialBodies and assign their properties
        //Reset();

        // Compute Newton's constant only once
        _newtonG = NewtonG;
    }

    private void Start()
    {
        Reset();
        Debug.Log(unitLength);
    }

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (resetAfterOnePeriod)
        {
            // Re-establish the system to exact initial positions after one period to avoid numerical errors
            if (resetTimer >= Period)
            {
                resetTimer = 0;
                moon.Position = initMoonPosition;
            }

            resetTimer += timeScale * Time.fixedDeltaTime;
        }

        // Solve the equation of motion
        float substep = timeScale * Time.fixedDeltaTime / numSubsteps;
        for (int i = 1; i <= numSubsteps; i++)
        {
            StepForward(substep);
        }

        if (earthIsRotating)
        {
            float deltaAngle = timeScale * Time.fixedDeltaTime * 360 / earth.RotationPeriod;
            earth.IncrementRotation(deltaAngle * Vector3.down);
        }

        if (moonIsRotating)
        {
            float deltaAngle = timeScale * Time.fixedDeltaTime * 360 / moon.RotationPeriod;
            moon.IncrementRotation(deltaAngle * Vector3.down);
        }
    }

    private void StepForward(float deltaTime)
    {
        // Solve the equation of motion in polar coordinates
        Vector3 vectorR = moon.Position - earth.Position;
        float theta = Mathf.Atan2(vectorR.z, vectorR.x);

        // Update moon position
        float angularMomentum = Mathf.Sqrt(NewtonG * earth.Mass * moonDistance);
        theta += angularMomentum * deltaTime / vectorR.sqrMagnitude;
        float r = vectorR.magnitude;
        Vector3 position = new Vector3(r * Mathf.Cos(theta), 0, r * Mathf.Sin(theta));
        moon.Position = earth.Position + position;
    }

    public void Reset()
    {
        resetTimer = 0;

        // Earth
        if (earthPrefab != null)
        {
            if (earth == null)
            {
                Debug.Log("INIT");
                earth = Instantiate(earthPrefab, initEarthPosition, Quaternion.identity, transform).GetComponent<CelestialBody>();
                //earth.gameObject.name = "Earth";
            }
            Debug.Log(earth);
            earth.Position = initEarthPosition;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(earthRadius * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        // Moon
        if (moonPrefab != null)
        {
            if (moon == null)
            {
                moon = Instantiate(moonPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
                //moon.gameObject.name = "Moon";
            }
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(earthRadius * LunarRadius(unitLength));
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.RotationPeriod = Period;
        }
    }
}
