using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 /*
 * Script to attach to a gameObject if you want to colorized the corresonding text.
 */
public class OnOverLink : MonoBehaviour
{
    private List<ImageColorManager> listImages = new List<ImageColorManager>();

    public void SetImage(GameObject go) {
        ImageColorManager colorManager;
        if (!go.TryGetComponent<ImageColorManager>( out colorManager)){
            Debug.LogWarning("(No ImageColorManager component).");
            return;
        }
        listImages.Add(colorManager);
    }
    void OnMouseEnter()
    {
        Debug.Log("Enter");
        listImages.ForEach((image) => image.ColorVar());
    }

    void OnMouseExit()
    {
        //The mouse is no longer hovering over the GameObject so output this message each frame
        listImages.ForEach((image) => image.RestoreColor());
    }
}
