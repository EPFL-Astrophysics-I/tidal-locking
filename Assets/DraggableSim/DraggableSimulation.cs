using System.Collections;
using UnityEngine;
using static Units;

public class DraggableSimulation : Simulation
{
    [Header("Objects")]
    public CelestialBody earth;
    public CelestialBody moon;
    public LineRenderer orbitalRadius;
    public CircularOrbit moonOrbit;
    public Arrow tidalVectorPrefab;

    public enum DraggableBody { None, Earth, Moon }
    [Header("Interaction")]
    public DraggableBody draggableBody = default;
    private CelestialBody bodyBeingDragged;
    public float dragPlaneDistance = 10f;

    [Header("Options")]
    // Factor by which to scale the earth and moon radii
    public float radiusScale = 10;
    // Visibility of orbital features
    public bool showOrbit;
    public bool showOrbitalRadius;
    public float maxMoonRotationAngle = 45;

    // Units system
    private UnitTime unitTime = UnitTime.Day;
    private UnitLength unitLength = UnitLength.EarthMoonDistance;
    private UnitMass unitMass = UnitMass.EarthMass;

    // Moon distance in these units
    private float moonDistance = Units.LunarDistance(UnitLength.EarthMoonDistance);

    // For interactions
    private Camera mainCamera;
    private Plane dragPlane;

    private Coroutine reshapeAnimation;
    private float angleOffsetSign;

