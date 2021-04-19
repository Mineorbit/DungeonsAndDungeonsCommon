using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{


public class PlayerSpawn : LevelObject
{

        public enum Color { Blue = 0, Red, Green, Yellow };
        public Color color;
        public ColorChanger colorChanger;
    
    public override void OnInit()
    {
            if (LevelManager.currentLevel.spawn[(int)color] != null)
                LevelManager.currentLevel.Remove(this);
            colorChanger.SetColor(0,UnityEngine.Color.green);
            LevelManager.currentLevel.spawn[(int)color] = this;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

}