using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextButton : MonoBehaviour
{
    [SerializeField]
    LanguageToggle languageToggle;
    [SerializeField] private List<Button> listButtonEN;
    [SerializeField] private List<Button> listButtonFR;

    private int indexActiveButton;

    void Start() {
        if (languageToggle != null) {
            languageToggle.OnLanguageToggle += SetButton;

            // Active first Button and English Language as default
            indexActiveButton = 0;
            SetButton(LanguageToggle.ActiveLanguage.EN);
        }
    }

    private void SetButton(LanguageToggle.ActiveLanguage language){
        switch (language) {
            case LanguageToggle.ActiveLanguage.EN: {
                SetActiveButtonList(listButtonEN, true);
                SetActiveButtonList(listButtonFR, false);
                ConfigureButtons(listButtonEN, indexActiveButton);
                ConfigureButtons(listButtonFR, indexActiveButton);
                break;
            }
            case LanguageToggle.ActiveLanguage.FR: {
                SetActiveButtonList(listButtonEN, false);
                SetActiveButtonList(listButtonFR, true);
                ConfigureButtons(listButtonFR, indexActiveButton);
                ConfigureButtons(listButtonEN, indexActiveButton);
                break;
            }
        }
    }

    private void ConfigureButtons(List<Button> listBtn, int index) {
        // Underline chosen Button:
        Button btn = listBtn[index];
        TextMeshProUGUI tmp = btn.transform.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp)
        {
            tmp.fontStyle = FontStyles.Underline;
        }
        // Reset all remaining button:
        for (int i = 0; i < listBtn.Count; i++)
        {
            if (i==indexActiveButton) {
                continue;
            }
            Button button = listBtn[i];
            tmp = button.transform.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp)
            {
                tmp.fontStyle = FontStyles.Normal;
            }
        }
    }

    private void SetActiveButtonList(List<Button> listButton, bool activeBool) {
        listButton.ForEach((Button btn) => btn.gameObject.SetActive(activeBool));
    }

    public void TextButtonOnClick(int buttonIndex) {
        indexActiveButton = buttonIndex;
        ConfigureButtons(listButtonFR, indexActiveButton);
        ConfigureButtons(listButtonEN, indexActiveButton);
    }
    
    public void OnMouseEnterButton(int buttonIndex) {
        Button button = listButtonEN[buttonIndex];
        Color32 lightgray = new Color32(178, 178, 178, 255);
        ChangeTextColor(button, lightgray);
        button = listButtonFR[buttonIndex];
        ChangeTextColor(button, lightgray);
    }

    public void OnMouseExitButton(int buttonIndex) {
        Button button = listButtonEN[buttonIndex];
        ChangeTextColor(button, Color.white);
        button = listButtonFR[buttonIndex];
        ChangeTextColor(button, Color.white);
    }

    private void ChangeTextColor(Button btn, Color32 color)
    {
        TextMeshProUGUI tmp = btn.transform.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp)
        {
            tmp.color = color;
        }
    }
}
