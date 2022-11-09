using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CelestialBody : MonoBehaviour
{
    private float radius = 1f;
    private Vector3 squashedScale;
    private Vector3 normalScale;

    // Use methods instead of property ? Whats is the convention ?
    private Vector3 squashFactor = new Vector3(1.6f, 1f, 1f);
    private bool isSquashed = false;
    public bool IsSquashed
    {
        get { return isSquashed; }
        set 
        { 
            if (isSquashed!=value) {
                isSquashed = value;
                OnSquashed();
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
    [HideInInspector]public float RotationPeriod { get; set; } = 1f;

    public void SetRadius(float radius)
    {
        this.radius = radius;
        transform.localScale = 2 * this.radius * Vector3.one;

        normalScale =  transform.localScale;
        squashedScale = Vector3.Scale(transform.localScale, squashFactor);
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;

        normalScale =  transform.localScale;
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

    private void OnSquashed() {
        // ERROR IF TOO FAST ?
        if (isSquashed) {
            StartCoroutine(LerpScale(transform, squashedScale, 1f));
        } else {
            StartCoroutine(LerpScale(transform, normalScale, 1f));
        }
    }

    IEnumerator LerpScale(Transform body, Vector3 scale, float lerpTime) {
        float time = 0;
        Vector3 startScale = body.localScale;
        Vector3 targetScale = scale;

        while (time < lerpTime) {
            time += Time.deltaTime;
            body.localScale = Vector3.Lerp(startScale, targetScale, time/lerpTime);
            yield return null;
        }

        body.localScale = targetScale;
    }
}
