using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownView : MonoBehaviour
{

    [SerializeField] Vector3 AxisOfRotation;
    [SerializeField] float InitialAngle;
    [SerializeField] List<GameObject> listVectors;

    void Awake() {
        listVectors.ForEach((vec) => {
            vec.transform.Rotate(InitialAngle*AxisOfRotation);
        });
    }

    public void SetRotationOfVector(float angle, int index) {
        if (listVectors.Count<=index) {
            return;
        }

        listVectors[index].transform.rotation = Quaternion.AngleAxis(angle+InitialAngle, AxisOfRotation);
    }

    public void ResetRotation()
    {
        listVectors.ForEach((vec) => {
            vec.transform.rotation = Quaternion.Euler(InitialAngle*AxisOfRotation);
        });
    }
}
