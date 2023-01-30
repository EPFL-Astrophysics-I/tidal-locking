﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using static Units;

public class OneBodySimulation : Simulation
{
    /* ************************************************************* */

    public enum OneBodySimType { ContinuousSim, DiscreteSim, MoonBulgeOscillation, MoonSquashing };

    [Header("Simulation Properties")]
    public OneBodySimType simulationType = OneBodySimType.ContinuousSim; // Default=ContinuousSim
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    private UnitTime unitTime = UnitTime.Day;
    private UnitLength unitLength = UnitLength.EarthMoonDistanceFactor;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;
    private float timerAnimation;
    private float timerOrbitMoon;
    private float timerLerpBulgeAxis = 6;
    private float timerLerpToCI = 5;
    private float timerIntervalSteps = 1;
    private bool waitForMoonToCI = false;
    private bool angleOffsetIsCompute = false;

    /* ************************************************************* */
    // Damped harmonic parameters
    private float oscillationV;
    private float oscillationX;
    private float oscillationK = 8f;
    private float oscillationB = 5f;
    private float oscillationM = 1f;
    private bool rot180Moon = true;

    private float oscillationXInvert;

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

    public TopDownView topDownView;
    public BarOnPlot spinSpeedBar;
    public SliderSync sliderSync;

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
            prefabs.SetMoonTidalVectorActivation(value);
            if (value) {
                //prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            }
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

    private bool activationEarthOrbit = false;
    public bool ActivationEarthOrbit {
        get {
            return activationEarthOrbit;
        }
        set {
            activationEarthOrbit = value;
            prefabs.SetEarthOrbitActivation(value);
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

    private float dragEdgesRange = 0;
    public float DragEdgesRange {
        get {
            return dragEdgesRange;
        }
        set {
            dragEdgesRange = value;
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
    [HideInInspector] public bool squashingAnimation = false;
    [HideInInspector] public float radiusScale = 10;
    private bool moonSquashed = false;
    [HideInInspector] public bool MoonIsSquashed {
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
                }

                if (spinSpeedBar && spinSpeedBar.observable=="MoonPeriodFactor") {
                    //Debug.Log(value);
                    spinSpeedBar.SetPosition(value);
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
            waitForMoonToCI = value;
            if (value && moon!=null) {
                SetMoonInitialCondition();
            }
        }
    }
    public float angleMoonOrbitInit;
    public float angleMoonSpinInit;
    private float moonSpinSpeed=0;
    public float MoonSpinSpeed {
        get {
            return moonSpinSpeed;
        }
        set {
            moonSpinSpeed = value;
            if (spinSpeedBar && spinSpeedBar.observable=="MoonSpinSpeed") {
                spinSpeedBar.SetPosition(moonSpinSpeed);
            }
        }
    }

    /* ************************************************************* */
    private OneBodyPrefabs prefabs;

    /* ************ Mouse Clicks & Drag **************************** */
    private Camera mainCamera;
    public bool dragMoonIsAllowed;
    public bool dragEarthIsAllowed;
    public bool dragMoonEdgesIsAllowed;
    private bool draggingMoonCenter;
    private bool draggingEdgeMoon;
    private Vector2 centerOfRotation;
    private Vector2 centerOfSpin;
    private float moonStartAngle;
    private float mouseStartAngle;
    private Vector3 moonStartSpin;

    public bool dragRotatesMoon;
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
            waitForMoonToCI = true;
            SetMoonInitialCondition();
        }

        //prefabs.setMoonPointPosition();

        CircularOrbit moonOrbit = prefabs.moonOrbit;
        if (moonOrbit) {
            moonOrbit.DrawOrbit(initEarthPosition, LunarDistance(unitLength), 100);
        }

        CircularOrbit earthOrbit = prefabs.earthOrbit;
        if (earthOrbit) {
            earthOrbit.DrawOrbit(initMoonPosition, LunarDistance(unitLength), 100);
        }

        prefabs.SetMoonTidalVectorActivation(activationPointsOnMoon);
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        timerOrbitMoon = Period / 10;

        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;
    }

    /* ************************************************************* */
    // Update and FixedUpdate:
    private void Update()
    {
        if (simIsStationary)
        {
            if (dragEarthIsAllowed || dragMoonIsAllowed || dragMoonEdgesIsAllowed) {
                DragBody();
            }

            return;
        }
    }


    private void FixedUpdate()
    {
        if (paused)
        {
            return;
        }

        if (waitForMoonToCI) {
            // Wait until Moon
            return;
        }

        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);

        switch (simulationType)
        {
            case OneBodySimType.ContinuousSim: {
                if (simIsStationary) {return;}
                // Continuous simulation update done after the switch case.
                break;
            }
            case OneBodySimType.DiscreteSim: {
                if (simIsStationary) {return;}
                UpdateSimDiscrete();
                return;
            }
            case OneBodySimType.MoonBulgeOscillation: {
                // Simulation needs to be stationary.
                if (simIsStationary)
                {
                    UpdateSimMoonBulgeOscillation();
                }
                return;
            }
            case OneBodySimType.MoonSquashing: {
                // Moon Squashing Animation can be run even with StationarySim Bool True.
                UpdateSimMoonSquashing();
                if (simIsStationary) {return;}
                break;
            }
            default:
                break;
        }

        // SIM NOT STATIONARY:
        // Continuous Sim update:
        UpdateSimContinuous();
    }

    /* ************************************************************* */
    // Update functions for the different simulation type:

    // Moon Bulge Oscillation : 
    private void UpdateSimMoonBulgeOscillation()
    {
        // Moon Bugle get pull back toward Earth when left mouse click is unpressed:
        if (!draggingEdgeMoon) {
            float substep = timeScale * Time.fixedDeltaTime / numSubsteps;
            for (int i = 1; i <= numSubsteps; i++)
            {
                dampedHarmonicOscillation(substep);
            }
        }
    }

    private void dampedHarmonicOscillation(float deltaTime)
    {        
        float a = (-oscillationK*oscillationX - oscillationB*oscillationV)/oscillationM;
        oscillationV += a*deltaTime;
        oscillationX += oscillationV*deltaTime;
        
        float rot = oscillationX;
        moon.SetRotationSprite(new Vector3(0, oscillationXInvert-rot, 0));
        if (rot180Moon) {
            rot += 180;
        }

        moon.SetRotation(new Vector3(0, rot, 0));
        //moon.SetRotationSprite(new Vector3(0, rot, 0));

        //prefabs.setMoonPointPosition();
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
        prefabs.DrawLineMoonBulge();
    }

    // Moon Squashing Animation : 
    private void UpdateSimMoonSquashing()
    {
        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);

        if (squashingAnimation) {
            StartCoroutine(MoonTidalAction(10f));
            squashingAnimation=false;
        }
    }

