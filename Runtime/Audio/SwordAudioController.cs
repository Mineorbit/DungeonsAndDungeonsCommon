namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class SwordAudioController : ItemAudioController
    {
    
    public Sword GetItem()
    {
        return (Sword) item;
    }

        public override void Start()
        {
            base.Start();
            GetItem().onUseEvent.AddListener(Swoosh);
            GetItem().onHitEvent.AddListener(Hit);
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
