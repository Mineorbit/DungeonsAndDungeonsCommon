using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerGoal : LevelObject
    {
        public static UnityEvent GameWinEvent = new UnityEvent();
        Hitbox hitbox;

        public bool[] playersInside;

        // Start is called before the first frame update
        public override void OnInit()
        {
            if (LevelManager.currentLevel.goal != null)
                LevelManager.currentLevel.Remove(this);
            LevelManager.currentLevel.goal = this;
            hitbox = GetComponentInChildren<Hitbox>();
            hitbox.Attach("Player");
            hitbox.enterEvent.AddListener((x) => { Enter(x); });
            hitbox.exitEvent.AddListener((x) => { Exit(x); });
        }


        void OnEnable()
        {
            playersInside = new bool[4];
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Enter(GameObject other)
        {
            Debug.Log("Trigger triggered");
            int p = PlayerManager.GetPlayerId(other);
            Debug.Log("We got "+p);
            if (p > 0) playersInside[p] = true;
            for (int i = 0; i < 4; i++)
            {
                if (!playersInside[i]) return;
            }
            Debug.Log("Game won");
            GameWinEvent.Invoke();
        }

        void Exit(GameObject other)
        {
            int p = PlayerManager.GetPlayerId(other);
            if (p > 0) playersInside[p] = false;
        }
    }
}