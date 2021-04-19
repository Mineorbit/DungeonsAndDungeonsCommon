using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerGoal : LevelObject
    {
        // Start is called before the first frame update
        void Start()
        {
            if (LevelManager.currentLevel.goal != null)
                LevelManager.currentLevel.Remove(this);
            LevelManager.currentLevel.goal = this;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}