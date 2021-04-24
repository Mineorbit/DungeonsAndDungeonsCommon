using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class LevelObjectInstanceData
    {

        public LevelObjectInstanceData(Vector3 position)
        {
            locations = new List<Location>();
            locations.Add(new Location(position));
        }

        [Serializable]
        public struct Location
        {
            public Location(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public Location(Vector3 vector)
            {
                X = vector.x;
                Y = vector.y;
                Z = vector.z;
            }

            public float X { get; }
            public float Y { get; }
            public float Z { get; }

            public override string ToString() => $"({X}, {Y}, {Z})";
            public Vector3 ToVector() => new Vector3(X,Y,Z);
        }

        public string type;


        // the first element will be the primary location of the LevelObject
        public List<Location> locations;

        float x
        {
            get
            {
                return locations[0].X;
            }
        }


        float y
        {
            get
            {
                return locations[0].Y;
            }
        }

        float z
        {
            get
            {
                return locations[0].Z;
            }
        }

        float q_x;
        float q_y;
        float q_z;
        float q_w;


        public Vector3 GetLocation()
        {
            return locations[0].ToVector();
        }

        public Vector3 GetLocation(int i)
        {
            if(i < locations.Count)
            return locations[i].ToVector();

            return locations[0].ToVector();
        }

        public Quaternion GetRotation()
        {
            return new Quaternion(q_x,q_y,q_z,q_w);
        }

        public void AddLocation(Vector3 position)
        {
            locations.Add(new Location(position));
        }

        public static LevelObjectInstanceData FromInstance(InteractiveLevelObject o)
        {
            Debug.Log("Interactive Save");
            LevelObjectInstanceData d = new LevelObjectInstanceData(o.transform.position);
            Quaternion rotation = o.transform.rotation;
            d.q_x = rotation.x;
            d.q_y = rotation.y;
            d.q_z = rotation.z;
            d.q_w = rotation.w;
            d.type = o.levelObjectDataType;


            foreach (Vector3 receiverLocation in o.receivers.Keys)
            {
                d.AddLocation(receiverLocation);
            }

            return d;

        }

        public static LevelObjectInstanceData FromInstance(LevelObject o)
        {
            LevelObjectInstanceData d = new LevelObjectInstanceData(o.transform.position);
            Quaternion rotation = o.transform.rotation;
            d.q_x = rotation.x;
            d.q_y = rotation.y;
            d.q_z = rotation.z;
            d.q_w = rotation.w;
            d.type = o.levelObjectDataType;


            return d;

        }

        


        public string ToString()
        {
            return "Object "+type+" "+x+" "+y+" "+z;
        }
    }
}
