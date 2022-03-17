using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public Slider MouseSensitivitySlider;

    void Start()
    {
        MouseSensitivitySlider.onValueChanged.AddListener(sliderValue =>
        {
            var controller = GetFirstPersonController();
            if (controller == null)
                return;

            float mouseSensitivity = sliderValue - 0.5f;
            mouseSensitivity *= 3; //arbitrary constant chosen because it feels right
            mouseSensitivity = Mathf.Exp(mouseSensitivity);

            controller.MouseSensitivity = mouseSensitivity;
        });
    }

    private static FirstPersonController GetFirstPersonController()
    {
        //not fast but OK for single instance on isolated event handling
        return Object.FindObjectOfType<FirstPersonController>();
    }
}
