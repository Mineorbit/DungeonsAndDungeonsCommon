using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Google.Protobuf;
using General;
using System.Reflection;
using System.Linq.Expressions;

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



        public Type packetType;

        public Type handlerType;

        public List<MethodInfo> bindedMethods;

        public UnityAction<Packet> test;
        public class Binding : Attribute
        {

        }


        void OnValidate()
        {



        possiblePacketTypes = new List<string>();
        possibleHandlerTypes = new List<string>();
        possibleMethods = new List<string>();

        List<Type> types = (
                    from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                        // alternative: from domainAssembly in domainAssembly.GetExportedTypes()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(IMessage).IsAssignableFrom(assemblyType)
                    // alternative: where assemblyType.IsSubclassOf(typeof(B))
                    // alternative: && ! assemblyType.IsAbstract
                    select assemblyType).ToList();


            List<Type> networkHandlerTypes = (
                    from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                        // alternative: from domainAssembly in domainAssembly.GetExportedTypes()
                    from assemblyType in domainAssembly.GetTypes()
                    where typeof(NetworkHandler).IsAssignableFrom(assemblyType)
                    // alternative: where assemblyType.IsSubclassOf(typeof(B))
                    // alternative: && ! assemblyType.IsAbstract
                    select assemblyType).ToList();




            possiblePacketTypes.AddRange(types.Select((x) => { return x.FullName; }).ToList());
            possibleHandlerTypes.AddRange(networkHandlerTypes.Select((x) => { return x.FullName; }).ToList());

            packetType = Type.GetType(PacketType);
            handlerType = Type.GetType(HandlerType);

            if(packetType != null && handlerType != null)
            { 
            var methods = handlerType.GetMethods()
                      .Where(m => m.GetCustomAttributes(typeof(Binding), false).Length > 0)
                      .ToList();

            possibleMethods.AddRange(methods.Select((x) => { return x.Name; }).ToList());

            bindedMethods = BindedMethods.Select((x) => { return handlerType.GetMethod(x); }).ToList();
            }
        }


        static Delegate CreateMethod(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }

            if (!method.IsStatic)
            {
                throw new ArgumentException("The provided method must be static.", "method");
            }

            if (method.IsGenericMethod)
            {
                throw new ArgumentException("The provided method must not be generic.", "method");
            }

            var parameters = method.GetParameters()
                                   .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                                   .ToArray();
            var call = Expression.Call(null, method, parameters);
            return Expression.Lambda(call, parameters).Compile();
        }



        void AddToBinding()
        {
            if (bindedMethods != null && bindedMethods.Count > 0)
            {
                Debug.Log("Trying Binding");
                foreach (MethodInfo methodInfo in bindedMethods)
                {
                    Debug.Log("Binding for " + methodInfo.Name);

                    UnityAction<Packet> action = (x) => { CreateMethod(methodInfo).DynamicInvoke(x); };
                    Tuple<Type, Type> key = new Tuple<Type, Type>(packetType, handlerType);
                    if (!NetworkHandler.globalMethodBindings.ContainsKey(key))
                        NetworkHandler.globalMethodBindings.Add(key, action);
                }
            }
        }

        public void Awake()
        {
            AddToBinding();
        }

        public void OnEnable()
        {
            AddToBinding();
        }

    }
}
