using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ClickableObject : MonoBehaviour
{
    public bool interactable;

    public Texture2D hoverCursor;
    private Vector2 cursorOffset = new Vector2(14, 6);

    public static event System.Action<ClickableObject> OnObjectMouseDown;
    public static event System.Action<ClickableObject> OnObjectMouseUp;

    private bool mouseIsDown;

    public void OnMouseEnter()
    {
        // Display the cursor while hovering
        if (hoverCursor && interactable)
        {
            Cursor.SetCursor(hoverCursor, cursorOffset, CursorMode.Auto);
        }
    }

    private void OnMouseExit()
    {
        if (!interactable || mouseIsDown) return;

        ResetCursor();
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseDown()
    {
        if (!interactable) return;

        mouseIsDown = true;
        OnObjectMouseDown?.Invoke(this);
    }

    private void OnMouseUp()
    {
        if (!interactable) return;

        ResetCursor();

        mouseIsDown = false;
        OnObjectMouseUp?.Invoke(this);
    }
}
