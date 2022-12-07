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
    private UnitLength unitLength = UnitLength.EarthMoonDistanceFactor;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;
    public bool IsAnimationThreeSteps;
    private float timerAnimation;
    private float timerOrbitMoon;
    private float timerLerpBulgeAxis = 5;
    private float timerLerpToCI = 5;
    private float timerIntervalSteps = 1;
    private bool waitForMoonToCI = false;
    private bool angleOffsetIsCompute = false;

    /* ************************************************************* */
    // Damped harmonic parameters
    private float oscillationV;
    private float oscillationX;
    private float oscillationK = 8f;
    private float oscillationB = 1f;
    private float oscillationM = 1f;
    private bool rot180Moon = true;
    public bool oscillationMoonRotation = false;

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
    private float vectorGravScale = 400f;
    public float VectorGravScale {
        get {
            return vectorGravScale;
        }
        set {
            vectorGravScale = value;
        }
    }
    private float vectorTidalScale = 500f;
    public float VectorTidalScale {
        get {
            return vectorTidalScale;
        }
        set {
            vectorTidalScale = value;
        }
    }

    private float vectorGravLineWidth;
    public float VectorGravLineWidth {
        get {
            return vectorGravLineWidth;
        }
        set {
            vectorGravLineWidth = value;
            if (prefabs) {
                prefabs.SetGravVecLineWidth(vectorGravLineWidth);
            }
        }
    }
    
    private float vectorTidalLineWidth;
    public float VectorTidalLineWidth {
        get {
            return vectorTidalLineWidth;
        }
        set {
            vectorTidalLineWidth = value;
            if (prefabs) {
                prefabs.SetTidalVecLineWidth(vectorTidalLineWidth);
            }
        }
    }

    /* ************************************************************* */

    private bool activationPointsOnMoon = false;
    public bool ActivationPointsOnMoon {
        get {
            return activationPointsOnMoon;
        }
        set {
            activationPointsOnMoon = value;
            prefabs.SetPointsOnMoonActivation(value);
            if (value) {
                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            }
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

    private bool activationMoonOrbit = false;
    public bool ActivationMoonOrbit {
        get {
            return activationMoonOrbit;
        }
        set {
            activationMoonOrbit = value;
            prefabs.SetMoonOrbitActivation(value);
        }
    }

    private bool activationMoonBulgeLine = false;
    public bool ActivationMoonBulgeLine {
        get {
            return activationMoonBulgeLine;
        }
        set {
            activationMoonBulgeLine = value;
            prefabs.SetMoonBulgeLineActivation(value);
        }
    }

    private bool activationMoonRefSystem = false;
    public bool ActivationMoonRefSystem {
        get {
            return activationMoonRefSystem;
        }
        set {
            activationMoonRefSystem = value;
            prefabs.SetMoonRefSystemActivation(value);
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
    public bool squashingAnimation = false;
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
            if (moon!=null) {
                moon.RotationPeriod = Period * value;
                if (value==1 && value!=moonPeriodFactor) {
                    // Reset rotation of the moon
                    // Keep the same face toward the earth,
                    // Otherwise from moonPeriodFactor != 1 to moonPeriodFactor = 1
                    // The face of the moon will not be the same.
                    moon.transform.rotation = Quaternion.Euler(0, 180, 0);
                    float deltaAngle = timeScale * resetTimer * 360 / moon.RotationPeriod;
                    moon.IncrementRotation(deltaAngle * Vector3.down);
                    Debug.Log(value);
                }
            }
            moonPeriodFactor = value;
        }
    }

    public float getMoonPeriod() {
        if (moon) {
            return moon.RotationPeriod;
        }
        return 27.5f;
    }

    private bool useMoonCI;
    public bool UseMoonCI {
        get {
            return useMoonCI;
        }
        set {
            useMoonCI = value;
            if (value && moon!=null) {
                waitForMoonToCI = true;
                SetMoonInitialCondition();
            }
        }
    }
    public float angleMoonOrbitInit;
    public float angleMoonSpinInit;

    /* ************************************************************* */
    private OneBodyPrefabs prefabs;

    /* ************ Mouse Clicks & Drag **************************** */
    private Camera mainCamera;
    public bool dragMoonIsAllowed;
    public bool dragMoonEdgesIsAllowed;
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

        prefabs.SetGravVecLineWidth(vectorGravLineWidth);
        //prefabs.SetTidalVecLineWidth(vectorTidalLineWidth);

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
            waitForMoonToCI = true;
            SetMoonInitialCondition();
        }

        prefabs.setMoonPointPosition();

        CircularOrbit moonOrbit = prefabs.moonOrbit;
        if (moonOrbit) {
            moonOrbit.DrawOrbit(initEarthPosition, LunarDistance(unitLength), 100);
        }

        prefabs.SetPointsOnMoonActivation(activationPointsOnMoon);
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        timerOrbitMoon = Period / 10;

        oscillationV = 0f;
        oscillationX = 0f;
    }

    /* ************************************************************* */
    private void Update()
    {
        if (simIsStationary)
        {
            if (dragMoonIsAllowed || dragMoonEdgesIsAllowed)
                DragMoonAlongOrbit();
                MouseOverMoon();
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
            if (waitForMoonToCI) {
                // Wait until Moon
                return;
            }
            /*
            if (stationaryTimer<0.1f) {
                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance);
                stationaryTimer+=Time.fixedDeltaTime;
            }*/

            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);

            if (squashingAnimation) {
                StartCoroutine(MoonTidalAction(10f, 1f));
                squashingAnimation=false;
                return;
            }

            if (oscillationMoonRotation) {
                //x = moon.transform.eulerAngles.y - 180; 
                // Solve the equation of motion
                //Debug.Log(x);
                float substep = timeScale * Time.fixedDeltaTime / numSubsteps;
                for (int i = 1; i <= numSubsteps; i++)
                {
                    dampedHarmonicOscillation(substep);
                    //setGravitationalVectors();
                    //setMoonPointPosition();
                }
            }

            return;
        }

        if (IsAnimationThreeSteps)
        {
            if (waitForMoonToCI) {
                // Wait until Moon
                return;
            }

            if (timerAnimation <= timerIntervalSteps) {
                timerAnimation += timeScale * Time.fixedDeltaTime;
                return;
            }
            // Three Steps Animation for slide 5:
            if (timerAnimation <= timerIntervalSteps+timerOrbitMoon) {
                // Update of timerAnimation is done in UpdateOneBodySimulation()
                UpdateOneBodySimulation();
                return;
            }

            if (timerAnimation <= (timerIntervalSteps+timerOrbitMoon+(Time.fixedDeltaTime*2))) {
                if (!angleOffsetIsCompute) {
                    StartCoroutine(LerpMoonRotationAlongBulge(timerLerpBulgeAxis));
                    angleOffsetIsCompute = true;
                }
                /*
                float substep = angleOffset / (timeStep2Animation * Time.fi);
                Debug.Log(substep);
                moon.IncrementRotation(substep * Vector3.down);

                timerAnimation += Time.fixedDeltaTime;*/
                timerAnimation += timeScale * Time.fixedDeltaTime;
                return;
            }
            float periodSlowDownOffset = 1f - MoonPeriodFactor;
            MoonPeriodFactor = MoonPeriodFactor + 0.1f*periodSlowDownOffset;
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
                if (!IsAnimationThreeSteps) {
                    moon.Position = initMoonPosition;
                }
            }

            resetTimer += timeScale * Time.fixedDeltaTime;
            timerAnimation += timeScale * Time.fixedDeltaTime;
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
        prefabs.DrawLineMoonBulge();
        prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
        prefabs.SetMoonRefSystem();
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

        prefabs.SetMoonRefSystem();
    }
    private void dampedHarmonicOscillation(float deltaTime)
    {        
        float a = (-oscillationK*oscillationX - oscillationB*oscillationV)/oscillationM;
        oscillationV += a*deltaTime;
        oscillationX += oscillationV*deltaTime;
        
        float rot = oscillationX;
        if (rot180Moon) {
            rot += 180;
        }
        moon.SetRotation(new Vector3(0, rot, 0));
        prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
        prefabs.DrawLineMoonBulge();
    }

    /* ************************************************************* */
    public void ResetSimulation()
    {
        // Disable coroutine if we click reset during a LerpMoonRotationAlongBulge;
        StopAllCoroutines();
        MoonPeriodFactor = 1;

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
        prefabs.DrawLineMoonBulge();
        prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
    }

    public void ResetSquashingAnim() {
        StopAllCoroutines();

        ActivationPointsOnMoon = false;
        MoonIsSquashed=false;
        squashingAnimation = true;
    }
    /* ************************************************************* */
    private void SetMoonInitialCondition() {
        resetTimer = 0;
        initMoonPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit));
        moonDistance = (initMoonPosition - earth.Position).magnitude;

        Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);
        float currentAngle = Mathf.Atan2(moon.Position.z, moon.Position.x);

        StartCoroutine(LerpMoonPositionAlongOrbit(currentAngle, angleMoonOrbitInit, timerLerpToCI));
        StartCoroutine(LerpMoonRotation(moon.transform.rotation.eulerAngles, targetRotation, timerLerpToCI-0.1f));
    }

    /* ************************************************************* */
    IEnumerator LerpMoonPositionAlongOrbit(float start, float target, float lerpTime) {
        float time = 0;
        float angleSubstep = (target-start)/lerpTime*Time.fixedDeltaTime;

        float sAngle = start;
        while (time < lerpTime) {
            time += Time.fixedDeltaTime;

            sAngle += angleSubstep;
            Vector3 posSubstep = new Vector3(moonDistance * Mathf.Cos(sAngle), 0, moonDistance * Mathf.Sin(sAngle));
            moon.Position = posSubstep;

            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineEarthMoon();
            prefabs.DrawLineMoonBulge();
            
            yield return null;
        }
        waitForMoonToCI = false;
        if (oscillationMoonRotation) {
            oscillationV = 0f;
            oscillationX = 0f;
            rot180Moon = true;
        }
    }

    IEnumerator LerpMoonRotation(Vector3 start, Vector3 target, float lerpTime) {
        float time = 0;

        while (time < lerpTime) {
            time += Time.fixedDeltaTime;
            moon.SetRotation(Vector3.Lerp(start, target, time/lerpTime));
            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineMoonBulge();
            
            yield return null;
        }
    }

    IEnumerator LerpMoonRotationAlongBulge(float lerpTime) {
        float time = 0;

        float deltaAngle = (timeScale * resetTimer * 360 / Period);
        float target = 180 - deltaAngle;
        float start = moon.transform.eulerAngles.y;

        float substep;
        if (moonPeriodFactor==1) {
            substep=0;
        }
        else if (moonPeriodFactor>1) {
            if (target<start && start<180) {
                substep = (target-start)/lerpTime*Time.fixedDeltaTime;
            } else {
                substep = (360-start) + target;
                substep = substep/lerpTime*Time.fixedDeltaTime;
            }
        } else {
            if (target<start) {
                substep = (360-start) + target;
                substep = substep/lerpTime*Time.fixedDeltaTime;
            } else {
                substep = (target-start)/lerpTime*Time.fixedDeltaTime;
            }
        }

        //float step = start;
        while (time < lerpTime) {
            time += Time.fixedDeltaTime;
            //step += substep;
            //moon.SetRotation(new Vector3(0, step, 0));
            moon.IncrementRotation(new Vector3(0, substep, 0));
            prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineMoonBulge();
            
            yield return null;
        }
        moon.SetRotation(new Vector3(0, target, 0));
    }

    private IEnumerator MoonTidalAction(float fadeTime, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        float time = 0;
        float nblink = 2f;
        float index=1f;
        bool toggle = false;

        while (time < fadeTime)
        {
            time += Time.fixedDeltaTime;
            if (time>(nblink*index)) {
                toggle = !toggle;
                index++;
                ActivationPointsOnMoon = toggle;
            }
            yield return null;
        }

        ActivationPointsOnMoon = true;
        MoonIsSquashed = true;
    }

    /* ************************************************************* */
    /* For Interaction purpose */
    private void DragMoonAlongOrbit()
    {
        if(Input.GetMouseButtonDown(0)) {
            // Do not need z component as the camera is "looking down",
            // simulation is in the plan (X, Y)
            Vector2 moonPositionInScreen = mainCamera.WorldToScreenPoint(moon.Position);
            Vector2 mousePositionOnClick = Input.mousePosition;

            centerOfRotation = mainCamera.WorldToScreenPoint(earth.Position);
            centerOfSpin = mainCamera.WorldToScreenPoint(moon.Position);

            Vector2 distance = mousePositionOnClick - moonPositionInScreen;
            float distanceMag = distance.magnitude;

            Vector3 a = new Vector3(moon.transform.localScale.x/2, moon.transform.localScale.x/2, 0);
            Vector2 moonRadius = mainCamera.WorldToScreenPoint(moon.Position + a);

            Vector2 range = moonRadius - moonPositionInScreen;
            float rangeMag = range.magnitude;

            float innerRangeMag = rangeMag * 0.6f;
            float outerRangeMag = rangeMag * 1.3f;

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

            if (draggingMoonCenter && dragMoonIsAllowed) {
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
                prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
                prefabs.DrawLineEarthMoon();
                prefabs.DrawLineMoonBulge();
            } 
            else if(draggingEdgeMoon && dragMoonEdgesIsAllowed) {
                
                Vector2 screenDisplacement = currentMousePosition - centerOfSpin;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x) * Mathf.Rad2Deg;

                Vector3 newEuler = moonStartSpin + Vector3.down * (deltaAngle - mouseStartAngle);

                // Add 360, then mod 360:
                // otherwise you might have negative euler angle if you click on the moon 
                // while it has 180deg rotation or more: 
                // 150 -> 179.9 -> 180 -> -179.9 -> -150
                // and not: 
                // 150 -> 179.9 -> 180 -> 180.1 -> 210
                float newEulerY = (newEuler.y+360) % 360;
                newEuler.y = newEulerY;

                moon.transform.eulerAngles = new Vector3(0, newEulerY, 0);

                oscillationX = newEulerY;
                //Debug.Log(newEulerY);
                if (90 <= newEulerY && newEulerY <= 270) {
                    rot180Moon = true;
                    oscillationX -= 180;
                } else {
                    rot180Moon = false;
                    if (newEulerY>270) {
                        oscillationX = newEulerY-360;
                    }
                }                

                prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
                prefabs.DrawLineMoonBulge();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMoonCenter = false;
            draggingEdgeMoon = false;
        }
    }

    private void MouseOverMoon() {
        Vector2 mousePosition = Input.mousePosition;
        Vector2 moonCenter = mainCamera.WorldToScreenPoint(moon.Position);

        Vector2 lineMouseMoon = mousePosition - moonCenter;
        float mouseAngle = Mathf.Atan2(lineMouseMoon.y, lineMouseMoon.x) * Mathf.Rad2Deg;
        float moonAngle = moon.transform.eulerAngles.y;
        Debug.Log(mouseAngle + " " + moonAngle);
        float totalAngle= (mouseAngle+moonAngle)*Mathf.Deg2Rad;

        float cosAngle = Mathf.Cos(totalAngle);
        float sinAngle = Mathf.Sin(totalAngle);
        
        float semiMajorAxis = moon.transform.localScale.x/2;
        float semiMinorAxis = moon.transform.localScale.z/2;

        float radiusMoon = (semiMajorAxis*semiMinorAxis)/Mathf.Sqrt(semiMajorAxis*semiMajorAxis*sinAngle*sinAngle+semiMinorAxis*semiMinorAxis*cosAngle*cosAngle);

        Vector3 pointOnMoon = moon.Position + (radiusMoon)*(Quaternion.AngleAxis(-mouseAngle, Vector3.up)*Vector3.right);
        prefabs.setMoonVecOnMouse(pointOnMoon, NewtonG, moonDistance, vectorGravScale);
        //if (radiusMoon<1)
        //    Debug.Log(radiusMoon);
            //prefabs.setMoonPointOnMouse(mainCamera.ScreenToWorldPoint(mousePosition3));
    }
}
