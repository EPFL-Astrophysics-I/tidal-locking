using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using static Units;

public class TidalLockingSimulation : Simulation
{
    /* ************************************************************* */
    // Prefabs holder:
    private TidalLockingPrefabs prefabs;

    /* ************************************************************* */
    // Main Parameters:
    public enum SimulationType { ContinuousSim, DiscreteSim, MoonBulgeOscillation, MoonSquashing };

    [Header("Simulation Properties")]
    [HideInInspector] public bool simIsStationary = false;
    [HideInInspector] public SimulationType simulationType = SimulationType.ContinuousSim; // Default=ContinuousSim
    [HideInInspector] public bool squashingAnimation = false;
    public int numSubsteps = 100;
    public bool resetAfterOnePeriod = true;
    private UnitTime unitTime = UnitTime.Day;
    private UnitLength unitLength = UnitLength.EarthMoonDistanceFactor;
    private UnitMass unitMass = UnitMass.EarthMass;
    public float timeScale = 1;
    private float bodyRadiusScale = 10;
    public float BodyRadiusScale
    {
        get
        {
            return bodyRadiusScale;
        }
        set
        {
            if (bodyRadiusScale != value)
            {
                bodyRadiusScale = value;
                if (earth)
                {
                    earth.SetRadius(bodyRadiusScale * EarthRadius(unitLength));
                }
                if (moon)
                {
                    moon.SetRadius(bodyRadiusScale * LunarRadius(unitLength));
                }
            }
        }
    }

    // Timer for resetting the simulation after one orbital period
    private float resetTimer;
    // Gravitational constant
    private float _newtonG;
    public float NewtonG => (_newtonG != 0) ? _newtonG : Units.NewtonG(unitTime, unitLength, unitMass);
    // Orbital period
    public float Period => 2 * Mathf.PI * Mathf.Sqrt(Mathf.Pow(moonDistance, 3) / NewtonG / Units.EarthMass(unitMass));

    // Discrete Simulation Parameters:
    private float timerDiscreteSim;
    private float timerOrbitMoon;
    private float timerIntervalSteps = 1;
    private bool angleOffsetIsCompute = false;

    // Lerp Timer:
    private float timerLerpBulgeAxis = 6;
    private float timerLerpToCI = 5;

    // Initial Condition:
    private bool resetEarthPos;
    public bool ResetEarthPos
    {
        get
        {
            return resetEarthPos;
        }
        set
        {
            resetEarthPos = value;
            if (value && earth != null)
            {
                earth.Position = initEarthPosition;
            }
        }
    }
    private bool waitForMoonToCI = false;
    private bool useMoonCI;
    public bool UseMoonCI
    {
        get
        {
            return useMoonCI;
        }
        set
        {
            useMoonCI = value;
            waitForMoonToCI = value;
            if (value && moon != null)
            {
                SetMoonInitialCondition();
            }
        }
    }
    [HideInInspector] public float angleMoonOrbitInit;
    [HideInInspector] public float angleMoonSpinInit;

    /* ************************************************************* */

    [Header("Earth Parameters")]
    private CelestialBody earth;
    public bool earthIsRotating = false;
    private Vector3 initEarthPosition = Vector3.zero;

    /* ************************************************************* */

    [Header("Moon Parameters")]
    private CelestialBody moon;
    public bool moonIsRotating = true;
    private Vector3 initMoonPosition;
    private float moonDistance;
    private bool moonSquashed = false;
    [HideInInspector]
    public bool MoonIsSquashed
    {
        get
        {
            if (moon != null)
                return moon.IsSquashed;
            else
                return moonSquashed;
        }
        set
        {
            moonSquashed = value;
            if (moon != null)
            {
                moon.IsSquashed = value;
            }
        }
    }
    private float moonPeriodFactor;
    public float MoonPeriodFactor
    {
        get { return moonPeriodFactor; }
        set
        {
            if (moon != null)
            {
                moon.RotationPeriod = Period * value;
                if (value == 1 && value != moonPeriodFactor)
                {
                    // Reset rotation of the moon
                    // Keep the same face toward the earth,
                    // Otherwise from moonPeriodFactor != 1 to moonPeriodFactor = 1
                    // The face of the moon will not be the same.
                    moon.transform.rotation = Quaternion.Euler(0, 180, 0);
                    float deltaAngle = timeScale * resetTimer * 360 / moon.RotationPeriod;
                    moon.IncrementRotation(deltaAngle * Vector3.down);
                }
            }
            moonPeriodFactor = value;
        }
    }