    // Continuous Simulation Update (One Body Problem Step) : 
    private void UpdateSimContinuous() {
        if (resetAfterOnePeriod)
        {
            // Re-establish the system to exact initial positions after one period to avoid numerical errors
            if (resetTimer >= Period)
            {
                resetTimer = 0;
                moon.Position = initMoonPosition;
            }

            resetTimer += timeScale * Time.fixedDeltaTime;
            timerAnimation += timeScale * Time.fixedDeltaTime;
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

            if (Mathf.Abs(moonSpinSpeed)<=0.1f) {
                MoonSpinSpeed=0;
            } else {
                MoonSpinSpeed = moonSpinSpeed - 0.002f*moonSpinSpeed;
            }

            moon.IncrementRotationSprite(moonSpinSpeed*deltaAngle*Vector3.down);
            float UV2Angle = moon.getUVoffset()*360;
                        
            prefabs.SetMoonReferenceSystem(UV2Angle);

            if (sliderSync && sliderSync.sliderValueName==SliderSync.SliderValueName.MoonSpinSpeed) {
                sliderSync.updateValue(getMoonPeriod(), MoonSpinSpeed);
            }
            
            if (topDownView) {
                // vector following spin of the moon is index 1 in top down view.
                Vector3 moonPos = moon.Position;
                float theta = Mathf.Atan2(moonPos.z, moonPos.x)*Mathf.Rad2Deg;
                topDownView.SetRotationOfVector(-UV2Angle, 1);
                topDownView.SetRotationOfVector(-UV2Angle, 2);
            }
        }

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
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

        //prefabs.SetMoonRefSystem(moonSpinSpeed*0*Mathf.Rad2Deg);
        //float UV2Angle = moon.getUVoffset()*Mathf.PI*2;
        //prefabs.SetMoonRefSystem(moonSpinSpeed*UV2Angle*Mathf.Rad2Deg);
        if (topDownView) {
            // vector following orbit rate of the Moon is index 0 in top down view.
            //topDownView.SetRotationOfVector(theta*Mathf.Rad2Deg, 0);
        }
    }

