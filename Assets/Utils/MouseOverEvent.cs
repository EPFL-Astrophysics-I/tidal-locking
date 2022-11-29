using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Texture2D cursor;
    private Vector2 cursorOffset = new Vector2(14, 6);
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
        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
        listImages.ForEach((image) => image.ColorVar());
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("Cursor Exiting " + name + " GameObject");
        //throw new System.NotImplementedException();
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        listImages.ForEach((image) => image.RestoreColor());
    }
}
