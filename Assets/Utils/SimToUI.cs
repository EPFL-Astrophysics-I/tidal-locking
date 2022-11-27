using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimToUI : MonoBehaviour
{
    [SerializeField] private OneBodySimulation sim;
    private TextMeshProUGUI TMPgui;
    // Start is called before the first frame update
    void Start()
    {
        TextMeshProUGUI tmp;
        if(TryGetComponent<TextMeshProUGUI>(out tmp)) {
            TMPgui=tmp;
            updateGUI();
        }
    }

    public void updateGUI() {
        TMPgui.text = (sim.getMoonPeriod()).ToString("F1");
    }
}
