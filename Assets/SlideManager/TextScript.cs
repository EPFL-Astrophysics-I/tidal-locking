using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class TextScript : MonoBehaviour
{
    [SerializeField]
    LanguageToggle languageToggle;
    
    [SerializeField] 
    private TextMeshProUGUI tmpUI;

    [SerializeField] 
    private string FR;

    private string oldFR = "";

    [SerializeField] 
    private string EN;

    private string oldEN = "";

    [SerializeField]
    private Vector3 posOffsetFR;
    private RectTransform rectTransform;
    private Vector3 posEN;

    void OnValidate() {
        getRectTransform();

        if (tmpUI) {
            if (FR != oldFR) {
                tmpUI.text = FR;
                oldFR = FR;
                if (!posOffsetFR.Equals(Vector3.zero)) {
                    SetOffset(posOffsetFR);
                }
            }
            if (EN != oldEN) {
                tmpUI.text = EN;
                oldEN = EN;
                if (!posOffsetFR.Equals(Vector3.zero)) {
                    SetOffset(Vector3.zero);
                }
            }
        }
    }
    
    void Start()
    {
        getRectTransform();

        if (languageToggle != null) {
            languageToggle.OnLanguageToggle += SetLanguage;

            // Set English as the default language.
            SetLanguage(LanguageToggle.ActiveLanguage.EN);
        }
    }

    private void SetLanguage(LanguageToggle.ActiveLanguage language){
        switch (language) {
            case LanguageToggle.ActiveLanguage.EN: {
                if (tmpUI) {
                    tmpUI.text = EN;
                }
                if (!posOffsetFR.Equals(Vector3.zero)) {
                    SetOffset(Vector3.zero);
                }
                break;
            }
            case LanguageToggle.ActiveLanguage.FR: {
                if (tmpUI) {
                    tmpUI.text = FR;
                }
                if (!posOffsetFR.Equals(Vector3.zero)) {
                    SetOffset(posOffsetFR);
                }
                break;
            }
        }
    }

    private void getRectTransform() {
        if (!posOffsetFR.Equals(Vector3.zero)) {
            RectTransform rt = GetComponent<RectTransform>();
            if (rt) {
                rectTransform = rt;
                posEN = rt.localPosition;
            }
        }
    }

    private void SetOffset(Vector3 offset) {
        rectTransform.localPosition = posEN + offset;
    }
}
