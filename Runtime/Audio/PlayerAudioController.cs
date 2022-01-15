using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class PlayerAudioController : EntityAudioController
    {
        public Player player;

        private bool jumped;
        private float walkSoundStrength;



        public void Footstep()
        {
            Play(2);
            
            
        }

        public void SwapItem()
        {
            Play(3);
        }
        
        private void Update()
        {
            walkSoundStrength = (walkSoundStrength + player.speed) / 2;
            Blend(0, walkSoundStrength);
            if ((player).isGrounded && player.speed > 0)
            {
                Play(0);
            }
            else
            {
                Stop(0);
            }

            if (!jumped && player.jumping)
            {
                jumped = true;
                Play(1);
            }

            if (((Player) player).isGrounded) jumped = false;
        }
    }
}