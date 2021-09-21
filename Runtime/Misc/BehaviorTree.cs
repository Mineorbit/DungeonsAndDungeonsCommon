using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorTree : MonoBehaviour
{
    public enum Response
    {
        Success,
        Failure,
        Running
    }

    public abstract class Node
    {
        public Node[] children;

        public abstract Response Tick();
    }

    public class SelectorNode : Node
    {
        public override Response Tick()
        {
            Response childStatus;
            for (int i = 0; i < children.Length; i++)
            {
                childStatus = children[i].Tick();
                if (childStatus == Response.Running)
                {
                    return Response.Running;
                }else if (childStatus == Response.Success)
                {
                    return Response.Success;
                }
            }

            return Response.Failure;
        }
    }
    
    public class SequenceNode : Node
    {
        public override Response Tick()
        {
            Response childStatus;
            for (int i = 0; i < children.Length; i++)
            {
                childStatus = children[i].Tick();
                if (childStatus == Response.Running)
                {
                    return Response.Running;
                }else if (childStatus == Response.Failure)
                {
                    return Response.Failure;
                }
            }

            return Response.Success;
        }
    }
}