    // Discrete case: 3 steps animation : 
    private void UpdateSimDiscrete()
    {
        if (timerAnimation <= timerIntervalSteps) {
            timerAnimation += timeScale * Time.fixedDeltaTime;
           return;
        }
        // Three Steps Animation for slide 5:
        if (timerAnimation <= timerIntervalSteps+timerOrbitMoon) {
            // Update of timerAnimation is done in UpdateOneBodySimulation()
            UpdateSimContinuous();
            return;
        }

        if (timerAnimation <= (timerIntervalSteps+timerOrbitMoon+(Time.fixedDeltaTime*2))) {
            if (!angleOffsetIsCompute) {
                StartCoroutine(LerpMoonRotationAlongBulge(timerLerpBulgeAxis));
                StartCoroutine(FadeInOutTidalVectors(timerLerpBulgeAxis));
                //StartCoroutine(MoonTidalAction(timerLerpBulgeAxis, 0, 1));
                angleOffsetIsCompute = true;
            }

            timerAnimation += timeScale * Time.fixedDeltaTime;
            return;
        }
            
        float periodSlowDownOffset = 1f - MoonPeriodFactor;
        if (Mathf.Abs(periodSlowDownOffset)<0.05) {
            MoonPeriodFactor=1;
        } else {
            MoonPeriodFactor = MoonPeriodFactor + 0.3f*periodSlowDownOffset;
        }
        angleOffsetIsCompute = false;
        timerAnimation = 0;

        if (sliderSync && sliderSync.sliderValueName==SliderSync.SliderValueName.MoonPeriodFactor) {
            sliderSync.updateValue(getMoonPeriod(), MoonPeriodFactor);
        }
    }

