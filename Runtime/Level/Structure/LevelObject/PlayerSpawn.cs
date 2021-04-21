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

        UnityEngine.Color GetColor(Color c)
        {
            switch(c)
            {
                case Color.Blue:
                    return UnityEngine.Color.blue;
                case Color.Green:
                    return UnityEngine.Color.green;
                case Color.Red:
                    return UnityEngine.Color.red;
                case Color.Yellow:
                    return UnityEngine.Color.yellow;
            }
            return UnityEngine.Color.white;
        }
    public override void OnInit()
    {
            if (LevelManager.currentLevel.spawn[(int)color] != null)
                LevelManager.currentLevel.Remove(this.gameObject);
            LevelManager.currentLevel.spawn[(int)color] = this;
            colorChanger.SetColor(0,GetColor(color));
    }
    
    void Start()
    {
        
    }
	public override void OnEnable()
	{
	}
	
	
    // Update is called once per frame
    void Update()
    {
        
    }
}

}