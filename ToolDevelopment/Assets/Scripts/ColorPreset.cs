using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPreset : MonoBehaviour
{
    public GameObject colorPicker;

    private void Start()
    {
        colorPicker.SetActive(false);
    }

    public void OpenColorPicker()
    {
        colorPicker.SetActive(true);
    }
}
