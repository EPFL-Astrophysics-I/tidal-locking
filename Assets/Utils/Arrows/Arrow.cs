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

    [Header("Head")]
    public LineRenderer headLR;
    [Range(0, 60)] public float headAngle = 45;

    /*
    void Start() {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
 
        Mesh mesh = new Mesh();
        bodyLR.BakeMesh(mesh, true);
        mesh.Optimize();
        meshCollider.sharedMesh = mesh;
    }*/

    
    // Add edge collider for Mouse Over
    EdgeCollider2D edgeColliderHead;
    BoxCollider boxCollider;
    private MeshCollider meshCollider;
    private Mesh lineMesh;
    private Camera mainCamera;

    void Start() {
        /*
        if (!gameObject.TryGetComponent<EdgeCollider2D>( out edgeColliderHead))
        {
            return;
        }*/
    }

    void SetMeshCollider() {
        if (meshCollider) {
            if (bodyLR) {
                if (bodyLR.positionCount==2) {
                    lineMesh.Clear();
                    bodyLR.BakeMesh(lineMesh, mainCamera, false);
                    if (IsMeshOk(lineMesh))
                        meshCollider.sharedMesh = lineMesh;
                }
            }
        }
    }

    public bool IsMeshOk(Mesh m) {
      foreach (Vector3 v in m.vertices) {
        if (!IsFinite(v.x) || !IsFinite(v.y) || !IsFinite(v.z)) {
            return false;
        }
      };
      return true;
    }

    bool IsFinite(float f) {
        return !float.IsInfinity(f) && !float.IsNaN(f);
    }



    void SetEdgeColliders() {
        if (edgeColliderHead) {
            Vector2[] edges = new Vector2[bodyLR.positionCount];

            for(int i = 0; i<bodyLR.positionCount; i++) {
                Vector3 lrPoint = bodyLR.GetPosition(i);
                Vector3 lrPosition = transform.position;
                edges[i] = new Vector2(lrPoint.x, lrPoint.z);
                Vector2 originPos = new Vector2(transform.position.x, transform.position.z);
                //edges[i] += originPos;
            }
            edgeColliderHead.points = edges;
        }
    }

    void SetBoxCollider() {
        if (boxCollider) {
            Vector3[] edges = new Vector3[bodyLR.positionCount];

            boxCollider.transform.position=bodyLR.GetPosition(0);
            boxCollider.size=bodyLR.GetPosition(1);
        }
    }
    

    private void Awake()
    {
        mainCamera = Camera.main;
        gameObject.TryGetComponent<MeshCollider>( out meshCollider);
        lineMesh = new Mesh();

        bodyLR = GetComponent<LineRenderer>();
        bodyLR.positionCount = 2;
        if (headLR) headLR.positionCount = 3;
    }

    public void Redraw()
    {
        bodyLR.sortingOrder = sortingOrder;
        if (headLR) {headLR.sortingOrder = sortingOrder;}
        // Draw the body
        bodyLR.SetPositions(new Vector3[] { Vector3.zero, components });

        /*
        if (meshCollider) {
            
            Mesh m = new Mesh();
            m.vertices = new Vector3[] {Vector3.zero, components};

            meshCollider.sharedMesh = m;
        }*/

        float width = Mathf.Min(components.magnitude / 3, lineWidth);

        bodyLR.startWidth = width;
        bodyLR.endWidth = width;
        bodyLR.startColor = color;
        bodyLR.endColor = color;

        // Draw the head
        if (!headLR) return;

        Vector3 headPosition = components;
        // Direction along the arrow
        Vector3 e1 = components.normalized;
        // Direction orthogonal to the vector in the plane spanned by the arrow and the y-axis
        Vector3 e2;
        if (headInPlanXY) {
            e2 = (e1.x == 0) ? Vector3.right : Vector3.Cross(Vector3.Cross(e1, Vector3.up).normalized, e1);
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

    public void SetComponents(Vector3 components, bool redraw = true)
    {
        this.components = components;
        if (redraw) {
            Redraw();
            //SetEdgeColliders();
            //SetMeshCollider();
        }
    }
}