    /* ************************************************************* */
    public void ResetSimulation()
    {
        // Disable coroutine if we click reset during a LerpMoonRotationAlongBulge;
        StopAllCoroutines();

        simIsStationary=true;

        MoonPeriodFactor = 1;
        MoonSpinSpeed = 0;

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
            initMoonPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit*Mathf.Deg2Rad), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit*Mathf.Deg2Rad));
            moonDistance = (initMoonPosition - earth.Position).magnitude;

            Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);
            if (angleMoonSpinInit==404)
            {
                targetRotation = new Vector3(0, 180, 0);
            }

            moon.Position = initMoonPosition;
            moon.SetRotation(targetRotation);

            moon.SetRotationSprite(Vector3.zero);
            //float UV2Angle = moon.getUVoffset()*360;
            prefabs.SetMoonReferenceSystem(0);
        }

        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();
        //prefabs.setMoonPointPosition();
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
        initMoonPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit*Mathf.Deg2Rad), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit*Mathf.Deg2Rad));
        moonDistance = (initMoonPosition - earth.Position).magnitude;

        Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);
        if (angleMoonSpinInit==404)
        {
            targetRotation = new Vector3(0, 180, 0);
        }

        float currentAngle = Mathf.Atan2(moon.Position.z, moon.Position.x);

        StartCoroutine(LerpMoonPositionAlongOrbit(currentAngle, angleMoonOrbitInit*Mathf.Deg2Rad, timerLerpToCI));
        StartCoroutine(LerpMoonRotation(moon.transform.rotation.eulerAngles, targetRotation, timerLerpToCI-0.1f));
        moon.SetRotationSprite(Vector3.zero);
        //float UV2Angle = moon.getUVoffset()*360;
        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;
    }

    /* ************************************************************* */
    // Lerp Functions

    IEnumerator LerpMoonPositionAlongOrbit(float start, float target, float lerpTime) {
        float time = 0;
        float angleSubstep = (target-start)/lerpTime*Time.fixedDeltaTime;

        float sAngle = start;
        while (time < lerpTime) {
            time += Time.fixedDeltaTime;

            sAngle += angleSubstep;
            Vector3 posSubstep = new Vector3(moonDistance * Mathf.Cos(sAngle), 0, moonDistance * Mathf.Sin(sAngle));
            moon.Position = posSubstep;

            //prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineEarthMoon();
            prefabs.DrawLineMoonBulge();
            prefabs.SetMoonReferenceSystem(0);
            
            yield return null;
        }
        waitForMoonToCI = false;

        oscillationV = 0f;
        oscillationX = 0f;
        rot180Moon = true;
        
        prefabs.SetMoonReferenceSystem(0);
    }

    IEnumerator LerpMoonRotation(Vector3 start, Vector3 target, float lerpTime) {
        float time = 0;

        while (time < lerpTime) {
            time += Time.fixedDeltaTime;
            moon.SetRotation(Vector3.Lerp(start, target, time/lerpTime));
            //prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineMoonBulge();
            prefabs.SetMoonReferenceSystem(0);
            
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
            if (start<180) {
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
            moon.IncrementRotationSprite(new Vector3(0, -substep, 0));
            //prefabs.setMoonPointPosition();
            prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
            prefabs.DrawLineMoonBulge();
            
            yield return null;
        }
        moon.SetRotation(new Vector3(0, target, 0));
    }

    private IEnumerator MoonTidalAction(float fadeTime, float startDelay=1f, float blinkFreq=2f)
    {
        yield return new WaitForSeconds(startDelay);

        float time = 0;
        float index=1f;
        bool toggle = false;

        while (time < fadeTime)
        {
            time += Time.fixedDeltaTime;
            if (time>(blinkFreq*index)) {
                toggle = !toggle;
                index++;
                ActivationPointsOnMoon = toggle;
            }
            yield return null;
        }

        ActivationPointsOnMoon = true;
        MoonIsSquashed = true;
    }

    private IEnumerator FadeInOutTidalVectors(float fadeTime)
    {
        ActivationPointsOnMoon = true;
        float time = 0;
        while (time < fadeTime)
        {
            time += Time.fixedDeltaTime;
            yield return null;
        }
        ActivationPointsOnMoon = false;
    }

    /* ************************************************************* */
    /* For Interaction purpose */
    private void DragBody()
    {
        CelestialBody draggableBody = moon;
        CelestialBody centerBody = earth;
        if (dragEarthIsAllowed) {
            draggableBody = earth;
            centerBody = moon;
        }
        if(Input.GetMouseButtonDown(0)) {
            // Do not need z component as the camera is "looking down",
            // simulation is in the plan (X, Y)


            Vector2 bodyPositionOnScreen = mainCamera.WorldToScreenPoint(draggableBody.Position);
            Vector2 mousePositionOnClick = Input.mousePosition;

            centerOfRotation = mainCamera.WorldToScreenPoint(centerBody.Position);
            centerOfSpin = mainCamera.WorldToScreenPoint(draggableBody.Position);

            Vector2 distance = mousePositionOnClick - bodyPositionOnScreen;
            float distanceMag = distance.magnitude;

            Vector3 a = new Vector3(draggableBody.transform.localScale.x/2, draggableBody.transform.localScale.x/2, 0);
            Vector2 bodyRadius = mainCamera.WorldToScreenPoint(draggableBody.Position + a);

            Vector2 range = bodyRadius - bodyPositionOnScreen;
            float rangeMag = range.magnitude;

            float innerRangeMag = rangeMag * 0.6f;
            float outerRangeMag = rangeMag * 1.3f;

            // Check that the mouse click is in the center of the moon
            if (-innerRangeMag <= distanceMag && distanceMag <= innerRangeMag) 
            {
                //moonStartAngle = Mathf.Atan2(moon.Position.y, moon.Position.x);
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

            if (draggingMoonCenter && (dragMoonIsAllowed || dragEarthIsAllowed)) {
                Vector2 screenDisplacement = currentMousePosition - centerOfRotation;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x);
                
                // Compute new position
                Vector3 vectorR = draggableBody.Position - centerBody.Position;
                float r = vectorR.magnitude;

                float pointNewX = r * Mathf.Cos(deltaAngle);
                float pointNewY = r * Mathf.Sin(deltaAngle);
                Vector3 position = new Vector3(pointNewX, 0, pointNewY);

                Vector3 oldMoonPos = draggableBody.Position;

                /*
                if (pointNewX>oldMoonPos.x) {
                    // can't drag the moon in the opposite direction of its orbit rotation.
                    return;
                }
                */

                // Assign new position
                draggableBody.Position = centerBody.Position + position;

                Vector3 diff = draggableBody.Position-oldMoonPos;
                float diffMag = diff.magnitude;
                //Debug.Log(angularMoonSpeed*Time.deltaTime);

                if (dragRotatesMoon && diffMag!=0f) {
                    //moon.SetRotationSprite((deltaAngle * Mathf.Rad2Deg) * Vector3.down);
                    //moon.SetRotationSprite(moonSpinSpeed*deltaAngle*Vector3.down);
                    moon.SetRotation((deltaAngle * Mathf.Rad2Deg+180) * Vector3.down);
                    moon.IncrementRotationSprite(moonSpinSpeed*deltaAngle*Vector3.down);
                    float UV2Angle = moon.getUVoffset()*360;
                    
                    prefabs.SetMoonReferenceSystem(UV2Angle);
                    /*
                    if (moonSpinSpeed==-2f) {
                        moon.SetRotationSprite(new Vector3(0, deltaAngle* Mathf.Rad2Deg, 0));
                    } else {
                        moon.IncrementRotationSprite(moonSpinSpeed*deltaAngle * Vector3.down);
                    }*/
                }
                if (dragEarthIsAllowed) {
                    moon.SetRotation((deltaAngle * Mathf.Rad2Deg) * Vector3.down);
                    moon.SetRotationSprite(new Vector3(0, deltaAngle * Mathf.Rad2Deg+180, 0));
                }

                //prefabs.setMoonPointPosition();
                prefabs.setGravitationalVectors(NewtonG, moonDistance, vectorGravScale, vectorTidalScale);
                prefabs.DrawLineEarthMoon();
                prefabs.DrawLineMoonBulge();
            } 
            else if(draggingEdgeMoon && dragMoonEdgesIsAllowed) {
                
                Vector2 screenDisplacement = currentMousePosition - centerOfSpin;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x) * Mathf.Rad2Deg;

                if(DragEdgesRange!=0) {
                    Debug.Log(DragEdgesRange);
                    if(DragEdgesRange*Mathf.Deg2Rad > deltaAngle || deltaAngle > DragEdgesRange*Mathf.Deg2Rad) {
                        return;
                    }
                }

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

                //moon.SetRotationSprite(new Vector3(0, newEulerY, 0));

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

                oscillationXInvert = oscillationX + moon.getUVoffset()*360;

                //prefabs.setMoonPointPosition();
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
}
