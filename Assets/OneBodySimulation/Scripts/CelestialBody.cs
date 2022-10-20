using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CelestialBody : MonoBehaviour
{

    private float radius = 1f;

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
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
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
}
