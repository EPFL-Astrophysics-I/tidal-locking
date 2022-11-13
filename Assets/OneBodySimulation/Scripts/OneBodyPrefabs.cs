﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodyPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject moonOrbitPrefab;
    [SerializeField] private GameObject moonInfPoint;
    [SerializeField] private GameObject moonCenterVecPrefab;
    [SerializeField] private GameObject moonLeftVecPrefab;
    [SerializeField] private GameObject moonRightVecPrefab;
    [SerializeField] private GameObject moonPointVecPrefab;
    [SerializeField] private GameObject lineEarthMoonPrefab;

    [Header("Multiple Points")]
    [HideInInspector] public List<PointOnBody> listPointOnMoon;
    [SerializeField] private int numberOfMoonPoints;
    [HideInInspector] public List<Arrow> moonVectorList;
    [SerializeField] private int numberOfVectors;

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
                OnOverLink link;
                if (earth.gameObject.TryGetComponent<OnOverLink>( out link))
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
                OnOverLink link;
                if (moon.gameObject.TryGetComponent<OnOverLink>( out link))
                {
                    link.SetImage(moonMassVarEquation);
                }
            }
        }

        if (moonInfPoint)
        {
            moonPointRight = Instantiate(moonInfPoint, Vector3.zero, Quaternion.identity, transform).GetComponent<PointOnBody>();
            moonPointRight.gameObject.name = "dm point right";

            moonPointLeft = Instantiate(moonInfPoint, Vector3.zero, Quaternion.identity, transform).GetComponent<PointOnBody>();
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
                OnOverLink link;
                if (moonCenterVec.gameObject.TryGetComponent<OnOverLink>( out link))
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

        // ****************************************
        if (numberOfMoonPoints == numberOfVectors) {
            if (moonPointVecPrefab)
            {
                moonVectorList = new List<Arrow>();

                for (int i = 0; i < numberOfVectors; i++) {
                    Arrow ar = Instantiate(moonPointVecPrefab, transform).GetComponent<Arrow>();
                    ar.gameObject.name = "vector " + i;

                    moonVectorList.Add(ar);
                }
            }

            if (numberOfMoonPoints != 0)
            {
                listPointOnMoon = new List<PointOnBody>();

                for (int i = 0; i < numberOfMoonPoints; i++) {
                    PointOnBody pt = Instantiate(moonInfPoint, transform).GetComponent<PointOnBody>();
                    pt.gameObject.name = "point " + i;

                    listPointOnMoon.Add(pt);
                }
            }
        }
    }
}
