namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerAudioController : AudioController
    {
        public PlayerController controller;

        private bool jumped;
        private float walkSoundStrength;

        private void Update()
        {
            walkSoundStrength = (walkSoundStrength + controller.currentSpeed) / 2;
            Blend(0, walkSoundStrength);
            if (controller.IsGrounded && controller.currentSpeed > 0)
            {
                Stop(2);
                Play(0);
            }
            else
            {
                Play(2);
                Stop(0);
            }

            if (!jumped && controller.speedY > 0)
            {
                jumped = true;
                Play(1);
            }

            if (controller.IsGrounded) jumped = false;
        }
    }
}