    public float getMoonPeriod()
    {
        if (moon)
        {
            return moon.RotationPeriod;
        }
        return 27.5f;
    }

    private float moonSpinSpeed = 0;
    public float MoonSpinSpeed
    {
        get
        {
            return moonSpinSpeed;
        }
        set
        {
            moonSpinSpeed = value;
        }
    }

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
    private float vectorTidalScale = 500f;
    public float VectorTidalScale
    {
        get
        {
            return vectorTidalScale;
        }
        set
        {
            vectorTidalScale = value;
        }
    }

    private float vectorTidalLineWidth;
    public float VectorTidalLineWidth
    {
        get
        {
            return vectorTidalLineWidth;
        }
        set
        {
            vectorTidalLineWidth = value;
            if (prefabs)
            {
                prefabs.SetTidalVecLineWidth(vectorTidalLineWidth);
            }
        }
    }

    /* ************************************************************* */
    // Activation properties: used for display parameters
    private bool activationMoonEarthLine = false;
    public bool ActivationMoonEarthLine
    {
        get
        {
            return activationMoonEarthLine;
        }
        set
        {
            activationMoonEarthLine = value;
            prefabs.SetLineEarthMoonActivation(value);
            if (value)
            {
                prefabs.DrawLineEarthMoon();
            }
        }
    }

    private bool activationTidalVectors = false;
    public bool ActivationTidalVectors
    {
        get
        {
            return activationTidalVectors;
        }
        set
        {
            activationTidalVectors = value;
            prefabs.SetMoonTidalVectorActivation(value);
            if (value)
            {
                prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
            }
        }
    }

    private bool activationMoonOrbit = false;
    public bool ActivationMoonOrbit
    {
        get
        {
            return activationMoonOrbit;
        }
        set
        {
            activationMoonOrbit = value;
            prefabs.SetMoonOrbitActivation(value);
        }
    }

    private bool activationEarthOrbit = false;
    public bool ActivationEarthOrbit
    {
        get
        {
            return activationEarthOrbit;
        }
        set
        {
            activationEarthOrbit = value;
            prefabs.SetEarthOrbitActivation(value);
        }
    }
    private bool activationMoonBulgeLine = false;
    public bool ActivationMoonBulgeLine
    {
        get
        {
            return activationMoonBulgeLine;
        }
        set
        {
            activationMoonBulgeLine = value;
            prefabs.SetMoonBulgeLineActivation(value);
        }
    }

    private bool activationMoonRefSystem = false;
    public bool ActivationMoonRefSystem
    {
        get
        {
            return activationMoonRefSystem;
        }
        set
        {
            activationMoonRefSystem = value;
            prefabs.SetMoonRefSystemActivation(value);
        }
    }

    /* ************************************************************* */
    // Drag/Interactivity Parameters
    public enum DragBodyName { None, Earth, Moon };
    [HideInInspector] public DragBodyName dragBodyName = DragBodyName.None; // Default: no draggable body;
    [HideInInspector] public bool dragMoonEdgesIsAllowed;
    [HideInInspector] public float draggableEdgesAngleRange = 0;
    private Camera mainCamera;

    // Flag user curently drag Body/Edges:
    private bool draggingMoonCenter;
    private bool draggingEdgeMoon;

    // Body draggable:
    private Vector2 centerOfRotation;
    private Vector2 centerOfSpin;
    private float mouseStartAngle;
    private Vector3 moonStartSpin;

