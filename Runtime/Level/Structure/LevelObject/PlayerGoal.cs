using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerGoal : LevelObject
    {
        public static UnityEvent GameWinEvent = new UnityEvent();

        bool[] playersInside;

        // Start is called before the first frame update
        public override void OnInit()
        {
            if (LevelManager.currentLevel.goal != null)
                LevelManager.currentLevel.Remove(this);
            LevelManager.currentLevel.goal = this;
        }


        void OnEnable()
        {
            playersInside = new bool[4];
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnTriggerEnter(Collider other)
        {
            int p = PlayerManager.GetPlayerId(other.gameObject);
            if (p > 0) playersInside[p] = true;
            for (int i = 0; i < 4; i++)
            {
                if (!playersInside[i]) return;
            }
            GameWinEvent.Invoke();
        }
        void OnTriggerExit(Collider other)
        {
            int p = PlayerManager.GetPlayerId(other.gameObject);
            if (p > 0) playersInside[p] = false;
        }
    }
}