using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class Player : Entity
    {

        public ColorChanger colorChanger;

        public int _localId;

        public float speed;

        bool hitCooldown = false;


        public int localId
        {
            set
            {
                _localId = value;
                changeColor(_localId);
            }
            get { return _localId; }
        }


        public string name;
        public enum Color { Blue, Red, Green, Yellow };
        public Color playerColor;

        bool alive = true;
        float cooldownTime = 2f;


        public override void OnStartRound()
        {
            base.OnStartRound();
            health = 100;
        }


        float skipDistance = 5f;
        


        public void UpdateEquipItem()
        {
            Item toAttach = itemsInProximity.Find((x) => !x.isEquipped);
            if (toAttach != null)
            {
                Debug.Log("Picking up");
                if (toAttach.GetType() == typeof(Sword))
                {
                    GetLeftHandle().Dettach();
                    GetLeftHandle().Attach(toAttach);
                }
                if (toAttach.GetType() == typeof(Shield))
                {
                    GetRightHandle().Dettach();
                    GetRightHandle().Attach(toAttach);
                }
            }
            else
            {
                Debug.Log("Trying drop");
                GetLeftHandle().Dettach();
                GetRightHandle().Dettach();
            }
        }


        public ItemHandle GetLeftHandle()
        {
            foreach (ItemHandle i in itemHandles)
            {
                if (i.handleType == ItemHandle.HandleType.LeftHand) return i;
            }
            return null;
        }

        public ItemHandle GetRightHandle()
        {
            foreach (ItemHandle i in itemHandles)
            {
                if (i.handleType == ItemHandle.HandleType.RightHand) return i;
            }
            return null;
        }

        public virtual void Awake()
        {
            colorChanger = gameObject.GetComponent<ColorChanger>();
        }



        public virtual bool IsAlive()
        {
            return alive;
        }

        public void changeColor(int id)
        {
            switch (id)
            {
                case 0:
                    playerColor = Color.Blue;
                    setColor(UnityEngine.Color.blue);
                    break;
                case 1:
                    playerColor = Color.Yellow;
                    setColor(UnityEngine.Color.yellow);
                    break;
                case 2:
                    playerColor = Color.Red;
                    setColor(UnityEngine.Color.red);
                    break;
                case 3:
                    playerColor = Color.Green;
                    setColor(UnityEngine.Color.green);
                    break;
            }
        }

        void setColor(UnityEngine.Color baseC)
        {

            colorChanger.SetColor(7, baseC);
            colorChanger.SetColor(2, colorChanger.comp(baseC));
            colorChanger.SetColor(8, UnityEngine.Color.Lerp(baseC, UnityEngine.Color.white, 0.75f));
        }

        

        public override void setMovementStatus(bool allowedToMove)
        {
            PlayerManager.playerManager.playerControllers[localId].allowedToMove = allowedToMove;
        }


        public void UseLeft()
        {
            ItemHandle h = GetLeftHandle();
            if(h != null)
            {
                UseHandle(h);
            }
        }

        public void UseRight()
        {
            ItemHandle h = GetRightHandle();
            if (h != null)
            {
                UseHandle(h);
            }
        }


        public void StopUseRight()
        {
            Debug.Log("JJJJAAAY");
            ItemHandle h = GetRightHandle();
            if (h != null)
            {
                Debug.Log("JJJJAAAY");
                StopUseHandle(h);
            }
        }

        IEnumerator HitTimer(float time)
        {
            yield return new WaitForSeconds(time);
            hitCooldown = false;
        }




        void StartHitCooldown()
        {

            hitCooldown = true;
            StartCoroutine(HitTimer(cooldownTime));
        }

        public override void Hit(Entity hitter, int damage)
        {
            base.Hit(hitter,damage);

        }

      



        public void OnDestroy()
        {
            foreach(ItemHandle itemHandle in itemHandles)
            {
                itemHandle.Dettach();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            GameObject col = collision.collider.gameObject;
            if (col.tag == "Enemy")
            {
                //int damage = col.GetComponent<EnemyController>().damage;
                int damage = 10;
                if (damage > 0)
                {
                    Hit(col.GetComponent<Entity>(), damage);
                }
            }
        }

        public override void Kill()
        {
            base.Kill();
        }

    }
}
