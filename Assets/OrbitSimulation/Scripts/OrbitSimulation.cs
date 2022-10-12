using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitSimulation : Simulation
{
    [Header("Simulation Properties")]
    [SerializeField, Min(0)] private float newtonG = 1f;
    [SerializeField, Min(1)] private int numSubsteps = 20;
    [SerializeField, Min(0)] private float mass1 = 1f;
    [SerializeField, Min(0)] private float mass2 = 1f;

    [Header("Body1 Parameters")]

    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private Vector3 initPosition1 = Vector3.left;
    [SerializeField] private Vector3 initVelocity1 = Vector3.up;

    [Header("Body2 Parameters")]
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private Vector3 initPosition2 = Vector3.right;
    [SerializeField] private Vector3 initVelocity2 = Vector3.down;                          

    // References to the actual transforms held in TwoBodyPrefabs
    private Transform body1;
    private Transform body2;

    // Initial center of mass quantities
    private Vector3 initPositionCM;
    private Vector3 initVelocityCM;

    // Quantities of motion
    private float time;
    private float resetTimer;
    private float totalMass;
    private float reducedMass;
    [HideInInspector] public Vector3 r;  // r1 - r2
    [HideInInspector] public Vector3 v;  // time derivative of r
    [HideInInspector] public float theta;  // angular coordinate in the orbital plane

    // Conserved quantites (evaluated in CM frame)
    private float energy;
    private float period;
    private Vector3 angularMomentum;
    private float magnitudeL;
    private float semiMajorAxis;
    private float eccentricity;
    private float initTheta;

    // Coordinate system with angular momentum along the z-axis and x, y in the orbital plane
    private Vector3 xHat = Vector3.right;
    private Vector3 yHat = Vector3.up;
    private Vector3 zHat = Vector3.forward;


    // Start is called before the first frame update
    void Start()
    {
        if (earthPrefab && moonPrefab)
        {
            // Place the simulation at the center of mass
            totalMass = mass1 + mass2;
            initPositionCM = (mass1 * initPosition1 + mass2 * initPosition2) / totalMass;
            initVelocityCM = (mass1 * initVelocity1 + mass2 * initVelocity2) / totalMass;
            transform.position = initPositionCM;

            body1 = Instantiate(earthPrefab, initPosition1, Quaternion.identity, transform).GetComponent<Transform>();
            body1.localScale = 2 * Mathf.Pow(3f * mass1 / 4f / Mathf.PI, 0.333f) * Vector3.one;

            body2 = Instantiate(moonPrefab, initPosition2, Quaternion.identity, transform).GetComponent<Transform>();
            body2.localScale = 2 * Mathf.Pow(3f * mass2 / 4f / Mathf.PI, 0.333f) * Vector3.one;
        }

        Reset();

        // Orbital plane coordinate system
        zHat = angularMomentum.normalized;
        //xHat = Quaternion.AngleAxis(-initTheta, zHat) * r.normalized;
        xHat = -r.normalized;
        yHat = Vector3.Cross(zHat, xHat);
    }

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (resetTimer >= period)
        {
            resetTimer = 0;
            r = initPosition1 - initPosition2;
            v = initVelocity1 - initVelocity2;
            body1.localPosition = mass2 / totalMass * r;
            body2.localPosition = -mass1 / totalMass * r;

            theta = initTheta;
        }

        time += Time.fixedDeltaTime;
        resetTimer += Time.fixedDeltaTime;

        // Move the simulation to the new CM position
        transform.position = CenterOfMassPosition(time);

        // Bound orbits
        if (energy < 0)
        {
            // Solve the equation of motion for theta
            float substep = Time.fixedDeltaTime / numSubsteps;
            for (int i = 0; i < numSubsteps; i++)
            {
                StepForwardThetaR(substep);
            }
        }
        else
        {
            // Solve the equation of motion for r
            float substep = Time.fixedDeltaTime / numSubsteps;
            for (int i = 1; i <= numSubsteps; i++)
            {
                StepForward(substep);
            }
        }

        // Update each body's position in the CM frame
        body1.localPosition = mass2 / totalMass * r;
        body2.localPosition = -mass1 / totalMass * r;

        Vector3 rotationAxis = (Vector3.up).normalized;
        body1.Rotate(rotationAxis, -10 * Time.deltaTime);

        body2.Rotate(rotationAxis, -100 * Time.deltaTime);
    }

    private void StepForward(float deltaTime)
    {
        // Euler Method

        // Solve the equation of motion for the difference vector r in the CM frame
        // (1) dr/dt = v
        // (2) dv/dt = -(G * M / r^2) * rhat

        // Change in velocity during time deltaTime
        Vector3 specificForce = -(newtonG * totalMass / r.sqrMagnitude) * r.normalized;
        Vector3 deltaV = specificForce * deltaTime;
        r += v * deltaTime;
        v += deltaV;
    }

    private void StepForwardThetaR(float deltaTime)
    {
        float angularSpeed = -magnitudeL / reducedMass / r.sqrMagnitude;
        theta += angularSpeed * deltaTime;
        float a = semiMajorAxis;
        float e = eccentricity;
        float rMagnitude = a * (1f - e * e) / (1f + e * Mathf.Cos(theta));
        r = rMagnitude * (Mathf.Cos(theta) * xHat + Mathf.Sin(theta) * yHat);
    }

    public void Reset()
    {
        time = 0;
        resetTimer = 0;

        r = initPosition1 - initPosition2;
        v = initVelocity1 - initVelocity2;

        // Compute conserved quantities (in CM frame)
        reducedMass = mass1 * mass2 / totalMass;
        energy = 0.5f * reducedMass * v.sqrMagnitude - newtonG * reducedMass * totalMass / r.magnitude;
        angularMomentum = reducedMass * Vector3.Cross(r, v);
        magnitudeL = angularMomentum.magnitude;

        // Compute orbital properties
        semiMajorAxis = -0.5f * newtonG * reducedMass * totalMass / energy;
        eccentricity = Mathf.Sqrt(1f + 2f * energy * Mathf.Pow(magnitudeL / newtonG / reducedMass / totalMass, 2) / reducedMass);
        // Period and initial theta
        if (energy >= 0)
        {
            // Unbound orbit
            period = float.PositiveInfinity;
        }
        else
        {
            // Bound orbit
            float a = semiMajorAxis;
            period = 2 * Mathf.PI * Mathf.Sqrt(a * a * a / newtonG / totalMass);

            float e = eccentricity;
            // Avoid NANs by making sure the argument of arccos is strictly between -1 and 1
            float arg = Mathf.Clamp(((a * (1f - e * e) / r.magnitude) - 1f) / e, -1f, 1f);
            initTheta = Mathf.Acos(arg);
            theta = initTheta;
        }
    }

    // Center of mass position at any time
    public Vector3 CenterOfMassPosition(float time)
    {
        return initPositionCM + initVelocityCM * time;
    }
}
