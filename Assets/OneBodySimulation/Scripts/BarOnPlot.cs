using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarOnPlot : MonoBehaviour
{
    public string observable;
    [SerializeField] float rangeUI; 
    [SerializeField] float rangeValue;
    [SerializeField] float rangeNegValue;
    [SerializeField] Vector3 initialPosition;
    [SerializeField] GameObject line;
    [SerializeField] RectTransform parentPosUI;
    private Vector3 UIpos;

    void Awake() {
        UIpos = line.transform.position;
    }

    public void SetPosition(float value) {
        if (line) {
            float posX = 0;
            if (value<0) {
                // SpinSpeed not symmetric, so we need to rescale it:
                posX = -(value/rangeNegValue)*rangeUI;
            } else {
                posX = (value/rangeValue)*rangeUI;
            }
            //Debug.Log(line.transform.position);
            //Debug.Log(new Vector3(posX, initialPosition.y, initialPosition.z));
            Vector3 posOffset = new Vector3(posX, initialPosition.y, initialPosition.z);
            line.transform.position = parentPosUI.position + posOffset;
        }
    }
}
