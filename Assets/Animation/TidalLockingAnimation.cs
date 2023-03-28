using System.Collections;
using UnityEngine;
using static Units;

public class TidalLockingAnimation : Simulation
{
    public CelestialBody earth;
    public CelestialBody moon;
    public Vector moonOrbitVector;
    public Vector moonReferenceVector;
    public LineRenderer orbitalRadius;
    public LineRenderer moonOrbit;

    // Factor by which to scale the earth and moon radii
    public float radiusScale = 10;
    // Simulation time scale
    public float timeScale = 1;

    public bool useDiscreteSteps;
    public int numSteps = 8;
    public float maxStepAngle = 45;
    private float orbitalAngleOffset = 0;

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
    private Coroutine reshapeAnimation;

    private void Awake()
    {
        // Compute Newton's constant once at the start
        _newtonG = NewtonG;

        Debug.Log("Orbital Period " + OrbitalPeriod + " " + unitTime.ToString());
    }

    private void Update()
    {
        if (paused || !earth || !moon || !animationIsPlaying) return;

        float deltaTheta = timeScale * Time.deltaTime * 360f;

        // Spin the earth on its axis
        float deltaAngleEarthRotation = deltaTheta;
        earth.IncrementRotation(deltaAngleEarthRotation * Vector3.down);

        // Move the moon in its orbit
        float deltaAngleOrbit = deltaTheta / OrbitalPeriod;
        Vector3 moonPosition = moon.Position - earth.Position;
        moon.Position = earth.Position + Quaternion.Euler(deltaAngleOrbit * Vector3.down) * moonPosition;
        orbitalAngleOffset += deltaAngleOrbit;

        // Rotate the moon about its axis
        float deltaAngleMoonRotation = deltaTheta / moon.RotationPeriod;

        // Difference between moon axis rotation angle and ortibal angle
        float deltaAngleDifference = deltaAngleMoonRotation - deltaAngleOrbit;

        if (useDiscreteSteps)
        {
            if (orbitalAngleOffset < maxStepAngle)
            {
                moon.IncrementRotation(deltaAngleMoonRotation * Vector3.down);
            }
            else
            {
                Pause();
                reshapeAnimation = StartCoroutine(ReshapeSequence(1.4f));
            }
        }
        else
        {
            moon.IncrementRotation(deltaAngleOrbit * Vector3.down);
            // moon.IncrementRotationSprite((deltaAngleMoonRotation - deltaAngleOrbit) * Vector3.down);
            moon.IncrementTextureOffset(-deltaAngleDifference / 360f * Vector2.right);
        }

        // Rotate the moon orientation vector and orbital radius
        RedrawMoonOrbitVector(false);
        RedrawMoonReferenceVector(false);
        RedrawOrbitalRadius();
    }

    private void LateUpdate()
    {
        if (useDiscreteSteps || !animationIsPlaying || moon.RotationPeriod == OrbitalPeriod) return;

        float previousDelta = moon.RotationPeriod - OrbitalPeriod;

        // Update the moon's rotation period
        float percentDifference = Mathf.Abs(moon.RotationPeriod - OrbitalPeriod) / OrbitalPeriod;
        float rateFactor = 5 * periodDifferenceSign * Mathf.Pow(percentDifference, 0.8f);
        moon.RotationPeriod -= rateFactor * timeScale * Time.deltaTime;
        OnUpdateMoonRotationPeriod?.Invoke(moon.RotationPeriod);

        // Stop condition for continuously updating moon's rotation
        float currentDelta = moon.RotationPeriod - OrbitalPeriod;
        if (Mathf.Sign(previousDelta) != Mathf.Sign(currentDelta))
        {
            moon.RotationPeriod = OrbitalPeriod;
        }
    }

    public void StartAnimation(float sign, bool animationIsDiscrete, float stepAngle)
    {
        periodDifferenceSign = sign;
        useDiscreteSteps = animationIsDiscrete;
        maxStepAngle = stepAngle;

        Resume();
        animationIsPlaying = true;
    }

    private IEnumerator ReshapeSequence(float lerpTime)
    {
        // Determine the angle by which to rotate the moon to realign its bulge
        float sign = -periodDifferenceSign;
        float deltaAngle = sign * Vector3.Angle(moon.transform.right, moon.Position - earth.Position);
        Quaternion startRotation = moon.transform.localRotation;
        Quaternion targetRotation = Quaternion.Euler(deltaAngle * Vector3.up) * startRotation;

        float startTextureOffset = moon.GetTextureOffset().x;
        float targetTextureOffset = startTextureOffset - deltaAngle / 360f;

        yield return new WaitForSeconds(0.5f);

        float time = 0;
        while (time < lerpTime)
        {
            time += Time.deltaTime;
            Quaternion rotation = Quaternion.Slerp(startRotation, targetRotation, time / lerpTime);
            moon.transform.localRotation = rotation;

            float offset = Mathf.Lerp(startTextureOffset, targetTextureOffset, time / lerpTime);
            moon.SetTextureOffset(offset * Vector2.right);

            RedrawMoonOrbitVector(false);
            RedrawMoonReferenceVector(false);

            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        // TODO decrease / increase the period now !!
        //
        //  ...
        //
        //

        orbitalAngleOffset = 0;
        Resume();
    }

    public void Reset()
    {
        Debug.Log("TidalLockingAnimation > Reset()");

        if (reshapeAnimation != null)
        {
            StopCoroutine(reshapeAnimation);
            reshapeAnimation = null;
        }

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

            // Recall that this starts a coroutine
            moon.IsSquashed = true;
        }

        RedrawMoonOrbitVector(true);
        RedrawMoonReferenceVector(true);
        RedrawOrbitalRadius();

        SetMoonRotationPeriod(OrbitalPeriod);

        Pause();
        animationIsPlaying = false;
        orbitalAngleOffset = 0;
    }

    public void SetMoonRotationPeriod(float value)
    {
        Debug.Log("TidalLockingAnimation > Moon rotation period : " + value);
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

    public void SetTimeScale(float value)
    {
        timeScale = value;
    }
}
