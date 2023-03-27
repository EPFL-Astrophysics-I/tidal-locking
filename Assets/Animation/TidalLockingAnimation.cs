using UnityEngine;
using static Units;

public class TidalLockingAnimation : Simulation
{
    public CelestialBody earth;
    public CelestialBody moon;
    public Vector moonOrbitVector;
    public Vector moonReferenceVector;
    public LineRenderer orbitalRadius;

    // Factor by which to scale the earth and moon radii
    public float radiusScale = 10;
    // Number of discrete time steps to take per orbit
    public float numTimeSteps = 8;
    // Simulation time scale
    public float timeScale = 1;

    // Units system
    private UnitTime unitTime = UnitTime.Day;
    private UnitLength unitLength = UnitLength.EarthMoonDistance;
    private UnitMass unitMass = UnitMass.EarthMass;

    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);

    // Orbital period
    private float moonDistance = Units.LunarDistance(UnitLength.EarthMoonDistance);
    public float OrbitalPeriod
    {
        get
        {
            float ratio = Mathf.Pow(moonDistance, 3) / NewtonG / Units.EarthMass(unitMass);
            return 2 * Mathf.PI * Mathf.Sqrt(ratio);
        }
    }

    // Rotation period
    public float MoonRotationPeriod => moon ? moon.RotationPeriod : 0;
    public static event System.Action<float> OnUpdateMoonRotationPeriod;
    private float periodDifferenceSign;

    private bool animationIsPlaying;

    private void Awake()
    {
        // Compute Newton's constant once at the start
        _newtonG = NewtonG;

        Debug.Log("Orbital Period " + OrbitalPeriod + " " + unitTime.ToString());
    }

    private void Update()
    {
        if (paused || !earth || !moon) return;

        float deltaAngle = timeScale * Time.fixedDeltaTime * 360f;

        // Spin the earth on its axis
        float deltaAngleEarthRotation = deltaAngle;
        earth.IncrementRotation(deltaAngleEarthRotation * Vector3.down);

        // Move the moon in its orbit
        float deltaAngleOrbit = deltaAngle / OrbitalPeriod;
        Vector3 moonPosition = moon.Position - earth.Position;
        moon.Position = earth.Position + Quaternion.Euler(deltaAngleOrbit * Vector3.down) * moonPosition;

        // Rotate the moon about its axis
        float deltaAngleMoonRotation = deltaAngle / moon.RotationPeriod;
        moon.IncrementRotation(deltaAngleOrbit * Vector3.down);
        // moon.IncrementRotationSprite((deltaAngleMoonRotation - deltaAngleOrbit) * Vector3.down);
        moon.IncrementTextureOffset(-(deltaAngleMoonRotation - deltaAngleOrbit) / 360f * Vector2.right);

        // Rotate the moon orientation vector and orbital radius
        RedrawMoonOrbitVector(false);
        RedrawMoonReferenceVector(false);
        RedrawOrbitalRadius();
    }

    private void LateUpdate()
    {
        if (!animationIsPlaying || moon.RotationPeriod == OrbitalPeriod) return;

        float previousDelta = moon.RotationPeriod - OrbitalPeriod;

        // Update the moon's rotation period
        float percentDifference = Mathf.Abs(moon.RotationPeriod - OrbitalPeriod) / OrbitalPeriod;
        float rateFactor = periodDifferenceSign * percentDifference;
        moon.RotationPeriod -= 3 * rateFactor * timeScale * Time.deltaTime;
        OnUpdateMoonRotationPeriod?.Invoke(moon.RotationPeriod);

        // Stop condition for updating moon's rotation
        float currentDelta = moon.RotationPeriod - OrbitalPeriod;
        if (Mathf.Sign(previousDelta) != Mathf.Sign(currentDelta))
        {
            moon.RotationPeriod = OrbitalPeriod;
        }
    }

    public void StartAnimation(float sign)
    {
        periodDifferenceSign = sign;

        Resume();
        animationIsPlaying = true;

        // float anglePerStep = 360f / numTimeSteps;
        // StartCoroutine(TakeDiscreteStep(anglePerStep));
    }

    public void Reset()
    {
        Debug.Log("TidalLockingAnimation > Reset()");

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
            moon.SetTextureOffset(Vector2.zero);
            // moon.SetRotationSprite(Vector3.zero);

            // Recall that this starts a coroutine
            moon.IsSquashed = true;
        }

        RedrawMoonOrbitVector(true);
        RedrawMoonReferenceVector(true);
        RedrawOrbitalRadius();

        SetMoonRotationPeriod(OrbitalPeriod);

        Pause();
        animationIsPlaying = false;
    }

    public void SetMoonRotationPeriod(float value)
    {
        if (moon) moon.RotationPeriod = value;
    }

    private void RedrawMoonOrbitVector(bool firstDraw)
    {
        if (moonOrbitVector && moon)
        {
            // Vector3 positionOffset = -0.49f * moon.transform.localScale.x * moon.transform.right;
            moonOrbitVector.transform.position = moon.Position; // + positionOffset;
            if (firstDraw)
            {
                moonOrbitVector.components = 0.35f * Vector3.left;
            }
            else
            {
                moonOrbitVector.components = -moonOrbitVector.components.magnitude * moon.transform.right;
            }
            moonOrbitVector.Redraw();
        }
    }

    private void RedrawMoonReferenceVector(bool firstDraw)
    {
        if (moonReferenceVector && moon)
        {
            // Vector3 positionOffset = moon.transform.localScale.x * moon.transform.right;
            moonReferenceVector.transform.position = moon.Position; // + positionOffset;
            if (firstDraw)
            {
                moonReferenceVector.components = 0.35f * Vector3.left;
            }
            else
            {
                float angleOffset = moon.GetTextureOffset().x * 360f;
                Vector3 components = -moonReferenceVector.components.magnitude * moon.transform.right;
                // Rotate by angle offset
                components = Quaternion.Euler(angleOffset * Vector3.up) * components;
                moonReferenceVector.components = components;
            }
            moonReferenceVector.Redraw();
        }
    }

    private void RedrawOrbitalRadius()
    {
        if (orbitalRadius && earth && moon)
        {
            orbitalRadius.SetPositions(new Vector3[2] { earth.Position, moon.Position });
        }
    }
}
