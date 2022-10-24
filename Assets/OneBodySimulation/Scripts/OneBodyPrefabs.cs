using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodyPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject earthPrefab;
    [SerializeField] private GameObject moonPrefab;
    [SerializeField] private GameObject moonOrbitPrefab;

    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public CelestialBody moon;
    [HideInInspector] public CircularOrbit moonOrbit;

    public void InstantiatePrefabs()
    {
        if (earthPrefab)
        {
            earth = Instantiate(earthPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
            earth.gameObject.name = "Earth";
        }

        if (moonPrefab)
        {
            moon = Instantiate(moonPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<CelestialBody>();
            moon.gameObject.name = "Moon";
        }

        if (moonOrbitPrefab)
        {
            moonOrbit = Instantiate(moonOrbitPrefab, transform).GetComponent<CircularOrbit>();
            moonOrbit.gameObject.name = "Moon Orbit";
        }
    }


}
