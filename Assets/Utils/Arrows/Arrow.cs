using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Arrow : MonoBehaviour
{
    [HideInInspector] public LineRenderer bodyLR;

    [Header("Properties")]
    public bool headInPlanXY=false;
    public Vector3 components;
    public Color color = Color.black;
    [Min(0)] public float lineWidth = 0.2f;
    public int sortingOrder = 0;

    [Header("Head Arrow")]
    public LineRenderer headLR;
    [Range(0, 60)] public float headAngle = 45;

    [Header("Head Sphere")]
    public LineRenderer headOnePoint;

    /*
    void Start() {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
 
        Mesh mesh = new Mesh();
        bodyLR.BakeMesh(mesh, true);
        mesh.Optimize();
        meshCollider.sharedMesh = mesh;
    }*/
    private Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;

        bodyLR = GetComponent<LineRenderer>();
        bodyLR.positionCount = 2;
        if (headLR) {headLR.positionCount = 3;}
        if (headOnePoint) {
            headOnePoint.positionCount = 2;
            Vector3 headPosition = components;
            headOnePoint.SetPositions(new Vector3[] { headPosition, headPosition });
            headOnePoint.startWidth = lineWidth;
            headOnePoint.endWidth = lineWidth;
            headOnePoint.startColor = color;
            headOnePoint.endColor = color;

            headOnePoint.sortingOrder = sortingOrder;
        }
    }

    public void Redraw()
    {
        bodyLR.sortingOrder = sortingOrder;
        if (headLR) {headLR.sortingOrder = sortingOrder;}
        // Draw the body
        bodyLR.SetPositions(new Vector3[] { Vector3.zero, components });

        float width = Mathf.Min(components.magnitude / 3, lineWidth);

        bodyLR.startWidth = width;
        bodyLR.endWidth = width;
        bodyLR.startColor = color;
        bodyLR.endColor = color;

        // Draw the head
        if (headLR) {
            Vector3 headPosition = components;
            // Direction along the arrow
            Vector3 e1 = components.normalized;
            // Direction orthogonal to the vector in the plane spanned by the arrow and the y-axis
            Vector3 e2;
            if (headInPlanXY) {
                e2 = (e1.x == 0) ? Vector3.up : Vector3.Cross(Vector3.Cross(e1, Vector3.up).normalized, e1);
            } else {
                e2 = (e1.x == 0) ? Vector3.right : Vector3.Cross(Vector3.Cross(e1, Vector3.forward).normalized, e1);
            }

            float angle = Mathf.Deg2Rad * headAngle;
            float headLength = Mathf.Min(components.magnitude, 2 * width);
            Vector3 headPoint1 = headPosition + headLength * (-Mathf.Cos(angle) * e1 + Mathf.Sin(angle) * e2);
            Vector3 headPoint2 = headPosition + headLength * (-Mathf.Cos(angle) * e1 - Mathf.Sin(angle) * e2);
            headLR.SetPositions(new Vector3[] { headPoint1, headPosition, headPoint2 });

            headLR.startWidth = width;
            headLR.endWidth = width;
            headLR.startColor = color;
            headLR.endColor = color;

            headLR.sortingOrder = sortingOrder;
        }

        //Debug.Log("head one point: " + headOnePoint);
        if (headOnePoint) {
            Vector3 headPosition = components;
            headOnePoint.SetPositions(new Vector3[] { headPosition, headPosition });
            headOnePoint.startWidth = width;
            headOnePoint.endWidth = width;
            headOnePoint.startColor = color;
            headOnePoint.endColor = color;

            headOnePoint.sortingOrder = sortingOrder;
        }
    }

    public void SetComponents(Vector3 components, bool redraw = true)
    {
        this.components = components;
        if (redraw) {
            Redraw();
        }
    }
}
