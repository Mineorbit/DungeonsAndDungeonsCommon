using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using General;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PacketBinding", order = 1)]
    public class PacketBinding : ScriptableObject
    {
        public List<string> possiblePacketTypes = new List<string>();
        public List<string> possibleHandlerTypes = new List<string>();
        public List<string> possibleMethods = new List<string>();


        public string PacketType;


        public string HandlerType;


        public List<string> BindedMethods;

        public bool withIdentity;

        public List<MethodInfo> bindedMethods;

        public Type handlerType;

        public Type packetType;

        public UnityAction<Packet> test;

        public class SyncVar : Attribute
        {
        }
    }
}
