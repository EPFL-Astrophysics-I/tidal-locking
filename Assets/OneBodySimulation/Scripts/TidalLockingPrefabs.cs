using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TidalLockingPrefabs : MonoBehaviour
{
    [Header("Earth's Prefabs:")]
    // Serializable Prefabs:
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject earthOrbitPrefab;

    // Actual Scripted Objects:
    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public CircularOrbit earthOrbit;

    [Header("Moon's Prefabs:")]

    // Serializable Prefabs:
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject moonOrbitPrefab;
    [SerializeField] private int numberOfMoonTidalVectors;
    [SerializeField] private GameObject moonTidalVectorsPrefab;
    [SerializeField] private GameObject moonReferenceSystemPrefab;
    [SerializeField] private GameObject lineMoonBulgePrefab;

    // Actual Scripted Objects:
    [HideInInspector] public CelestialBody moon;
    [HideInInspector] public CircularOrbit moonOrbit;
    [HideInInspector] public List<Arrow> listMoonTidalVectors;
    [HideInInspector] public GameObject moonReferenceSystem;
    [HideInInspector] public Arrow lineMoonBulge;

    [Header("Earth & Moon Prefabs:")]

    // Serializable Prefabs:
    [SerializeField] private GameObject lineEarthMoonPrefab;

    // Actual Scripted Objects:
    [HideInInspector] public LineRenderer lineEarthMoon;

    [Header("Lights Prefabs")]

    // Serializable Prefabs:
    [SerializeField] private List<GameObject> listLightPrefabs;

    // Actual Scripted Objects:
    [HideInInspector] public List<Light> listLights;

    public void InstantiatePrefabs()
    {
        /* **********  Earth's Game Object Initialization *********** */
        if (earthPrefab)
        {
            earth = Instantiate(earthPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
            earth.gameObject.name = "Earth";
        }

        if (earthOrbitPrefab)
        {
            earthOrbit = Instantiate(earthOrbitPrefab, transform).GetComponent<CircularOrbit>();
            earthOrbit.gameObject.name = "Earth Orbit";
        }

        /* ********** Moon's Game Object Initialization ********** */

        if (moonPrefab)
        {
            moon = Instantiate(moonPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0), transform).GetComponent<CelestialBody>();
            moon.gameObject.name = "Moon";
        }

        if (moonOrbitPrefab)
        {
            moonOrbit = Instantiate(moonOrbitPrefab, transform).GetComponent<CircularOrbit>();
            moonOrbit.gameObject.name = "Moon Orbit";
        }

        if (moonTidalVectorsPrefab && numberOfMoonTidalVectors!=0)
        {
            listMoonTidalVectors = new List<Arrow>();

            GameObject tidalVectorsContainer = new GameObject("Vectors on the Moon in plan XY");
            tidalVectorsContainer.transform.parent = transform;

            for (int i = 0; i < numberOfMoonTidalVectors; i++) {
                Arrow ar = Instantiate(moonTidalVectorsPrefab, transform).GetComponent<Arrow>();
                ar.gameObject.name = "Moon Tidal Vector " + i;
                ar.transform.parent = tidalVectorsContainer.transform;

                listMoonTidalVectors.Add(ar);
            }
        }

        if (moonReferenceSystemPrefab)
        {
            moonReferenceSystem = Instantiate(moonReferenceSystemPrefab, Vector3.zero, Quaternion.identity, transform);
            moonReferenceSystem.gameObject.name = "Moon's reference system";
        }

        if (lineMoonBulgePrefab)
        {
            lineMoonBulge = Instantiate(lineMoonBulgePrefab, transform).GetComponent<Arrow>();
        }

        /* ********** Other Game Object Initialization ********** */

        if (lineEarthMoonPrefab)
        {
            lineEarthMoon = Instantiate(lineEarthMoonPrefab, transform).GetComponent<LineRenderer>();
            lineEarthMoon.positionCount = 2;
        }

        /* ********** Lights Game Object Initialization ********** */

        foreach (GameObject lightPrefab in listLightPrefabs)
        {
            Light light = Instantiate(lightPrefab, transform).GetComponent<Light>();
            listLights.Add(light);
        }
    }

    /* ************************************************************* */
    // Functions to update/redraw game objects :
    public void SetMoonReferenceSystem(float spriteAngle) {
        if (moonReferenceSystem) {
            moonReferenceSystem.transform.position=moon.Position;
            moonReferenceSystem.transform.rotation=moon.transform.rotation*Quaternion.AngleAxis(spriteAngle, Vector3.up);
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

    public void DrawLineMoonBulge() {
        if (lineMoonBulge) {
            float spinAngle = -moon.transform.eulerAngles.y * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(spinAngle), 0, Mathf.Sin(spinAngle));
            lineMoonBulge.transform.position = moon.Position;
            lineMoonBulge.SetComponents(dir*2);
        }
    }

    public void setMoonTidalVectors(float newtonG, float moonDistance, float tidalVecScale)
    {

        int listVectorSize = numberOfMoonTidalVectors;
        if (listVectorSize!=0)
        {
            Vector3 RAtCM = moon.Position - earth.Position;
            Vector3 gravForceAtCM = (- newtonG * earth.Mass * moon.Mass / (moonDistance * moonDistance)) * (RAtCM.normalized);

            float substep = 360 * Mathf.Deg2Rad / (numberOfMoonTidalVectors-1);

            for (int i = 0; i < listVectorSize; i++) {
                float moonRadiusX = moon.transform.localScale.x/2;
                float moonRadiusZ = moon.transform.localScale.z/2;

                if (listMoonTidalVectors[i].headInPlanXY) {
                    moonRadiusZ = moon.transform.localScale.y/2;;
                }

                float angleStep = substep * i;

                float moonRadiusXZ = (moonRadiusX*moonRadiusZ);
                float cos = Mathf.Cos(angleStep);
                float sin = Mathf.Sin(angleStep);
                moonRadiusXZ /= (Mathf.Sqrt(moonRadiusX*moonRadiusX*sin*sin + moonRadiusZ*moonRadiusZ*cos*cos));

                Vector3 position = getTidalPosition(!listMoonTidalVectors[i].headInPlanXY, moon.Position, angleStep, moonRadiusXZ, moon.transform.localEulerAngles);
                listMoonTidalVectors[i].transform.position = position;
                
                Vector3 vectorR = position - earth.Position;
                float r_dm = vectorR.sqrMagnitude;
                float dm = moon.Mass*1f;
                Vector3 gravForce = (- newtonG * earth.Mass * dm / r_dm) * (vectorR.normalized);

                listMoonTidalVectors[i].SetComponents((gravForce-gravForceAtCM)*tidalVecScale);
            }
        }
    }

    /* ************************************************************* */
    // Functions to set the activation/visibility of game objects :
    public void SetLineEarthMoonActivation(bool toggle) {
        if (lineEarthMoon) {
            GameObject go = lineEarthMoon.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetMoonRefSystemActivation(bool toggle) {
        if (moonReferenceSystem) {
            GameObject go = moonReferenceSystem.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetMoonTidalVectorActivation(bool toggle) {
        
        if (numberOfMoonTidalVectors!=0)
        {
            listMoonTidalVectors.ForEach(vec => {
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
    public void SetEarthOrbitActivation(bool toggle) {
        if (earthOrbit) {
            GameObject go = earthOrbit.gameObject;
            go.SetActive(toggle);
        }
    }
    public void SetMoonBulgeLineActivation(bool toggle) {
        if (lineMoonBulge) {
            GameObject go = lineMoonBulge.gameObject;
            go.SetActive(toggle);
        }
    }
    /* ************************************************************* */
    public void SetTidalVecLineWidth(float lineWidth) {
        if (lineWidth==0) {
            return;
        }
        int listVectorSize = listMoonTidalVectors.Count;
        if (listVectorSize!=0)
        {
            for (int i = 0; i < listVectorSize; i++) {
                listMoonTidalVectors[i].lineWidth = lineWidth;
            }
        }
    }

    private Vector3 getTidalPosition(bool planXZ, Vector3 bodyPositionInWorld, float tidalPosAngle, float radiusBody, Vector3 moonEulerSpin) 
    {
        Vector3 pointPositionFromBody;
        if (planXZ) {
            pointPositionFromBody = new Vector3(radiusBody * Mathf.Cos(tidalPosAngle), 0, radiusBody * Mathf.Sin(tidalPosAngle));
        } else {
            pointPositionFromBody = new Vector3(radiusBody * Mathf.Cos(tidalPosAngle), radiusBody * Mathf.Sin(tidalPosAngle), 0);
        }
        return bodyPositionInWorld + (Quaternion.Euler(moonEulerSpin) * pointPositionFromBody);
    }
}
