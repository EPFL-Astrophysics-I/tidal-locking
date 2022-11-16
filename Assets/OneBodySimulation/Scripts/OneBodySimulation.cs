using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using static Units;

public class OneBodySimulation : Simulation
{
    /* ************************************************************* */

    [Header("Simulation Properties")]
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    private UnitTime unitTime = UnitTime.Day;
    // Earth Radius to big
    // Moon Radius ?
    private UnitLength unitLength = UnitLength.EarthMoonDistanceFactor;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;
    public bool IsAnimationThreeSteps;
    private float timerAnimation;
    private float timeStep1Animation = 3;
    private float timeStep2Animation = 5;
    private float timeStep3Animation = 1000;
    private bool angleOffsetIsCompute = false;
    private float angleLocked;
    private float angleOffset;

    /* ************************************************************* */

    [Header("Earth Parameters")]
    public bool earthIsRotating = false;
    private Vector3 initEarthPosition = Vector3.zero;
    private CelestialBody earth;

    /* ************************************************************* */

    [Header("Moon Parameters")] 
    public bool moonIsRotating = true;
    private Vector3 initMoonPosition;
    private float moonDistance;
    private CelestialBody moon; 

    /* ************************************************************* */

    private bool activationPointsOnMoon = false;
    public bool ActivationPointsOnMoon {
        get {
            return activationPointsOnMoon;
        }
        set {
            activationPointsOnMoon = value;
            prefabs.SetPointsOnMoonActivation(value);
            if (value)
                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance);
        }
    }

    private bool activationVectorsCM = false;
    public bool ActivationVectorsCM {
        get {
            return activationVectorsCM;
        }
        set {
            activationVectorsCM = value;
            prefabs.SetVectorCMactivation(value);
        }
    }

    private bool activationVectorsLR = false;
    public bool ActivationVectorsLR {
        get {
            return activationVectorsLR;
        }
        set {
            activationVectorsLR = value;
            prefabs.SetVectorLRactivation(value);
        }
    }

    /* ************************************************************* */
    // Timer for resetting the simulation after one orbital period
    private float resetTimer;
    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);
    // Orbital period
    public float Period => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / Units.EarthMass(unitMass));

    /* ************************************************************* */
    /* *** Parameters changed by SlideController */
    //[HideInInspector] public bool simIsStationary { get; set; } = false;
    public bool simIsStationary = false;
    [HideInInspector] public float radiusScale = 10;
    private bool moonSquashed = false;
    public bool MoonIsSquashed {
        get {
            if (moon!=null)
                return moon.IsSquashed;
            else
                return moonSquashed;
        }
        set {
            moonSquashed = value;
            if (moon!=null) {
                moon.IsSquashed = value;
                //setMoonPointPosition();
                //setGravitationalVectors();
            }
        }
    }

    private float moonPeriodFactor;
    public float MoonPeriodFactor {
        get {return moonPeriodFactor;}
        set {
            moonPeriodFactor = value;
            if (moon!=null) {
                moon.RotationPeriod = Period * value;
                if (value==1) {
                    // Reset rotation of the moon
                    // Keep the same face toward the earth,
                    // Otherwise from moonPeriodFactor != 1 to moonPeriodFactor = 1
                    // The face of the moon will not be the same.
                    moon.transform.rotation = Quaternion.Euler(0, 180, 0);
                    float deltaAngle = timeScale * resetTimer * 360 / moon.RotationPeriod;
                    moon.IncrementRotation(deltaAngle * Vector3.down);
                }
            }
        }
    }

    private bool useMoonCI;
    public bool UseMoonCI {
        get {
            return useMoonCI;
        }
        set {
            useMoonCI = value;
            if (value && moon!=null)
                SetMoonInitialCondition();
        }
    }
    public float angleMoonOrbitInit;
    public float angleMoonSpinInit;

    /* ************************************************************* */
    private OneBodyPrefabs prefabs;

    /* ************ Mouse Clicks & Drag **************************** */
    private Camera mainCamera;
    public bool dragMoonIsAllowed;
    private bool draggingMoonCenter;
    private bool draggingEdgeMoon;
    private Vector2 centerOfRotation;
    private Vector2 centerOfSpin;
    private float moonStartAngle;
    private float mouseStartAngle;
    private Vector3 moonStartSpin;
    /* ************************************************************* */

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

    /* ************************************************************* */
    private void Start()
    {
        /*
         use Start to pass any information back and forth
        */

        resetTimer = 0;
        timerAnimation = 0;

        earth = prefabs.earth;
        if (earth)
        {
            earth.Position = initEarthPosition;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(radiusScale * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        moon = prefabs.moon;
        if (moon)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(radiusScale * LunarRadius(unitLength));
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.RotationPeriod = Period * moonPeriodFactor;
            MoonIsSquashed = moonSquashed;
        }

        // USE CI :
        if (useMoonCI) {
            SetMoonInitialCondition();
        }

        prefabs.setMoonPointPosition();

        CircularOrbit moonOrbit = prefabs.moonOrbit;
        if (moonOrbit) {
            moonOrbit.DrawOrbit(initEarthPosition, LunarDistance(unitLength), 100);
        }

        prefabs.SetPointsOnMoonActivation(activationPointsOnMoon);
        prefabs.setGravitationalVectors(NewtonG, moonDistance);

        prefabs.DrawLineEarthMoon();
    }

    /* ************************************************************* */
    private void Update()
    {
        if (simIsStationary)
        {
            if (dragMoonIsAllowed)
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
            if (Time.fixedDeltaTime <= moon.squashTimer) {
                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance);
            }
            return;
        }

        if (IsAnimationThreeSteps)
        {
            // Three Steps Animation for slide 5:
            if (timerAnimation < timeStep1Animation) {
                UpdateOneBodySimulation();
                timerAnimation += Time.fixedDeltaTime;
                return;
            }

            if (timerAnimation < timeStep1Animation+5f) {
                if (!angleOffsetIsCompute) {
                    StartCoroutine(LerpMoonRotation(5f));
                    angleOffsetIsCompute = true;
                }
                /*
                float substep = angleOffset / (timeStep2Animation * Time.fi);
                Debug.Log(substep);
                moon.IncrementRotation(substep * Vector3.down);

                timerAnimation += Time.fixedDeltaTime;*/
                timerAnimation += Time.fixedDeltaTime;
                return;
            }
            angleOffsetIsCompute = false;
            timerAnimation = 0;
            return;
        }

        UpdateOneBodySimulation();
    }

    private void UpdateOneBodySimulation() {
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
            //setGravitationalVectors();
            //setMoonPointPosition();
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

        prefabs.DrawLineEarthMoon();
        prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance);
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

    /* ************************************************************* */
    public void ResetSimulation()
    {
        resetTimer = 0;
        timerAnimation = 0;
        angleOffsetIsCompute = false;

        if (earth)
        {
            earth.Position = initEarthPosition;
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        if (moon)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        prefabs.DrawLineEarthMoon();
        prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance);
    }
    /* ************************************************************* */
    private void SetMoonInitialCondition() {
        //moon.Position = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit));
        //moon.SetRotation(new Vector3(0, angleMoonSpinInit, 0));
        //initMoonPosition = moon.Position;

        Vector3 targetPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit));
        Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);

        StartCoroutine(LerpMoonPosition(moon.Position, targetPosition, 5f));
    }

    public void ChangeMoonPeriod(float periodFactor)
    {
        moon.RotationPeriod = Period*periodFactor;
    }
    public void ToggleStationaryFlag()
    {
        simIsStationary = !simIsStationary;
    }

    /* ************************************************************* */
    IEnumerator LerpMoonPosition(Vector3 start, Vector3 target, float lerpTime) {
        float time = 0;

        while (time < lerpTime) {
            time += Time.fixedDeltaTime;
            moon.Position = (Vector3.Lerp(start, target, time/lerpTime));

            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance);
            prefabs.DrawLineEarthMoon();
            
            yield return null;
        }
    }

    IEnumerator LerpMoonRotation(float lerpTime) {
        float time = 0;
        //float startAngle = 180 + moon.transform.rotation.eulerAngles.y;
        //Vector3 startVec = startAngle * Vector3.down;
        Vector3 startVec = moon.transform.rotation.eulerAngles;

        Vector3 targetVec;
        targetVec = Quaternion.Euler(0, 180, 0).eulerAngles;
        float deltaAngle = timeScale * resetTimer * 360 / Period;
        targetVec += deltaAngle * Vector3.down;

        /*
        float targetAngle = timeScale * resetTimer * 360 / Period;
        Vector3 targetVec;
        if (startVec.y > targetAngle) {
            targetVec = targetAngle * Vector3.down;
        } else {
            targetVec = targetAngle * Vector3.down;
        }

        targetVec += startVec;
        */

        Debug.Log(startVec + " to " + targetVec);

        while (time < lerpTime) {
            time += Time.fixedDeltaTime;
            moon.SetRotation(Vector3.Lerp(startVec, targetVec, time/lerpTime));
            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance);
            
            yield return null;
        }

        moon.SetRotation(targetVec);
    }

    /* ************************************************************* */
    /* For Interaction purpose */
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
                mouseStartAngle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
                moonStartSpin = moon.transform.eulerAngles;
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

                float pointNewX = r * Mathf.Cos(deltaAngle);
                float pointNewY = r * Mathf.Sin(deltaAngle);
                Vector3 position = new Vector3(pointNewX, 0, pointNewY);

                // Assign new position
                moon.Position = earth.Position + position;

                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance);
                prefabs.DrawLineEarthMoon();
            } else {
                
                Vector2 screenDisplacement = currentMousePosition - centerOfSpin;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x) * Mathf.Rad2Deg;

                moon.transform.eulerAngles = moonStartSpin + Vector3.down * (deltaAngle - mouseStartAngle);
                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMoonCenter = false;
            draggingEdgeMoon = false;
        }
    }
}
