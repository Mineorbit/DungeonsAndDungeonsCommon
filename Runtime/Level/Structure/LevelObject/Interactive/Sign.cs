using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Sign : InteractiveLevelObject
    {
        public TextMeshProUGUI textMesh;
        public void Start()
        {
            textMesh = transform.GetComponent<TextMeshProUGUI>();
            GetProperty("text").valueChangedHandler += (a, b) =>
            {
                textMesh.SetText(GetProperty("text").Value);
            };
        }
    }
}