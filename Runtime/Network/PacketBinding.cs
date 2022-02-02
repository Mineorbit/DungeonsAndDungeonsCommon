using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PacketBinding", order = 1)]
    public class PacketBinding : ScriptableObject
    {

        public class SyncVar : Attribute
        {
        }
    }
}
