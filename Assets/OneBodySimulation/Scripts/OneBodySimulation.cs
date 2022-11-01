using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using static Units;

public class OneBodySimulation : Simulation
{
    [Header("Simulation Properties")]
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    private UnitTime unitTime = UnitTime.Day;
    // Earth Radius to big
    // Moon Radius ?
    private UnitLength unitLength = UnitLength.Test;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;
    [HideInInspector] public float radiusScale = 10;

    [Header("Earth Parameters")]
    public bool earthIsRotating = false;
    private Vector3 initEarthPosition = Vector3.zero;
    private CelestialBody earth;

    [Header("Moon Parameters")] 
    public bool moonIsRotating = true;
    public bool MoonIsSquashed {
        get {return moon.IsSquashed;}
        set {
            // moon needs to be init, use Prefabs script ?
            if (moon!=null) {
                moon.IsSquashed = value;
            }
        }
    }
    private Vector3 initMoonPosition;
    private float moonDistance;
    private CelestialBody moon; 

    /* ************************************************************* */
    // Timer for resetting the simulation after one orbital period
    private float resetTimer;
    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);
    // Orbital period
    public float Period => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / Units.EarthMass(unitMass));
    /* ************************************************************* */

    public bool simIsStationary;

    /* ************************************************************* */
    private OneBodyPrefabs prefabs;

    /* ************ Mouse Clicks & Drag **************************** */
    private Camera mainCamera;
    private bool draggingMoonCenter;
    private bool draggingEdgeMoon;
    private Vector2 centerOfRotation;
    private Vector2 centerOfSpin;
    private float screenClickAngle;
    private float moonStartAngle;
    private float spinAngleOffset;
    private float viewportClickAngle;
    private void Awake()
    {
        /* From Unity doc:
         Awake is used to initialize any variables or game state before the game starts.
         Awake is called only once during the lifetime of the script instance.
         Awake is always called before any Start functions.
        */


        // Awake function because you need to recompute some values
        // if you change units parameters for exemple in the slideController.
        //      If unitTime, unitLength, unitMass we can put this in the Start function.

        // Create CelestialBodies and assign their properties
        //Reset();

        if (!TryGetComponent(out prefabs))
        {
            Debug.LogWarning("No OneBodyPrefab component found.");
            return;
        }

        prefabs.InstantiatePrefabs();

        // Compute Newton's constant only once
        _newtonG = NewtonG;

        mainCamera = Camera.main;

        draggingMoonCenter = false;
        draggingEdgeMoon = false;
    }

    private void Start()
    {
        /*
         use Start to pass any information back and forth
        */

        resetTimer = 0;

        earth = prefabs.earth;
        if (earth)
        {
            earth.Position = initEarthPosition;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(radiusScale * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        moon = prefabs.moon;
        if (earth)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(radiusScale * LunarRadius(unitLength));
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.RotationPeriod = Period;
        }

        CircularOrbit moonOrbit = prefabs.moonOrbit;
        if (moonOrbit) {
            moonOrbit.DrawOrbit(initEarthPosition, LunarDistance(unitLength), 100);
        }

        Vector moonCenterVec = prefabs.moonCenterVec;
        if (moonCenterVec) {
            moonCenterVec.TailPosition = moon.Position;
            //Vector2 pointPosition = sim.point.position - sim.earthPosition;
            //float angle = Mathf.Atan2(pointPosition.y, pointPosition.x);
            //Vector3 tide = tidalMagnitude * (2 * Mathf.Cos(angle) * Vector3.right - Mathf.Sin(angle) * Vector3.up);
            moonCenterVec.HeadPosition = earth.Position;
            moonCenterVec.Redraw();
        }
    }
    private void Update()
    {
        if (simIsStationary)
        {
            DragMoonAlongOrbit();
        }
    }

    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (simIsStationary)
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

    private void DragMoonAlongOrbit()
    {
        if(Input.GetMouseButtonDown(0)) {
            //Debug.Log("OnGameObject");

            // Do not need z component as the camera is "looking down",
            // simulation is in the plan (X, Y)
            Vector2 moonPositionInScreen = mainCamera.WorldToScreenPoint(moon.Position);
            //moonPositionInScreen.z = 0;
            //Debug.Log("Moon Screen Pos:" + moonPositionInScreen);
            Vector2 mousePositionOnClick = Input.mousePosition;
            //mousePositionInScreen.z = 0;
            //Debug.Log("Mouse Screen Pos:" + mousePositionOnClick);
            centerOfRotation = mainCamera.WorldToScreenPoint(earth.Position);

            centerOfSpin = mainCamera.WorldToScreenPoint(moon.Position);

            // Vector from moon center to mouse position
            Vector2 distance = mousePositionOnClick - moonPositionInScreen;
            float distanceMag = distance.magnitude;

            //float range = moon.transform.localScale.z - moon.transform.localScale.z*0.1f;
            // Temp because moon radius is too small until now:
            Vector3 a = new Vector3(moon.transform.localScale.x/2, moon.transform.localScale.x/2, 0);
            Vector2 moonRadius = mainCamera.WorldToScreenPoint(moon.Position + a);

            Vector2 range = moonRadius - moonPositionInScreen;
            float rangeMag = range.magnitude;
            //Vector2 innerRange = (moonRadius - (Vector2.Scale(moonRadius, spacing))) - moonPositionInScreen;
            float innerRangeMag = rangeMag * 0.6f;

            //Vector2 outerRange = (moonRadius + (Vector2.Scale(moonRadius, spacing))) - moonPositionInScreen;
            float outerRangeMag = rangeMag * 1.3f;

            //Debug.Log("range: " + range + " distance: " + distanceMag);
            // Check that the mouse click is in the center of the moon
            if (-innerRangeMag <= distanceMag && distanceMag <= innerRangeMag) 
            {
                moonStartAngle = Mathf.Atan2(moon.Position.y, moon.Position.x);
                draggingMoonCenter = true;                
            }
            else if ((-outerRangeMag <= distanceMag && distanceMag < -innerRangeMag) ||
                     (innerRangeMag < distanceMag && distanceMag <= outerRangeMag)) 
            {
                spinAngleOffset = (Mathf.Atan2(transform.right.y, transform.right.x) - Mathf.Atan2(distance.y, distance.x)) * Mathf.Rad2Deg;
                draggingEdgeMoon = true; 
            }
        }

        if (Input.GetMouseButton(0) && (draggingMoonCenter || draggingEdgeMoon))
        {
            // Get mouse position and displacement
            Vector2 currentMousePosition = Input.mousePosition;

            if (draggingMoonCenter) {
                Vector2 screenDisplacement = currentMousePosition - centerOfRotation;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x);
                
                
                // Compute new position
                Vector3 vectorR = moon.Position - earth.Position;
                float r = vectorR.magnitude;
                
                //float r = LunarDistance(unitLength);

                //Debug.Log("angle:" +  (deltaAngle) * Mathf.Rad2Deg + " oncli k : " + moonStartAngle + " r: " + r);

                float pointNewX = r * Mathf.Cos(deltaAngle);
                float pointNewY = r * Mathf.Sin(deltaAngle);
                Vector3 position = new Vector3(pointNewX, 0, pointNewY);

                // Assign new position
                moon.Position = earth.Position + position;
            } else {
                
                Vector2 screenDisplacement = currentMousePosition - centerOfSpin;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x) * Mathf.Rad2Deg;
                moon.transform.eulerAngles = Vector3.down * (deltaAngle + spinAngleOffset);

                //deltaAngle = deltaAngle - moon.transform.rotation.x;
                // draggingEdgeMoon is true

                //Quaternion mouseRot = Quaternion.AngleAxis(deltaAngle, Vector3.up);
                //Quaternion newRot = mouseRot * Quaternion.Inverse(moon.transform.rotation);
                //moon.transform.rotation = mouseRot;

                //moon.IncrementRotation(deltaAngle * Vector3.up);

                //prevMousePos = currentMousePosition;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMoonCenter = false;
            draggingEdgeMoon = false;
        }
    }
}