    private void Start()
    {
        mainCamera = Camera.main;

        // Create a plane lying in the (x, z) plane
        dragPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private void OnEnable()
    {
        ClickableObject.OnObjectMouseDown += HandleClickableObjectMouseDown;
        ClickableObject.OnObjectMouseUp += HandleClickableObjectMouseUp;
        CameraController.OnInPosition += HandleCameraInPosition;
    }

    private void OnDisable()
    {
        ClickableObject.OnObjectMouseDown -= HandleClickableObjectMouseDown;
        ClickableObject.OnObjectMouseUp -= HandleClickableObjectMouseUp;
        CameraController.OnInPosition -= HandleCameraInPosition;
    }

    public void HandleClickableObjectMouseDown(ClickableObject clickableObject)
    {
        // Debug.Log("Start dragging " + clickableObject.name);
        clickableObject.TryGetComponent(out bodyBeingDragged);
    }

    public void HandleClickableObjectMouseUp(ClickableObject clickableObject)
    {
        // Debug.Log("Stop dragging " + clickableObject.name);
        if (clickableObject.CompareTag("Moon"))
        {
            clickableObject.interactable = false;
            reshapeAnimation = StartCoroutine(ReshapeSequence(2));
        }
        bodyBeingDragged = null;
    }

    public void HandleCameraInPosition()
    {
        // Debug.Log("DraggableSimulation > Camera is in position");

        ClickableObject clickableObject;

        if (earth.TryGetComponent(out clickableObject))
        {
            clickableObject.interactable = draggableBody == DraggableBody.Earth;
            // Debug.Log("Earth is clickable ? " + clickableObject.interactable);

            // Check if the mouse is already over the earth
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Earth"))
                {
                    clickableObject.OnMouseEnter();
                }
            }
        }

        if (moon.TryGetComponent(out clickableObject))
        {
            clickableObject.interactable = draggableBody == DraggableBody.Moon;
            // Debug.Log("Moon is clickable ? " + clickableObject.interactable);

            // Check if the mouse is already over the moon
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Moon"))
                {
                    clickableObject.OnMouseEnter();
                }
            }
        }
    }

    private void Update()
    {
        if (bodyBeingDragged == null || !moon) return;

        // Create a ray from the mouse click position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Initialise the enter variable
        float enter = 0.0f;

        if (dragPlane.Raycast(ray, out enter))
        {
            // Get the point that is clicked
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 direction = (hitPoint - moon.Position).normalized;

            if (bodyBeingDragged.CompareTag("Earth"))
            {
                bodyBeingDragged.Position = moon.Position + moonDistance * direction;
                moon.transform.right = -direction;
                float offsetU = Mathf.Atan2(direction.z, direction.x) / (2 * Mathf.PI);
                moon.SetTextureOffset(offsetU * Vector2.right);
                RedrawOrbitalRadius();
            }
            else if (bodyBeingDragged.CompareTag("Moon"))
            {
                float angle = Mathf.Atan2(direction.z, direction.x);
                float maxRadians = maxMoonRotationAngle * Mathf.PI / 180f;
                if (Mathf.Abs(angle) < 0.5f * Mathf.PI)
                {
                    // Clicked on the right side
                    angle = Mathf.Clamp(angle, -maxRadians, maxRadians);
                    bodyBeingDragged.transform.right = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    angleOffsetSign = Mathf.Sign(angle);
                }
                else
                {
                    // Clicked the left side
                    angle = Mathf.Sign(angle) * Mathf.PI - angle;
                    angle = Mathf.Clamp(angle, -maxRadians, maxRadians);
                    bodyBeingDragged.transform.right = new Vector3(Mathf.Cos(angle), 0, -Mathf.Sin(angle));
                    angleOffsetSign = Mathf.Sign(-angle);
                }
            }
        }
    }

    public void Reset()
    {
        if (reshapeAnimation != null)
        {
            StopCoroutine(reshapeAnimation);
            reshapeAnimation = null;
        }

        Debug.Log("DraggableSimulation > Reset()");
        ResetEarth();
        ResetMoon();
        RedrawOrbit();
        RedrawOrbitalRadius();

        bodyBeingDragged = null;

        Pause();
    }

    private void ResetEarth()
    {
        if (earth)
        {
            earth.Position = Vector3.zero;
            earth.Mass = EarthMass(unitMass);
            earth.SetRadius(radiusScale * EarthRadius(unitLength));
            earth.RotationPeriod = EarthRotationPeriod(unitTime);
            earth.SetRotation(Vector3.zero);

            if (earth.TryGetComponent(out ClickableObject clickableObject))
            {
                clickableObject.interactable = false;
            }
        }
    }

    private void ResetMoon()
    {
        if (moon)
        {
            moon.Position = earth.Position + LunarDistance(unitLength) * Vector3.right;
            moon.Mass = LunarMass(unitMass);
            moon.SetRadius(radiusScale * LunarRadius(unitLength));
            moonDistance = (moon.Position - earth.Position).magnitude;
            moon.SetRotation(Vector3.zero);
            moon.SetTextureOffset(0.5f * Vector2.right);

            // Recall that this starts a coroutine
            moon.IsSquashed = true;

            if (moon.TryGetComponent(out ClickableObject clickableObject))
            {
                clickableObject.interactable = false;
            }
        }
    }

    private void RedrawOrbit()
    {
        if (moonOrbit && earth && moon)
        {
            Vector3 center = (draggableBody == DraggableBody.Earth) ? moon.Position : earth.Position;
            moonOrbit.DrawOrbit(center, moonDistance, 180);

            moonOrbit.gameObject.SetActive(showOrbit);
        }
    }

    private void RedrawOrbitalRadius()
    {
        if (orbitalRadius && earth && moon)
        {
            orbitalRadius.SetPositions(new Vector3[2] { earth.Position, moon.Position });
            orbitalRadius.gameObject.SetActive(showOrbitalRadius);
        }
    }

    private IEnumerator ReshapeSequence(float lerpTime)
    {
        Quaternion startRotation = moon.transform.localRotation;
        Quaternion targetRotation = Quaternion.identity;

        float startTextureOffset = moon.GetTextureOffset().x;
        float deltaAngle = Quaternion.Angle(startRotation, targetRotation);
        float targetTextureOffset = startTextureOffset - angleOffsetSign * deltaAngle / 360f;

        float time = 0;
        while (time < lerpTime)
        {
            time += Time.deltaTime;
            float t = Mathf.Pow(time, 0.8f);
            Quaternion rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            moon.transform.localRotation = rotation;

            float offset = Mathf.Lerp(startTextureOffset, targetTextureOffset, t);
            moon.SetTextureOffset(offset * Vector2.right);

            yield return null;
        }

        // Allow clicks on the moon again
        if (moon.TryGetComponent(out ClickableObject clickableObject))
        {
            clickableObject.interactable = draggableBody == DraggableBody.Moon;
            // Debug.Log("Moon is clickable again");

            // Check if the mouse is still over the moon
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Moon"))
                {
                    Debug.Log("Still hovering moon");
                    clickableObject.OnMouseEnter();
                }
            }
        }

        reshapeAnimation = null;
        // angleOffsetSign = 0;
    }
}
