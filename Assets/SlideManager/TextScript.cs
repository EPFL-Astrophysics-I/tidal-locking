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

    void OnValidate() {
        if (FR != oldFR) {
            tmpUI.text = FR;
            oldFR = FR;
        }
        if (EN != oldEN) {
            tmpUI.text = EN;
            oldEN = EN;
        }
    }
    
    void Start()
    {
        if (languageToggle != null) {
            languageToggle.OnLanguageToggle += SetLanguage;

            // Set English as the default language.
            SetLanguage(LanguageToggle.ActiveLanguage.EN);
        }
    }

    private void SetLanguage(LanguageToggle.ActiveLanguage language){
        switch (language) {
            case LanguageToggle.ActiveLanguage.EN: {
                tmpUI.text = EN;
                break;
            }
            case LanguageToggle.ActiveLanguage.FR: {
                tmpUI.text = FR;
                break;
            }
        }
    }
}
