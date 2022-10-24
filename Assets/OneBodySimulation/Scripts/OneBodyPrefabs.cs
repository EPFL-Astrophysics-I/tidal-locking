using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneBodyPrefabs : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject earthPrefab;
    public GameObject moonPrefab;

    [HideInInspector] public CelestialBody earth;
    [HideInInspector] public CelestialBody moon;

    public void InstantiatePrefabs()
    {
        if (earthPrefab)
        {
            GameObject go = Instantiate(earthPrefab, transform);
            if (!go.transform.TryGetComponent(out earth))
            {
                Debug.LogWarning(go.name + " does not have a CelestialBody component");
            }
            go.name = "Earth";
        }

        if (moonPrefab)
        {
            GameObject go = Instantiate(moonPrefab, transform);
            if (!go.transform.TryGetComponent(out moon))
            {
                Debug.LogWarning(go.name + " does not have a CelestialBody component");
            }
            go.name = "Moon";
        }
    }


}
