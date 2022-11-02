using System.Collections;
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
        if(image) {
            image.ColorVar();
        }
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        if(image) {
            image.RestoreColor();
        }
    }
}