    /* ************************************************************* */
    [HideInInspector] public TopDownView topDownView;
    [HideInInspector] public SliderSync sliderSync;

    /* ************************************************************* */

    private void Awake()
    {
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
        resetTimer = 0;
        timerDiscreteSim = 0;

        earth = prefabs.earth;
        if (earth)
        {
            earth.Position = initEarthPosition;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(bodyRadiusScale * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        moon = prefabs.moon;
        if (moon)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(bodyRadiusScale * LunarRadius(unitLength));
            initMoonPosition = moon.Position;
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.RotationPeriod = Period * moonPeriodFactor;
            MoonIsSquashed = moonSquashed;
        }

        // USE CI :
        if (useMoonCI)
        {
            waitForMoonToCI = true;
            SetMoonInitialCondition();
        }

        CircularOrbit moonOrbit = prefabs.moonOrbit;
        if (moonOrbit)
        {
            moonOrbit.DrawOrbit(initEarthPosition, LunarDistance(unitLength), 100);
        }

        CircularOrbit earthOrbit = prefabs.earthOrbit;
        if (earthOrbit)
        {
            earthOrbit.DrawOrbit(initMoonPosition, LunarDistance(unitLength), 100);
        }

        prefabs.SetMoonTidalVectorActivation(ActivationTidalVectors);
        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        timerOrbitMoon = Period / 10;

        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;

        SetBodyMouseCursor();
    }

    /* ************************************************************* */
    // Update and FixedUpdate:
    private void Update()
    {
        if (simIsStationary)
        {
            if (((dragBodyName != DragBodyName.None) || dragMoonEdgesIsAllowed) && !waitForMoonToCI)
            {
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

        if (waitForMoonToCI)
        {
            // Wait until Moon
            return;
        }

        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);

        switch (simulationType)
        {
            case SimulationType.ContinuousSim:
                {
                    if (simIsStationary) { return; }
                    // Continuous simulation update done after the switch case.
                    break;
                }
            case SimulationType.DiscreteSim:
                {
                    if (simIsStationary) { return; }
                    UpdateSimDiscrete();
                    return;
                }
            case SimulationType.MoonBulgeOscillation:
                {
                    // Simulation needs to be stationary.
                    if (simIsStationary)
                    {
                        UpdateSimMoonBulgeOscillation();
                    }
                    return;
                }
            case SimulationType.MoonSquashing:
                {
                    // Moon Squashing Animation can be run even with StationarySim Bool True.
                    UpdateSimMoonSquashing();
                    if (simIsStationary) { return; }
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
        if (!draggingEdgeMoon)
        {
            float substep = timeScale * Time.fixedDeltaTime / numSubsteps;
            for (int i = 1; i <= numSubsteps; i++)
            {
                dampedHarmonicOscillation(substep);
            }
        }
    }

    private void dampedHarmonicOscillation(float deltaTime)
    {
        float a = (-oscillationK * oscillationX - oscillationB * oscillationV) / oscillationM;
        oscillationV += a * deltaTime;
        oscillationX += oscillationV * deltaTime;

        float rot = oscillationX;
        moon.SetRotationSprite(new Vector3(0, oscillationXInvert - rot, 0));
        if (rot180Moon)
        {
            rot += 180;
        }

        moon.SetRotation(new Vector3(0, rot, 0));

        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
        prefabs.DrawLineMoonBulge();
    }

    // Moon Squashing Animation : 
    private void UpdateSimMoonSquashing()
    {
        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);

        if (squashingAnimation)
        {
            StartCoroutine(MoonTidalAction(10f));
            squashingAnimation = false;
        }
    }

    // Continuous Simulation Update (One Body Problem Step) : 
    private void UpdateSimContinuous()
    {
        if (resetAfterOnePeriod)
        {
            // Re-establish the system to exact initial positions after one period to avoid numerical errors
            if (resetTimer >= Period)
            {
                resetTimer = 0;
                moon.Position = initMoonPosition;
            }

            resetTimer += timeScale * Time.fixedDeltaTime;
            timerDiscreteSim += timeScale * Time.fixedDeltaTime;
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

            // Sync moon spin speed:
            // Equivalent to sync the moon period factor in the UpdateSimDiscrete,
            // But we want to change the sprite UV offset!
            if (sliderSync && sliderSync.sliderValueName == SliderSync.SliderValueName.MoonSpinSpeed)
            {
                // Sync moon spin speed from slider range:
                float currentSliderValue = sliderSync.sim2slider(MoonSpinSpeed);
                float syncValueOffset = sliderSync.syncValue - currentSliderValue;

                float newSliderValue;
                if (Mathf.Abs(syncValueOffset) < 0.05)
                {
                    // Arrived at the sync value:
                    MoonSpinSpeed = 0; // should be == to slider2sim(sliderSync.syncValue)
                    newSliderValue = sliderSync.syncValue;
                }
                else
                {
                    // Step toward sync value:
                    newSliderValue = currentSliderValue + 0.002f * syncValueOffset;
                }

                sliderSync.updateValue(getMoonPeriod(), newSliderValue);
            }
            else
            {
                // Sync moon period factor:
                if (Mathf.Abs(moonSpinSpeed) <= 0.1f)
                {
                    MoonSpinSpeed = 0;
                }
                else
                {
                    MoonSpinSpeed = moonSpinSpeed - 0.002f * moonSpinSpeed;
                }
            }

            moon.IncrementRotationSprite(moonSpinSpeed * deltaAngle * Vector3.down);
            float UV2Angle = moon.getUVoffset() * 360;

            prefabs.SetMoonReferenceSystem(UV2Angle);

            if (topDownView)
            {
                // vector following spin of the moon is index 1 in top down view.
                Vector3 moonPos = moon.Position;
                float theta = Mathf.Atan2(moonPos.z, moonPos.x) * Mathf.Rad2Deg;
                topDownView.SetRotationOfVector(-UV2Angle, 1);
                topDownView.SetRotationOfVector(-UV2Angle, 2);
            }
        }

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
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

    // Discrete case: 3 steps animation : 
    private void UpdateSimDiscrete()
    {
        // Interval timer: otherwise to fast for the user experience.
        if (timerDiscreteSim <= timerIntervalSteps)
        {
            timerDiscreteSim += timeScale * Time.fixedDeltaTime;
            return;
        }

        // Timer for letting the moon orbits around the earth.
        if (timerDiscreteSim <= timerIntervalSteps + timerOrbitMoon)
        {
            // Update of timerDiscreteSim is done in UpdateSimContinuous()
            UpdateSimContinuous();
            return;
        }

        // Timer to align the moon's bugle toward the earth.
        if (timerDiscreteSim <= (timerIntervalSteps + timerOrbitMoon + (Time.fixedDeltaTime * 2)))
        {
            if (!angleOffsetIsCompute)
            {
                StartCoroutine(LerpMoonRotationAlongBulge(timerLerpBulgeAxis));
                StartCoroutine(FadeInOutTidalVectors(timerLerpBulgeAxis));
                angleOffsetIsCompute = true;
            }

            timerDiscreteSim += timeScale * Time.fixedDeltaTime;
            return;
        }

        if (sliderSync && sliderSync.sliderValueName == SliderSync.SliderValueName.MoonPeriodFactor)
        {
            // Sync moon period factor from slider range:
            float currentSliderValue = sliderSync.sim2slider(MoonPeriodFactor);
            float syncValueOffset = sliderSync.syncValue - currentSliderValue;
            float newSliderValue;
            if (Mathf.Abs(syncValueOffset) < 0.05)
            {
                // Arrived at the sync value:
                MoonPeriodFactor = 1; // should be == to slider2sim(sliderSync.syncValue)
                newSliderValue = sliderSync.syncValue;
            }
            else
            {
                // Step toward sync value:
                newSliderValue = currentSliderValue + 0.3f * syncValueOffset;
            }

            sliderSync.updateValue(getMoonPeriod(), newSliderValue);
        }
        else
        {
            // Sync moon period factor:
            float periodSlowDownOffset = 1f - MoonPeriodFactor;
            if (Mathf.Abs(periodSlowDownOffset) < 0.05)
            {
                MoonPeriodFactor = 1;
            }
            else
            {
                MoonPeriodFactor = MoonPeriodFactor + 0.3f * periodSlowDownOffset;
            }
        }

        angleOffsetIsCompute = false;
        timerDiscreteSim = 0;
    }

    /* ************************************************************* */
    public void ResetSimulation()
    {
        // Disable coroutine if we click reset during a LerpMoonRotationAlongBulge;
        StopAllCoroutines();

        simIsStationary = true;

        MoonPeriodFactor = 1;
        MoonSpinSpeed = 0;

        resetTimer = 0;
        timerDiscreteSim = 0;
        angleOffsetIsCompute = false;

        if (earth)
        {
            earth.Position = initEarthPosition;
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
        }

        if (moon)
        {
            initMoonPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit * Mathf.Deg2Rad), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit * Mathf.Deg2Rad));
            moonDistance = (initMoonPosition - earth.Position).magnitude;

            Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);

            moon.Position = initMoonPosition;
            moon.SetRotation(targetRotation);

            moon.SetRotationSprite(Vector3.zero);

            prefabs.SetMoonReferenceSystem(0);
        }

        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;

        prefabs.DrawLineEarthMoon();
        prefabs.DrawLineMoonBulge();

        prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
    }

