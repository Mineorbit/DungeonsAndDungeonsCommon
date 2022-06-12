namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerSpawn : Spawn
    {

        public Constants.Color color;
        public ColorChanger colorChanger;


        public override void OnInit()
        {
            base.OnInit();
            if (LevelManager.currentLevel.spawn[(int) color] != null &&
                LevelManager.currentLevel.spawn[(int) color] != this)
            {
                LevelManager.currentLevel.Remove(gameObject);
            }
            else
            {
                LevelManager.currentLevel.spawn[(int) color] = this;
                colorChanger.SetColor(0, Constants.ToColor(color));
            }
        }
    }
}