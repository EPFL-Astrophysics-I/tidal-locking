using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomToggle : MonoBehaviour
{
    //[SerializeField] AnimationCurve animationCurve;
    [SerializeField] private GameObject uiBackground;
    [SerializeField] private GameObject uiHandler;
    [SerializeField] private float lerpDuration;

    [Header("Colors configuration")]
    [SerializeField] private Color backgroundDefaultColor;
    [SerializeField] private Color handleDefaultColor;
    [SerializeField] private Color backgroundActiveColor;
    [SerializeField] private Color handleActiveColor;


    private Toggle toggle;
    private RectTransform uiHandlerRectTransform;
    private Vector2 startHandlerPosition;
    private Image uiHandlerImage;
    private Image uiBackgroundImage;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        uiHandlerRectTransform = uiHandler.GetComponent<RectTransform>();
        startHandlerPosition = uiHandlerRectTransform.anchoredPosition;

        uiHandlerImage = uiHandler.GetComponent<Image>();
        uiBackgroundImage = uiBackground.GetComponent<Image>();

        if (toggle.isOn) {
            OnToggle(true);
        }
    }

    public void OnToggle(bool b)
    {
        uiHandlerRectTransform.anchoredPosition = b ? startHandlerPosition*-1 : startHandlerPosition;
        /*
        if (b)
            StartCoroutine(LerpHandlerPosition(startHandlerPosition, startHandlerPosition*-1, lerpDuration));
        else
            StartCoroutine(LerpHandlerPosition(startHandlerPosition*-1, startHandlerPosition, lerpDuration));*/

        uiBackgroundImage.color = b ? backgroundActiveColor : backgroundDefaultColor;
        uiHandlerImage.color = b ? handleActiveColor : handleDefaultColor;
    }
    /*
    private IEnumerator LerpHandlerPosition(Vector2 start, Vector2 target, float lerpTime) {
        float time = 0;

        while (time < lerpTime) {
            time += Time.fixedDeltaTime;

            uiHandlerRectTransform.anchoredPosition = Vector2.Lerp(start, target, animationCurve.Evaluate(time));
            
            yield return null;
        }

        uiHandlerRectTransform.anchoredPosition = target;
    }*/
}
