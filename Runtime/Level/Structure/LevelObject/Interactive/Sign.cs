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
        public override void OnInit()
        {
            base.OnInit();
            textMesh = transform.GetComponentInChildren<TextMeshProUGUI>();
            GetProperty("text").valueChangedHandler += (a, b) =>
            {
                GameConsole.Log("TEST");
                textMesh.SetText(GetProperty("text").Value);
            };
        }
    }
}