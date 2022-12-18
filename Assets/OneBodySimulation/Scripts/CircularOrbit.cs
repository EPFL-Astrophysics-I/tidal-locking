using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircularOrbit : MonoBehaviour
{
    public Color color;
    [Range(0, 5)]
    public float lineWidth = 0.1f;
    public Vector3 planeNormal = Vector3.up;

    private LineRenderer line;

    [SerializeField] private LineRenderer startDelimitation;
    [SerializeField] private LineRenderer endDelimitation;
    [SerializeField] private float delimiterSize=1;

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
        if (startDelimitation) {
            startDelimitation.startColor=color;
            startDelimitation.endColor=color;
        }
        if (endDelimitation) {
            endDelimitation.startColor=color;
            endDelimitation.endColor=color;
        }
    }

    public void SetLineWidth(float width)
    {
        if (line != null)
        {
            line.startWidth = width;
            line.endWidth = width;
        }
        if (startDelimitation) {
            startDelimitation.startWidth=width;
            startDelimitation.endWidth=width;
        }
        if (endDelimitation) {
            endDelimitation.startWidth=width;
            endDelimitation.endWidth=width;
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

    public void DrawArc(Vector3 origin, float radius, float endAngle, float startAngle, int numPoints = 100)
    {
        if (line == null)
        {
            return;
        }

        Vector3[] positions = new Vector3[numPoints];
        float arcAngle = (endAngle-startAngle);
        for (int i = 0; i < numPoints; i++)
        {
            // Create points in the XZ plane
            float theta = i * arcAngle / numPoints + startAngle;
            Vector3 position = radius * Mathf.Cos(theta) * Vector3.right;
            position += radius * Mathf.Sin(theta) * Vector3.forward;
            // Rotate to lie in the plane defined by planeNormal
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);
            positions[i] = origin + rotation * position;

            if (i==0)
            {
                DrawDelimiter(startDelimitation, origin, position);
            }
            if (i==numPoints-1)
            {
                DrawDelimiter(endDelimitation, origin, position);
            }
        }

        line.positionCount = numPoints;
        line.SetPositions(positions);
        line.loop = false;
    }

    private void DrawDelimiter(LineRenderer delimiter, Vector3 origin, Vector3 positionOnOrbit)
    {
        if (delimiter==null)
        {
            return;
        }
        Vector3 dir = (positionOnOrbit - origin).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, planeNormal);

        Vector3 pos = positionOnOrbit+delimiterSize*dir;
        pos = rotation*pos;

        Vector3 pos2 = positionOnOrbit-delimiterSize*dir;
        pos2 = rotation*pos2;

        delimiter.positionCount=2;
        Vector3[] positions = {pos, pos2};
        delimiter.SetPositions(positions);
        line.loop = false;
    }
}