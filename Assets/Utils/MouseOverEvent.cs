using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Texture2D cursor;
    private List<ImageColorManager> listImages = new List<ImageColorManager>();
    public void SetImage(GameObject go) {
        //Debug.Log("Set image: " + go);
        ImageColorManager colorManager;
        if (!go.TryGetComponent<ImageColorManager>( out colorManager)){
            Debug.LogWarning("(No ImageColorManager component).");
            return;
        }
        listImages.Add(colorManager);
    }
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("Cursor Entering " + name + " GameObject");
        //throw new System.NotImplementedException();
        listImages.ForEach((image) => image.ColorVar());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Cursor Exiting " + name + " GameObject");
        //throw new System.NotImplementedException();
        listImages.ForEach((image) => image.RestoreColor());
    }
}
