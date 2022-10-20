using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircularOrbit : MonoBehaviour
{
    public Color color = Color.black;
    [Range(0, 5)]
    public float lineWidth = 0.1f;
    public Vector3 planeNormal = Vector3.up;

    private LineRenderer line;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();

        SetColor(color);
        SetLineWidth(lineWidth);
        ClearOrbit();
    }

    public void ClearOrbit()
    {
        if (line != null)
        {
            line.positionCount = 0;
        }
    }

    public void SetColor(Color color)
    {
        if (line != null)
        {
            line.startColor = color;
            line.endColor = color;
        }
    }

    public void SetLineWidth(float width)
    {
        if (line != null)
        {
            line.startWidth = width;
            line.endWidth = width;
        }
    }

    public void DrawOrbit(Vector3 origin, float radius, int numPoints = 100)
    {
        if (line == null)
        {
            return;
        }

        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            // Create points in the XZ plane
            float theta = i * 2 * Mathf.PI / numPoints;
            Vector3 position = radius * Mathf.Cos(theta) * Vector3.right;
            position += radius * Mathf.Sin(theta) * Vector3.forward;
            // Rotate to lie in the plane defined by planeNormal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);
            positions[i] = origin + rotation * position;
        }

        line.positionCount = numPoints;
        line.SetPositions(positions);
        line.loop = true;
    }
}