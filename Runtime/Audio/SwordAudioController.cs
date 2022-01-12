namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwordAudioController : AudioController
    {
        public Sword sword;

        private void Start()
        {
            sword.onUseEvent.AddListener(Swoosh);
            sword.onHitEvent.AddListener(Hit);
            sword.onCollideEvent.AddListener(Collide);
        }

        
        public void Swoosh()
        {
            Play(0);
        }

        public void Hit()
        {
            Play(1);
        }
        
        public void Collide()
        {
            Play(2);
        }
    }
}
