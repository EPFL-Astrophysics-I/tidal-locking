﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 /*
 * Script to attach to a gameObject if you want to colorized the corresonding text.
 */
public class OnOverLink : MonoBehaviour
{
    private ImageColorManager image;

    public void SetImage(GameObject go) {
        if (!go.TryGetComponent<ImageColorManager>( out image)){
            Debug.LogWarning("(No ImageColorManager component).");
            return;
        }
    }
    void OnMouseEnter()
    {
        Debug.Log("Mouse enter on GameObject.");
        if(image) {
            image.ColorVar();
        }
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        Debug.Log("Mouse is no longer on GameObject.");

        if(image) {
            image.RestoreColor();
        }
    }
}