    public void ResetSquashingAnim()
    {
        StopAllCoroutines();

        ActivationTidalVectors = false;
        MoonIsSquashed = false;
        squashingAnimation = true;
    }
    /* ************************************************************* */
    private void SetMoonInitialCondition()
    {
        resetTimer = 0;
        initMoonPosition = new Vector3(moonDistance * Mathf.Cos(angleMoonOrbitInit * Mathf.Deg2Rad), 0, moonDistance * Mathf.Sin(angleMoonOrbitInit * Mathf.Deg2Rad));
        moonDistance = (initMoonPosition - earth.Position).magnitude;

        Vector3 targetRotation = new Vector3(0, angleMoonSpinInit, 0);
        if (angleMoonSpinInit == 404)
        {
            targetRotation = new Vector3(0, 180, 0);
        }

        float currentAngle = Mathf.Atan2(moon.Position.z, moon.Position.x);

        StartCoroutine(LerpMoonPositionAlongOrbit(currentAngle, angleMoonOrbitInit * Mathf.Deg2Rad, timerLerpToCI));
        StartCoroutine(LerpMoonRotation(moon.transform.rotation.eulerAngles, targetRotation, timerLerpToCI - 0.1f));
        moon.SetRotationSprite(Vector3.zero);

        oscillationV = 0f;
        oscillationX = 0f;

        oscillationXInvert = 0f;
    }

