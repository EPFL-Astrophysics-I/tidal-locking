using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnBody : MonoBehaviour
{
    public bool planXZ=false;
    public void SetPosition(Vector3 bodyPositionInWorld, float spinAngle, float radiusBody) 
    {
        Vector3 pointPositionFromBody;
        if (planXZ) {
            pointPositionFromBody = new Vector3(radiusBody * Mathf.Cos(spinAngle), 0, radiusBody * Mathf.Sin(spinAngle));
        } else {
            pointPositionFromBody = new Vector3(radiusBody * Mathf.Cos(spinAngle), radiusBody * Mathf.Sin(spinAngle), 0);
        }
        transform.position = bodyPositionInWorld + pointPositionFromBody;
    }
}
