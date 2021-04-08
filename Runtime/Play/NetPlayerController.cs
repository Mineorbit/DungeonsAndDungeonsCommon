﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    //Only in state Play (Online Play)
    public class NetPlayerController : PlayerController
    {

        NetPlayer player;

        void Update()
        {
            UpdateGround();
            PlayUpdate();
            Move();
        }

        void PlayUpdate()
        {
            doSim = isMe && !player.lockNetUpdate;
            doInput = PlayerManager.acceptInput && allowedToMove && !player.lockNetUpdate;
        }

    }
}