    /* ************************************************************* */
    // Lerp Functions

    IEnumerator LerpMoonPositionAlongOrbit(float start, float target, float lerpTime)
    {
        float time = 0;
        float angleSubstep = (target - start) / lerpTime * Time.fixedDeltaTime;

        float sAngle = start;
        while (time < lerpTime)
        {
            time += Time.fixedDeltaTime;

            sAngle += angleSubstep;
            Vector3 posSubstep = new Vector3(moonDistance * Mathf.Cos(sAngle), 0, moonDistance * Mathf.Sin(sAngle));
            moon.Position = posSubstep;

            prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
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

    IEnumerator LerpMoonRotation(Vector3 start, Vector3 target, float lerpTime)
    {
        float time = 0;

        while (time < lerpTime)
        {
            time += Time.fixedDeltaTime;
            moon.SetRotation(Vector3.Lerp(start, target, time / lerpTime));

            prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
            prefabs.DrawLineMoonBulge();
            prefabs.SetMoonReferenceSystem(0);

            yield return null;
        }
    }

    IEnumerator LerpMoonRotationAlongBulge(float lerpTime)
    {
        float time = 0;

        float deltaAngle = (timeScale * resetTimer * 360 / Period);
        float target = 180 - deltaAngle;
        float start = moon.transform.eulerAngles.y;

        float substep;
        if (moonPeriodFactor == 1)
        {
            substep = 0;
        }
        else if (moonPeriodFactor > 1)
        {
            if (start < 180)
            {
                substep = (target - start) / lerpTime * Time.fixedDeltaTime;
            }
            else
            {
                substep = (360 - start) + target;
                substep = substep / lerpTime * Time.fixedDeltaTime;
            }
        }
        else
        {
            if (target < start)
            {
                substep = (360 - start) + target;
                substep = substep / lerpTime * Time.fixedDeltaTime;
            }
            else
            {
                substep = (target - start) / lerpTime * Time.fixedDeltaTime;
            }
        }

        while (time < lerpTime)
        {
            time += Time.fixedDeltaTime;
            moon.IncrementRotation(new Vector3(0, substep, 0));
            moon.IncrementRotationSprite(new Vector3(0, -substep, 0));

            prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
            prefabs.DrawLineMoonBulge();

            yield return null;
        }
        moon.SetRotation(new Vector3(0, target, 0));
    }

    private IEnumerator MoonTidalAction(float fadeTime, float startDelay = 1f, float blinkFreq = 2f)
    {
        yield return new WaitForSeconds(startDelay);

        float time = 0;
        float index = 1f;
        bool toggle = false;

        while (time < fadeTime)
        {
            time += Time.fixedDeltaTime;
            if (time > (blinkFreq * index))
            {
                toggle = !toggle;
                index++;
                ActivationTidalVectors = toggle;
            }
            yield return null;
        }

        ActivationTidalVectors = true;
        MoonIsSquashed = true;
    }

    private IEnumerator FadeInOutTidalVectors(float fadeTime)
    {
        ActivationTidalVectors = true;
        float time = 0;
        while (time < fadeTime)
        {
            time += Time.fixedDeltaTime;
            yield return null;
        }
        ActivationTidalVectors = false;
    }

    /* ************************************************************* */
    /* For Interaction purpose */
    private void DragBody()
    {
        CelestialBody draggableBody = moon;
        CelestialBody centerBody = earth;

        if (dragBodyName == DragBodyName.Earth)
        {
            draggableBody = earth;
            centerBody = moon;
        }
        if (Input.GetMouseButtonDown(0))
        {
            // Do not need z component as the camera is "looking down",
            // simulation is in the plan (X, Y)
            Vector2 bodyPositionOnScreen = mainCamera.WorldToScreenPoint(draggableBody.Position);
            Vector2 mousePositionOnClick = Input.mousePosition;

            centerOfRotation = mainCamera.WorldToScreenPoint(centerBody.Position);
            centerOfSpin = mainCamera.WorldToScreenPoint(draggableBody.Position);

            Vector2 distance = mousePositionOnClick - bodyPositionOnScreen;
            float distanceMag = distance.magnitude;

            Vector3 a = new Vector3(draggableBody.transform.localScale.x / 2, draggableBody.transform.localScale.x / 2, 0);
            Vector2 bodyRadius = mainCamera.WorldToScreenPoint(draggableBody.Position + a);

            Vector2 range = bodyRadius - bodyPositionOnScreen;
            float rangeMag = range.magnitude;

            // Ranges to define the clickable area of the body's edges.
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

            if (draggingMoonCenter && (dragBodyName != DragBodyName.None))
            {
                Vector2 screenDisplacement = currentMousePosition - centerOfRotation;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x);

                // Compute new position
                Vector3 vectorR = draggableBody.Position - centerBody.Position;
                float r = vectorR.magnitude;

                float pointNewX = r * Mathf.Cos(deltaAngle);
                float pointNewY = r * Mathf.Sin(deltaAngle);
                Vector3 position = new Vector3(pointNewX, 0, pointNewY);

                Vector3 oldMoonPos = draggableBody.Position;

                // Assign new position
                draggableBody.Position = centerBody.Position + position;

                if (dragBodyName == DragBodyName.Earth)
                {
                    // Moon Bulge follows Earth:
                    moon.SetRotation((deltaAngle * Mathf.Rad2Deg) * Vector3.down);
                    moon.SetRotationSprite(new Vector3(0, deltaAngle * Mathf.Rad2Deg + 180, 0));
                }

                prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
                prefabs.DrawLineEarthMoon();
                prefabs.DrawLineMoonBulge();
            }
            else if (draggingEdgeMoon && dragMoonEdgesIsAllowed)
            {

                Vector2 screenDisplacement = currentMousePosition - centerOfSpin;
                float deltaAngle = Mathf.Atan2(screenDisplacement.y, screenDisplacement.x) * Mathf.Rad2Deg;

                Vector3 newEuler = moonStartSpin + Vector3.down * (deltaAngle - mouseStartAngle);

                // Add 360, then mod 360:
                // otherwise you might have negative euler angle if you click on the moon 
                // while it has 180deg rotation or more: 
                // 150 -> 179.9 -> 180 -> -179.9 -> -150
                // and not: 
                // 150 -> 179.9 -> 180 -> 180.1 -> 210
                float newEulerY = (newEuler.y + 360) % 360;
                newEuler.y = newEulerY;

                if (draggableEdgesAngleRange != 0)
                {
                    // We want newEulerY to stay in the range [angleMoonSpinInit-dragRange, angleMoonSpinInit+dragRange]
                    // eg: angleMoonSpinInit=180 dragRange=90
                    // [180-90, 180+90] -> [90, 270]
                    if (newEulerY > angleMoonSpinInit + draggableEdgesAngleRange || newEulerY < angleMoonSpinInit - draggableEdgesAngleRange)
                    {
                        return;
                    }
                }

                // Assign new angle:
                moon.transform.eulerAngles = new Vector3(0, newEulerY, 0);

                // Oscillation Logic:
                oscillationX = newEulerY;
                if (90 <= newEulerY && newEulerY <= 270)
                {
                    rot180Moon = true;
                    oscillationX -= 180;
                }
                else
                {
                    rot180Moon = false;
                    if (newEulerY > 270)
                    {
                        oscillationX = newEulerY - 360;
                    }
                }
                oscillationXInvert = oscillationX + moon.getUVoffset() * 360;

                prefabs.setMoonTidalVectors(NewtonG, moonDistance, vectorTidalScale);
                prefabs.DrawLineMoonBulge();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            draggingMoonCenter = false;
            draggingEdgeMoon = false;
        }
    }

    // ********************************************************* 
    public void SetBodyMouseCursor()
    {
        switch (dragBodyName)
        {
            case DragBodyName.None:
                {
                    if (earth) { earth.SetPointerHandlerBoolean(false); }
                    if (moon)
                    {
                        if (dragMoonEdgesIsAllowed) { moon.SetPointerHandlerBoolean(true); }
                        else { moon.SetPointerHandlerBoolean(false); }
                    }
                    return;
                }
            case DragBodyName.Earth:
                {
                    if (earth) { earth.SetPointerHandlerBoolean(true); }
                    if (moon) { moon.SetPointerHandlerBoolean(false); }
                    return;
                }
            case DragBodyName.Moon:
                {
                    if (earth) { earth.SetPointerHandlerBoolean(false); }
                    if (moon) { moon.SetPointerHandlerBoolean(true); }
                    return;
                }
            default:
                if (earth) { earth.SetPointerHandlerBoolean(false); }
                if (moon) { moon.SetPointerHandlerBoolean(false); }
                break;
        }
    }
}
