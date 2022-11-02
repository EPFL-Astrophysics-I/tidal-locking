using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointOnBody : MonoBehaviour
{
    public void SetPosition(Vector3 bodyPositionInWorld, float spinAngle, float radiusBody) 
    {
        Vector3 pointPositionFromBody = new Vector3(radiusBody * Mathf.Cos(spinAngle), 0, radiusBody * Mathf.Sin(spinAngle));
        transform.position = bodyPositionInWorld + pointPositionFromBody;
    }
}
