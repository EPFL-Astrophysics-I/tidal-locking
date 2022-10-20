using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoBodySimulation : Simulation
{

    [Header("Simulation Properties")]
    [SerializeField, Min(0)] private float newtonG = 1f;
    [SerializeField, Min(1)] private int numSubsteps = 20;

    [Header("Body1 Parameters")]

    [SerializeField] private GameObject body1Prefab;
    private Transform body1;
    [SerializeField, Min(0)] private float mass1 = 1f;
    [SerializeField] private Vector3 initPosition1 = Vector3.left;
    [SerializeField] private Vector3 initVelocity1 = Vector3.up;

    [Header("Body2 Parameters")]

    [SerializeField] private GameObject body2Prefab;
    private Transform body2;
    [SerializeField, Min(0)] private float mass2 = 1f;
    [SerializeField] private Vector3 initPosition2 = Vector3.right;
    [SerializeField] private Vector3 initVelocity2 = Vector3.down;

    // Initial center of mass quantities
    private Vector3 initPositionCM;
    private Vector3 initVelocityCM;

    // Quantities of motion
    private float simulationTime;
    private float totalMass;
    private float reducedMass;
    private Vector3 r;  // r2 - r1
    private Vector3 v;  // time derivative of r
    private float theta;  // angular coordinate in the orbital plane

    private Vector3 body1momentum;
    private Vector3 body2momentum;

    private Vector3 r1;
    private Vector3 r2;

    // Conserved quantites (evaluated in CM frame)
    private float lagrangian; // L = T - U
    private float energy;
    private Vector3 angularMomentum;
    private float magnitudeL;
    private float period;

    // Start is called before the first frame update
    void Start()
    {
        if (body1Prefab && body2Prefab)
        {
            // Place the simulation at the center of mass
            totalMass = mass1 + mass2;
            reducedMass = mass1 * mass2 / totalMass;
            initPositionCM = (mass1 * initPosition1 + mass2 * initPosition2) / totalMass;
            initVelocityCM = (mass1 * initVelocity1 + mass2 * initVelocity2) / totalMass;
            transform.position = initPositionCM;

            body1 = Instantiate(body1Prefab, initPosition1, Quaternion.identity, transform).GetComponent<Transform>();
            body1.localScale = 2 * Mathf.Pow(3f * mass1 / 4f / Mathf.PI, 0.333f) * Vector3.one;
            // Why 2 ?
            body2 = Instantiate(body2Prefab, initPosition2, Quaternion.identity, transform).GetComponent<Transform>();
            body2.localScale = 2 * Mathf.Pow(3f * mass2 / 4f / Mathf.PI, 0.333f) * Vector3.one;
        }



        Reset();
    }

    // Update is called once per frame

    /*
    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        r = body2.localPosition - body1.localPosition;

        Debug.Log(transform.localPosition);

        simulationTime += Time.fixedDeltaTime;

        float substep = Time.fixedDeltaTime;

        Vector3 F2norm = - newtonG * mass1 * mass2 * r.normalized / r.sqrMagnitude;
        body2momentum = body2momentum + F2norm*substep;
        body1momentum = body1momentum - F2norm*substep;

        body1.localPosition += body1momentum*substep/mass1;
        body2.localPosition += body2momentum*substep/mass2;
    }*/

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (simulationTime >= period) {
            simulationTime = 0;
            r = initPosition2 - initPosition1;
            body1momentum = mass1 * initVelocity1;
            body2momentum = mass2 * initVelocity2;

            r1 = initPosition1;
            r2 = initPosition2;

            body1.localPosition = mass2 / totalMass * r;
            body2.localPosition = -mass1 / totalMass * r;
        }

        simulationTime += Time.fixedDeltaTime;

        r = body2.localPosition - body1.localPosition;

        float substep = Time.fixedDeltaTime / numSubsteps;

        for (int i = 0; i < numSubsteps; ++i) {
            StepForward(substep);
        }

        body1.localPosition = r1;
        body2.localPosition = r2;
    }

    private void StepForward(float deltaTime)
    {
        // Euler Method

        Vector3 specificForce = -(newtonG * totalMass / r.sqrMagnitude) * r.normalized;

        body2momentum += specificForce*deltaTime;
        body1momentum -= specificForce*deltaTime;

        r1 += body1momentum*deltaTime/mass1;
        r2 += body2momentum*deltaTime/mass2;

        r = r2 - r1;
    }

    private void Reset() {
        simulationTime = 0;

        r = initPosition2 - initPosition1;
        v = initVelocity2 - initVelocity1;

        r1 = initPosition1;
        r2 = initPosition2;

        
        energy = 0.5f * reducedMass * v.sqrMagnitude - newtonG * reducedMass / r.magnitude;

        float semiMajorAxis = -0.5f * newtonG * reducedMass / energy;

        float a = semiMajorAxis;
        period = 2 * Mathf.PI * Mathf.Sqrt(a * a * a / newtonG / totalMass);

        //angularMomentum = reducedMass * Vector3.Cross(r, v);
        //magnitudeL = angularMomentum.magnitude; // .magnitude is performance consuming
    
        body1momentum = mass1 * initVelocity1;
        body2momentum = mass2 * initVelocity2;
    }
}
