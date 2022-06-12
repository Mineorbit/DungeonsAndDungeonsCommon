using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerGoal : LevelObject
    {
        public static UnityEvent GameWinEvent = new UnityEvent();

        public bool[] playersInside;
        public Hitbox hitbox;

        public Collider collider;

        public GoalAudioController goalAudioController;
        
        // Update is called once per frame
        private void Update()
        {
        }
        
        public void SetCollider()
        {
            var full_collider = Level.instantiateType == Level.InstantiateType.Play ||
                                Level.instantiateType == Level.InstantiateType.Test ||
                                Level.instantiateType == Level.InstantiateType.Online;
            collider.enabled = !full_collider;
            collider.isTrigger = full_collider;
        }
        
        
        public override void OnStartRound()
        {
            SetCollider();
        }

        public override void OnEndRound()
        {
            SetCollider();
        }
        
        // Start is called before the first frame update
        public override void OnInit()
        {
            base.OnInit();
            if (LevelManager.currentLevel.goal != null && LevelManager.currentLevel.goal != this)
            {
                LevelManager.currentLevel.Remove(gameObject);
                
            }
            else
            {
                LevelManager.currentLevel.goal = this;
            }
            SetCollider();
            hitbox = GetComponentInChildren<Hitbox>();
            hitbox.Attach("Entity");
            hitbox.enterEvent.AddListener(x => { Enter(x); });
            hitbox.exitEvent.AddListener(x => { Exit(x); });
            playersInside = new bool[4];
            goalAudioController.PlayAmbient();
            
        }


        private int pointsForGoalReached = 1000;
        private void Enter(GameObject other)
        {
                var p = PlayerManager.GetPlayerId(other);
                if (p >= 0) playersInside[p] = true;
                PlayerManager.GetPlayerById(p).points += pointsForGoalReached;
                CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            if (Level.instantiateType == Level.InstantiateType.Test ||
                Level.instantiateType == Level.InstantiateType.Play)
            {
                for (var i = 0; i < 4; i++)
                    if (PlayerManager.GetPlayerById(i) && !playersInside[i])
                        return;
                GameWinEvent.Invoke();
            }
        }

        private void Exit(GameObject other)
        {
            var p = PlayerManager.GetPlayerId(other);
            if (p >= 0) playersInside[p] = false;
        }
    }
}