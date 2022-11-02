using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * Script to attach to a gameObject holding an Image,
 * if you want that it's colorized.
 */
public class ImageColorManager : MonoBehaviour
{
    private Image image;
    [SerializeField] private Color color;
    [SerializeField] private Color overColor;

    private void Awake() {
        if (!gameObject.TryGetComponent<Image>(out image))
        {
            Debug.LogWarning("No OneBodyPrefab component found.");
            return;
        }
    }
    public void ColorVar()
    {
        image.color = overColor;
    }

    public void RestoreColor()
    {
        image.color = color;
    }
}
