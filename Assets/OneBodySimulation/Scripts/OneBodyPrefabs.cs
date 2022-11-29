using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodyPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject moonOrbitPrefab;
    [SerializeField] private GameObject pointOnMoon;
    [SerializeField] private GameObject moonCenterVecPrefab;
    [SerializeField] private GameObject moonLeftVecPrefab;
    [SerializeField] private GameObject moonRightVecPrefab;
    [SerializeField] private GameObject moonPointVecPrefab;
    [SerializeField] private GameObject lineEarthMoonPrefab;

    [Header("Multiple Points")]
    [HideInInspector] public List<PointOnBody> listPointOnMoon;
    [SerializeField] private int numberOfMoonPoints;
    [HideInInspector] public List<Arrow> listVectorMoonPoint;

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
    [HideInInspector] public Arrow moonCenterVec;
    [HideInInspector] public Arrow moonLeftVec;
    [HideInInspector] public Arrow moonRightVec;
    [HideInInspector] public LineRenderer lineEarthMoon;
    [HideInInspector] public List<Light> listLights;

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
            moonPointRight.gameObject.name = "dm point right";

            moonPointLeft = Instantiate(pointOnMoon, Vector3.zero, Quaternion.identity, transform).GetComponent<PointOnBody>();
            moonPointLeft.gameObject.name = "dm point left";
        }

        if (moonOrbitPrefab)
        {
            moonOrbit = Instantiate(moonOrbitPrefab, transform).GetComponent<CircularOrbit>();
            moonOrbit.gameObject.name = "Moon Orbit";
        }

        if (moonCenterVecPrefab)
        {
            moonCenterVec = Instantiate(moonCenterVecPrefab, transform).GetComponent<Arrow>();
            moonCenterVec.gameObject.name = "Vector from moon center to earth center";

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
        }

        if (moonRightVecPrefab)
        {
            moonRightVec = Instantiate(moonRightVecPrefab, transform).GetComponent<Arrow>();
            moonRightVec.gameObject.name = "Vector from moon dm right to earth center";
            //moonRightVec.color = moonRightVecColor;
        }

        if (lineEarthMoonPrefab)
        {
            lineEarthMoon = Instantiate(lineEarthMoonPrefab, transform).GetComponent<LineRenderer>();
            lineEarthMoon.positionCount = 2;
            //moonRightVec.color = moonRightVecColor;
        }

        foreach (GameObject lightPrefab in listLightPrefabs)
        {
            Light light = Instantiate(lightPrefab, transform).GetComponent<Light>();
            listLights.Add(light);
        }

        if (moonPointVecPrefab)
        {
            listVectorMoonPoint = new List<Arrow>();

            for (int i = 0; i < numberOfMoonPoints; i++) {
                Arrow ar = Instantiate(moonPointVecPrefab, transform).GetComponent<Arrow>();
                ar.gameObject.name = "vector " + i;

                listVectorMoonPoint.Add(ar);
            }
        }

        if (numberOfMoonPoints != 0)
        {
            listPointOnMoon = new List<PointOnBody>();

            for (int i = 0; i < numberOfMoonPoints; i++) {
                PointOnBody pt = Instantiate(pointOnMoon, transform).GetComponent<PointOnBody>();
                pt.gameObject.name = "point " + i;

                listPointOnMoon.Add(pt);
            }
        }
    }

    /* ************************************************************* */
    // Functions to update elements.
    public void DrawLineEarthMoon() {
        if (lineEarthMoon) {
            lineEarthMoon.SetPositions(new Vector3[] {
                earth.Position,
                moon.Position
            });
        }
    }

    public void setGravitationalVectors(float newtonG, float moonDistance)
    {
        if (moonCenterVec) {
            moonCenterVec.transform.position = moon.Position;
            Vector3 vectorR = moon.Position - earth.Position;
            Vector3 gravForce = (- newtonG * earth.Mass * moon.Mass / (moonDistance * moonDistance)) * (vectorR.normalized);
            gravForce = gravForce*400f;
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
            gravForce = gravForce*400f;
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
            gravForce = gravForce*400f;
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
                listVectorMoonPoint[i].SetComponents((gravForce-gravForceAtCM)*500);
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
    }

    /* ************************************************************* */
    // Functions to set the activation/visibility of prefabs elements.
    public void SetVectorLRactivation(bool toggle) {
        if (moonLeftVec) {
            GameObject go = moonLeftVec.gameObject;
            go.SetActive(!toggle);
        }

        if (moonRightVec) {
            GameObject go = moonRightVec.gameObject;
            go.SetActive(!toggle);
        }
    }
    public void SetVectorCMactivation(bool toggle) {
        if (moonCenterVec) {
            GameObject go = moonCenterVec.gameObject;
            go.SetActive(!toggle);
        }
    }
    public void SetPointsOnMoonActivation(bool toggle) {
        if (listPointOnMoon.Count!=0)
        {
            listVectorMoonPoint.ForEach(vec => {
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
}
