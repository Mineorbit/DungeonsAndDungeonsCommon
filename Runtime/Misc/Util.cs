using System;
using System.Security.Policy;
using NetLevel;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Util
    {
        
        public static Func<Location, Vector3> LocationToVector = (l) => { return new Vector3(l.X, l.Y, l.Z);};
        public static Func<Vector3, Location> VectorToLocation = (v) => { var l = new Location();
            l.X = v.x;
            l.Y = v.y;
            l.Z = v.z;
            return l;
        };

    }
}