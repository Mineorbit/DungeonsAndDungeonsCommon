using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverAudioController : MonoBehaviour
{
    public AudioSource clickSource;

    public AudioSource switchSource;

    public void PlaySwitch()
    {
        switchSource.Play();
    }

    public void PlayerLock()
    {
        clickSource.Play();
    }
}
