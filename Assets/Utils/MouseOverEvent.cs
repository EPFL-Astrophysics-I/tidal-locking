using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Texture2D cursor;
    public bool startEnabled = true;
    private Vector2 cursorOffset = new Vector2(14, 6);

    // enablePointerHandler is set to true as default:
    private bool enablePointerHandler = true;
    public bool EnablePointerHandler
    {
        get
        {
            return enablePointerHandler;
        }
        set
        {
            enablePointerHandler = value;
            if (!enablePointerHandler)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }

    private void Awake()
    {
        EnablePointerHandler = startEnabled;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (enablePointerHandler)
        {
            Cursor.SetCursor(cursor, cursorOffset, CursorMode.Auto);
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
