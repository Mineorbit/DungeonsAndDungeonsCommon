namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwordAudioController : ItemAudioController
    {
        public Sword item;

        public override void Start()
        {
            base.Start();
            item.onUseEvent.AddListener(Swoosh);
            item.onHitEvent.AddListener(Hit);
        }

        
        public void Swoosh()
        {
            Play(0);
        }

        public void Hit()
        {
            Play(1);
        }
        
        
    }
}
