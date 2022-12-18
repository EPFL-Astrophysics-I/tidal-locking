using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodyPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject moonOrbitPrefab;
    [SerializeField] private GameObject moonOrbitArcPrefab;
    [SerializeField] private GameObject pointOnMoon;
    [SerializeField] private GameObject moonCenterVecPrefab;
    [SerializeField] private GameObject moonLeftVecPrefab;
    [SerializeField] private GameObject moonRightVecPrefab;
    [SerializeField] private GameObject moonPointVecPrefab;
    [SerializeField] private GameObject moonPointVecPrefabXY;
    [SerializeField] private GameObject lineEarthMoonPrefab;
    [SerializeField] private GameObject lineMoonBulgePrefab;
    [SerializeField] private GameObject moonRefSystemPrefab;
    [SerializeField] private GameObject vecMoonMousePrefab;
    [HideInInspector] private Arrow vecMoonMouse;

    [Header("Multiple Points")]
    [HideInInspector] public List<PointOnBody> listPointOnMoon;
    [SerializeField] private int numberOfMoonPoints;
    [HideInInspector] public List<Arrow> listVectorMoonPoint;
    [HideInInspector] public List<PointOnBody> listPointOnMoonXY;
    [SerializeField] private int numberOfMoonPointsXY;
    [HideInInspector] public List<Arrow> listVectorMoonPointXY;

    [Header("Variable in Equations")]
    [SerializeField] private GameObject moonMassVarEquation;
    [SerializeField] private List<GameObject> ListEarthMassEquation;
    [SerializeField] private GameObject forceOnMoonCM;

    [Header("Lights")]
    [SerializeField] private List<GameObject> listLightPrefabs;

    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public CelestialBody moon;
    [HideInInspector] public PointOnBody moonPointRight;
    [HideInInspector] public PointOnBody moonPointLeft;
    [HideInInspector] public CircularOrbit moonOrbit;
    [HideInInspector] public CircularOrbit moonOrbitArc;
    [HideInInspector] public Arrow moonCenterVec;
    [HideInInspector] public Arrow moonLeftVec;
    [HideInInspector] public Arrow moonRightVec;
    [HideInInspector] public LineRenderer lineEarthMoon;
    [HideInInspector] public LineRenderer lineMoonBulge;
    [HideInInspector] public List<Light> listLights;

    [HideInInspector] public GameObject moonRefSystem;

    private float moonCenterVecLineWidth;
    private float moonLeftVecLineWidth;
    private float moonRightVecLineWidth;

    private Color32 moonLeftVecColor = new Color32(128, 96, 50, 255);
    private Color32 moonRightVecColor = new Color32(66, 128, 90, 255);

    public void InstantiatePrefabs()
    {
        if (earthPrefab)
        {
            earth = Instantiate(earthPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
            earth.gameObject.name = "Earth";

            foreach (GameObject go in ListEarthMassEquation) {
                MouseOverEvent link;
                if (earth.gameObject.TryGetComponent<MouseOverEvent>( out link))
                {
                    link.SetImage(go);
                }
            }
        }

        if (moonRefSystemPrefab)
        {
            moonRefSystem = Instantiate(moonRefSystemPrefab, Vector3.zero, Quaternion.identity, transform);
            moonRefSystem.gameObject.name = "Moon's reference system";
        }

        if (moonPrefab)
        {
            moon = Instantiate(moonPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0), transform).GetComponent<CelestialBody>();
            moon.gameObject.name = "Moon";

            if(moonMassVarEquation) {
                MouseOverEvent link;
                if (moon.gameObject.TryGetComponent<MouseOverEvent>( out link))
                {
                    link.SetImage(moonMassVarEquation);
                }
            }
        }

        if (pointOnMoon)
        {
            moonPointRight = Instantiate(pointOnMoon, Vector3.zero, Quaternion.identity, transform).GetComponent<PointOnBody>();
            moonPointRight.planXZ=true;
            moonPointRight.gameObject.name = "dm point right";

            moonPointLeft = Instantiate(pointOnMoon, Vector3.zero, Quaternion.identity, transform).GetComponent<PointOnBody>();
            moonPointLeft.planXZ=true;
            moonPointLeft.gameObject.name = "dm point left";
        }

        if (moonOrbitPrefab)
        {
            moonOrbit = Instantiate(moonOrbitPrefab, transform).GetComponent<CircularOrbit>();
            moonOrbit.gameObject.name = "Moon Orbit";
        }

        if (moonOrbitArcPrefab)
        {
            moonOrbitArc = Instantiate(moonOrbitArcPrefab, transform).GetComponent<CircularOrbit>();
            moonOrbitArc.gameObject.name = "Moon Orbit Arc";
        }

        if (moonCenterVecPrefab)
        {
            moonCenterVec = Instantiate(moonCenterVecPrefab, transform).GetComponent<Arrow>();
            moonCenterVec.gameObject.name = "Vector from moon center to earth center";

            moonCenterVecLineWidth = moonCenterVec.lineWidth;

            if(forceOnMoonCM) {
                MouseOverEvent link;
                if (moonCenterVec.gameObject.TryGetComponent<MouseOverEvent>( out link))
                {
                    link.SetImage(forceOnMoonCM);
                }
            }
        }

        if (moonLeftVecPrefab)
        {
            moonLeftVec = Instantiate(moonLeftVecPrefab, transform).GetComponent<Arrow>();
            moonLeftVec.gameObject.name = "Vector from moon dm left to earth center";
            //moonLeftVec.color = moonLeftVecColor;

            moonLeftVecLineWidth = moonLeftVec.lineWidth;
        }

        if (moonRightVecPrefab)
        {
            moonRightVec = Instantiate(moonRightVecPrefab, transform).GetComponent<Arrow>();
            moonRightVec.gameObject.name = "Vector from moon dm right to earth center";
            //moonRightVec.color = moonRightVecColor;

            moonRightVecLineWidth = moonRightVec.lineWidth;
        }

        if (lineEarthMoonPrefab)
        {
            lineEarthMoon = Instantiate(lineEarthMoonPrefab, transform).GetComponent<LineRenderer>();
            lineEarthMoon.positionCount = 2;
        }

        if (lineMoonBulgePrefab)
        {
            lineMoonBulge = Instantiate(lineMoonBulgePrefab, transform).GetComponent<LineRenderer>();
            lineMoonBulge.positionCount = 2;
        }

        foreach (GameObject lightPrefab in listLightPrefabs)
        {
            Light light = Instantiate(lightPrefab, transform).GetComponent<Light>();
            listLights.Add(light);
        }

        if (moonPointVecPrefab)
        {
            listVectorMoonPoint = new List<Arrow>();

            GameObject parent = new GameObject("Vector Moon in plan XZ");
            parent.transform.parent = transform;

            for (int i = 0; i < numberOfMoonPoints; i++) {
                Arrow ar = Instantiate(moonPointVecPrefab, transform).GetComponent<Arrow>();
                ar.gameObject.name = "vector " + i;
                ar.transform.parent = parent.transform;

                listVectorMoonPoint.Add(ar);
            }
        }

        if (numberOfMoonPoints != 0)
        {
            listPointOnMoon = new List<PointOnBody>();

            GameObject parentPointOnMoon = new GameObject("Points on the Moon in plan XZ");
            parentPointOnMoon.transform.parent = transform;

            for (int i = 0; i < numberOfMoonPoints; i++) {
                PointOnBody pt = Instantiate(pointOnMoon, transform).GetComponent<PointOnBody>();
                pt.planXZ=true;
                pt.gameObject.name = "point " + i;
                pt.transform.parent = parentPointOnMoon.transform;

                listPointOnMoon.Add(pt);
            }
        }

        if (moonPointVecPrefabXY)
        {
            listVectorMoonPointXY = new List<Arrow>();

            GameObject parentPointOnMoonXY = new GameObject("Vectors on the Moon in plan XY");
            parentPointOnMoonXY.transform.parent = transform;

            for (int i = 0; i < numberOfMoonPointsXY; i++) {
                Arrow ar = Instantiate(moonPointVecPrefabXY, transform).GetComponent<Arrow>();
                ar.gameObject.name = "vector XY " + i;
                ar.transform.parent = parentPointOnMoonXY.transform;

                listVectorMoonPointXY.Add(ar);
            }
        }

        if (numberOfMoonPointsXY != 0)
        {
            listPointOnMoonXY = new List<PointOnBody>();

            GameObject parent = new GameObject("Vector Moon in plan XY");
            parent.transform.parent = transform;

            for (int i = 0; i < numberOfMoonPointsXY; i++) {
                PointOnBody pt = Instantiate(pointOnMoon, transform).GetComponent<PointOnBody>();
                pt.gameObject.name = "point XY " + i;
                pt.transform.parent = parent.transform;

                listPointOnMoonXY.Add(pt);
            }
        }

        if (vecMoonMousePrefab)
        {
            vecMoonMouse = Instantiate(vecMoonMousePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Arrow>();
            vecMoonMouse.gameObject.name = "Vector Mouse Moon";
        }
    }

    /* ************************************************************* */
    // Functions to update elements.
    public void SetMoonRefSystem(float spriteAngle) {
        if (moonRefSystem) {
            moonRefSystem.transform.position=moon.Position;
            moonRefSystem.transform.rotation=moon.transform.rotation*Quaternion.AngleAxis(spriteAngle, Vector3.up);
            //moonRefSystem.transform.Rotate(moon.transform.rotation.eulerAngles+(spriteAngle* Vector3.down));
        }
    }
    public void DrawLineEarthMoon() {
        if (lineEarthMoon) {
            lineEarthMoon.SetPositions(new Vector3[] {
                earth.Position,
                moon.Position
            });
        }
    }
    public void DrawMoonOrbitArc(Vector3 origin, float radius, float arcAngle, float startAngle, int numPoints = 100) {
        if (moonOrbitArc) {
           moonOrbitArc.DrawArc(origin, radius, arcAngle, startAngle, numPoints);
        }
    }
    public void DrawLineMoonBulge() {
        if (lineMoonBulge) {
            float spinAngle = -moon.transform.eulerAngles.y * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(spinAngle), 0, Mathf.Sin(spinAngle));
            lineMoonBulge.SetPositions(new Vector3[] {
                moon.Position+dir*-2,
                moon.Position+dir*2
            });
        }
    }

    public void setGravitationalVectors(float newtonG, float moonDistance, float gravVecScale, float tidalVecScale)
    {
        if (moonCenterVec) {
            moonCenterVec.transform.position = moon.Position;
            Vector3 vectorR = moon.Position - earth.Position;
            Vector3 gravForce = (- newtonG * earth.Mass * moon.Mass / (moonDistance * moonDistance)) * (vectorR.normalized);
            gravForce = gravForce*gravVecScale;
            //gravForce = gravForce*Units.getUnitLength(unitLength);
            moonCenterVec.SetComponents(gravForce);
        }

        if (moonRightVec) {
            Vector3 position = moonPointRight.transform.position;
            moonRightVec.transform.position = position;
            Vector3 vectorR = position - earth.Position;
            float r_dm = vectorR.sqrMagnitude;
            float dm = moon.Mass*1f;
            Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);
            gravForce = gravForce*gravVecScale;
            //gravForce = gravForce*Units.getUnitLength(unitLength);
            moonRightVec.SetComponents(gravForce);
        }

        if (moonLeftVec) {
            Vector3 position = moonPointLeft.transform.position;
            moonLeftVec.transform.position = position;
            Vector3 vectorR = position - earth.Position;
            float r_dm = vectorR.sqrMagnitude;
            float dm = moon.Mass*1f;
            Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);
            gravForce = gravForce*gravVecScale;
            //gravForce = gravForce*Units.getUnitLength(unitLength);
            moonLeftVec.SetComponents(gravForce);
        }

        int listVectorSize = listPointOnMoon.Count;
        if (listVectorSize!=0)
        {
            Vector3 RAtCM = moon.Position - earth.Position;
            Vector3 gravForceAtCM = (- newtonG * earth.Mass * moon.Mass / (moonDistance * moonDistance)) * (RAtCM.normalized);

            for (int i = 0; i < listVectorSize; i++) {
                Vector3 position = listPointOnMoon[i].transform.position;
                listVectorMoonPoint[i].transform.position = position;
                
                Vector3 vectorR = position - earth.Position;
                float r_dm = vectorR.sqrMagnitude;
                float dm = moon.Mass*1f;
                Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);
                //gravForce = gravForce*Units.getUnitLength(unitLength);
                //listVectorMoonPoint[i].SetComponents(gravForce);
                listVectorMoonPoint[i].SetComponents((gravForce-gravForceAtCM)*tidalVecScale);
            }
        }

        int listVectorSizeXY = listPointOnMoonXY.Count;
        if (listVectorSizeXY!=0)
        {
            Vector3 RAtCM = moon.Position - earth.Position;
            Vector3 gravForceAtCM = (- newtonG * earth.Mass * moon.Mass / (moonDistance * moonDistance)) * (RAtCM.normalized);

            for (int i = 0; i < listVectorSizeXY; i++) {
                Vector3 position = listPointOnMoonXY[i].transform.position;
                listVectorMoonPointXY[i].transform.position = position;
                
                Vector3 vectorR = position - earth.Position;
                float r_dm = vectorR.sqrMagnitude;
                float dm = moon.Mass*1f;
                Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);
                //gravForce = gravForce*Units.getUnitLength(unitLength);
                //listVectorMoonPoint[i].SetComponents(gravForce);
                listVectorMoonPointXY[i].SetComponents((gravForce-gravForceAtCM)*tidalVecScale);
            }
        }
    }

    public void setMoonPointPosition()
    {
        if (moonPointRight)
        {
            float spinAngle = moon.transform.eulerAngles.y * Mathf.Deg2Rad;
            float moonRadiusX = moon.transform.localScale.x/2;
            moonPointRight.SetPosition(moon.Position, -spinAngle, moonRadiusX);
        }

        if (moonPointLeft)
        {
            float spinAngle = moon.transform.eulerAngles.y * Mathf.Deg2Rad;
            float moonRadiusX = moon.transform.localScale.x/2;

            moonPointLeft.SetPosition(moon.Position, -spinAngle, -moonRadiusX);
        }

        if (listPointOnMoon.Count!=0)
        {
            float substep = 360 * Mathf.Deg2Rad / (listPointOnMoon.Count-1);
            for (int i = 0; i < listPointOnMoon.Count; i++) {
                float spinAngle = moon.transform.eulerAngles.y * Mathf.Deg2Rad;
                float moonRadiusX = moon.transform.localScale.x/2;
                float moonRadiusZ = moon.transform.localScale.z/2;

                float angleStep = substep * i;

                float moonRadiusXZ = (moonRadiusX*moonRadiusZ);
                float cos = Mathf.Cos(angleStep);
                float sin = Mathf.Sin(angleStep);
                moonRadiusXZ /= (Mathf.Sqrt(moonRadiusX*moonRadiusX*sin*sin + moonRadiusZ*moonRadiusZ*cos*cos));

                listPointOnMoon[i].SetPosition(moon.Position, -(spinAngle+angleStep), -moonRadiusXZ);
            }
        }

        if (listPointOnMoonXY.Count!=0)
        {
            float substep = 360 * Mathf.Deg2Rad / (listPointOnMoonXY.Count-1);
            for (int i = 0; i < listPointOnMoonXY.Count; i++) {
                float spinAngle = moon.transform.eulerAngles.y * Mathf.Deg2Rad;
                float moonRadiusX = moon.transform.localScale.x/2;
                float moonRadiusY = moon.transform.localScale.y/2;

                float angleStep = substep * i;

                float moonRadiusXY = (moonRadiusX*moonRadiusY);
                float cos = Mathf.Cos(angleStep);
                float sin = Mathf.Sin(angleStep);
                moonRadiusXY /= (Mathf.Sqrt(moonRadiusX*moonRadiusX*sin*sin + moonRadiusY*moonRadiusY*cos*cos));

                listPointOnMoonXY[i].SetPosition(moon.Position, -(spinAngle+angleStep), -moonRadiusXY);
            }
        }
    }

    public void setVecMoonMouse(Vector3 origin, float newtonG, float moonDistance, float gravVecScale)
    {
        if (vecMoonMouse) {
            vecMoonMouse.transform.position = origin;
            Vector3 vectorR = origin - earth.Position;
            float r_dm = vectorR.sqrMagnitude;
            float dm = moon.Mass*1f;
            Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);
            gravForce = gravForce*gravVecScale;
            //gravForce = gravForce*Units.getUnitLength(unitLength);
            vecMoonMouse.SetComponents(gravForce);
        }
    }

    /* ************************************************************* */
    // Functions to set the activation/visibility of prefabs elements.
    public void SetMoonRefSystemActivation(bool toggle) {
        if (moonRefSystem) {
            GameObject go = moonRefSystem.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetVectorLRactivation(bool toggle) {
        if (moonLeftVec) {
            GameObject go = moonLeftVec.gameObject;
            go.SetActive(toggle);
        }

        if (moonRightVec) {
            GameObject go = moonRightVec.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetVectorCMactivation(bool toggle) {
        if (moonCenterVec) {
            GameObject go = moonCenterVec.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetPointsOnMoonActivation(bool toggle) {
        
        if (listPointOnMoon.Count!=0)
        {
            listVectorMoonPoint.ForEach(vec => {
                vec.gameObject.SetActive(toggle);
            });
        }

        if (listPointOnMoonXY.Count!=0)
        {
            listVectorMoonPointXY.ForEach(vec => {
                vec.gameObject.SetActive(toggle);
            });
        }
    }
    public void SetMoonOrbitActivation(bool toggle) {
        if (moonOrbit) {
            GameObject go = moonOrbit.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetMoonOrbitArcActivation(bool toggle) {
        if (moonOrbitArc) {
            GameObject go = moonOrbitArc.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetMoonBulgeLineActivation(bool toggle) {
        if (lineMoonBulge) {
            GameObject go = lineMoonBulge.gameObject;
            go.SetActive(toggle);
        }
    }

    public void SetVecMoonMouseActivation(bool toggle) {
        if (vecMoonMouse) {
            GameObject go = vecMoonMouse.gameObject;
            go.SetActive(toggle);
        }
    }
    public bool GetMoonMouseActivation() {
        if (vecMoonMouse) {
            GameObject go = vecMoonMouse.gameObject;
            return go.activeSelf;
        }
        return false;
    }
    /* ************************************************************* */
    public void SetTidalVecLineWidth(float lineWidth) {
        if (lineWidth==0) {
            return;
        }
        int listVectorSize = listPointOnMoon.Count;
        if (listVectorSize!=0)
        {
            for (int i = 0; i < listVectorSize; i++) {
                listVectorMoonPoint[i].lineWidth = lineWidth;
            }
        }
        int listVectorSizeXY = listPointOnMoonXY.Count;
        if (listVectorSizeXY!=0)
        {
            for (int i = 0; i < listVectorSizeXY; i++) {
                listVectorMoonPointXY[i].lineWidth = lineWidth;
            }
        }
    }

    public void SetGravVecLineWidth(float lineWidth) {
        if (lineWidth==0) {
            if (moonCenterVec) {
            moonCenterVec.lineWidth = moonCenterVecLineWidth;
            }
            if (moonRightVec) {
                moonRightVec.lineWidth = moonRightVecLineWidth;
            }
            if (moonLeftVec) {
                moonLeftVec.lineWidth = moonLeftVecLineWidth;
            }
        } 
        else {
            if (moonCenterVec) {
            moonCenterVec.lineWidth = lineWidth;
            }
            if (moonRightVec) {
                moonRightVec.lineWidth = lineWidth;
            }
            if (moonLeftVec) {
                moonLeftVec.lineWidth = lineWidth;
            }
        }
    }
}
