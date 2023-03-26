using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CelestialBody : MonoBehaviour
{
    public float squashTimer = 1f;
    private float radius = 1f;
    private Vector3 squashedScale;
    private Vector3 normalScale;
    private Vector3 squashFactor = new Vector3(1.6f, 1f, 1f);
    private bool isSquashed = false;
    public bool IsSquashed
    {
        get { return isSquashed; }
        set
        {
            if (isSquashed != value)
            {
                isSquashed = value;
                OnSquashed();
            }
            else if (transform.localScale != normalScale || transform.localScale != squashedScale)
            {
                // Coroutines are stopped when transitionning for one slide to another.
                // Then if the squashed boolean changes between two slides,
                // we have a coroutine, but if we click to next slide before the coroutine changes, we will not have the right scale.
                // this condition ensure it is the case.
                //OnSquashed();
                if (isSquashed)
                {
                    transform.localScale = squashedScale;
                }
                else
                {
                    transform.localScale = normalScale;
                }
            }
        }
    }

    // Quantities of motion
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }
    [HideInInspector] public Vector3 Velocity { get; set; } = Vector3.zero;
    [HideInInspector] public float Mass { get; set; } = 1f;

    // Rotation of the body about its own axis
    [HideInInspector] public float RotationPeriod { get; set; } = 1f;

    private Renderer spriteRenderer;

    /* -- !! --
        MouseOverEvent script needs to be above CelestialBody script !
        Otherwise TryGetComponent<MouseOverEvent>(out mouseOverEvent); can not recover the script,
        And the cursor functionality will not work.
    */
    private MouseOverEvent mouseOverEvent;
    void Start()
    {
        TryGetComponent<Renderer>(out spriteRenderer);
        TryGetComponent<MouseOverEvent>(out mouseOverEvent);
    }

    public void SetRadius(float radius)
    {
        this.radius = radius;
        transform.localScale = 2 * this.radius * Vector3.one;

        normalScale = transform.localScale;
        squashedScale = Vector3.Scale(transform.localScale, squashFactor);
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;

        normalScale = transform.localScale;
        squashedScale = Vector3.Scale(transform.localScale, squashFactor);
    }

    public void Translate(Vector3 displacement)
    {
        Position += displacement;
    }

    public void IncrementVelocity(Vector3 deltaVelocity)
    {
        Velocity += deltaVelocity;
    }

    public void IncrementRotation(Vector3 deltaRotation)
    {
        transform.Rotate(deltaRotation);
    }

    public void SetRotation(Vector3 rotation)
    {
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void SetRotationSprite(Vector3 rotation)
    {
        if (spriteRenderer)
        {
            float radians = Mathf.Deg2Rad * (rotation.y);
            Vector2 vec_offset = new Vector2(
                    radians / (Mathf.PI * 2),
                    0
                );
            spriteRenderer.material.SetVector("Vector2_Offset_Sprite", vec_offset);
        }
    }

    public void IncrementRotationSprite(Vector3 rotation)
    {
        if (spriteRenderer)
        {
            Vector2 previousOffset = spriteRenderer.material.GetVector("Vector2_Offset_Sprite");
            float radians = Mathf.Deg2Rad * (rotation.y % 360);
            Vector2 vec_offset = new Vector2(
                    radians / (Mathf.PI * 2),
                    0
                );
            spriteRenderer.material.SetVector("Vector2_Offset_Sprite", previousOffset + vec_offset);
        }
    }

    public float getUVoffset()
    {
        if (spriteRenderer)
        {
            Vector2 previousOffset = spriteRenderer.material.GetVector("Vector2_Offset_Sprite");
            return previousOffset.x;
        }
        return 0;
    }

    public void DEBUG_OFFET()
    {
        if (spriteRenderer)
        {
            Vector2 previousOffset = spriteRenderer.material.GetVector("Vector2_Offset_Sprite");
            Debug.Log(previousOffset);
        }
    }

    private void OnSquashed()
    {
        if (isSquashed)
        {
            StartCoroutine(LerpScale(transform, squashedScale, squashTimer));
        }
        else
        {
            StartCoroutine(LerpScale(transform, normalScale, squashTimer));
        }
    }

    IEnumerator LerpScale(Transform body, Vector3 scale, float lerpTime)
    {
        float time = 0;
        Vector3 startScale = body.localScale;
        Vector3 targetScale = scale;

        while (time < lerpTime)
        {
            time += Time.deltaTime;
            body.localScale = Vector3.Lerp(startScale, targetScale, time / lerpTime);
            yield return null;
        }

        body.localScale = targetScale;
    }

    public void SetPointerHandlerBoolean(bool enable)
    {
        if (mouseOverEvent)
        {
            mouseOverEvent.EnablePointerHandler = enable;
        }
        else
        {
            // Retry to get MouseOverEvent script:
            // Useful for the first slide of the simulation, otherwise bodies will not have proper cursor.
            TryGetComponent<MouseOverEvent>(out mouseOverEvent);
            if (mouseOverEvent)
                mouseOverEvent.EnablePointerHandler = enable;
        }
    }
}
