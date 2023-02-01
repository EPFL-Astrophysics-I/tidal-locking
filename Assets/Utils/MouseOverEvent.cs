using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Texture2D cursor;
    private Vector2 cursorOffset = new Vector2(14, 6);
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
