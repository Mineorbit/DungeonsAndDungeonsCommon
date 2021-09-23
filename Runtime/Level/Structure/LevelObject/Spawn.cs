using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Spawn : LevelObject
    {
        public override void OnInit()
        {
            SetCollider();
            base.OnInit();
        }
        public void SetCollider()
        {
            GameConsole.Log($"Setting Collider for {gameObject.name}");
            var full_collider = Level.instantiateType == Level.InstantiateType.Play ||
                                Level.instantiateType == Level.InstantiateType.Test ||
                                Level.instantiateType == Level.InstantiateType.Online;
            GetComponent<Collider>().enabled = !full_collider;
            GetComponent<Collider>().isTrigger = full_collider;
        }
    }
}