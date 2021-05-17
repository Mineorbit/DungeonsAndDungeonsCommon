using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class LevelObjectInstanceData
    {
        public string type;


        // the first element will be the primary location of the LevelObject
        public List<Location> locations;
        private float q_w;

        private float q_x;
        private float q_y;
        private float q_z;

        public LevelObjectInstanceData(Vector3 position)
        {
            locations = new List<Location>();
            locations.Add(new Location(position));
        }

        private float x => locations[0].X;


        private float y => locations[0].Y;

        private float z => locations[0].Z;


        public Vector3 GetLocation()
        {
            return locations[0].ToVector();
        }

        public Vector3 GetLocation(int i)
        {
            if (i < locations.Count)
                return locations[i].ToVector();

            return locations[0].ToVector();
        }

        public Quaternion GetRotation()
        {
            return new Quaternion(q_x, q_y, q_z, q_w);
        }

        public void AddLocation(Vector3 position)
        {
            locations.Add(new Location(position));
        }

        public static LevelObjectInstanceData FromInstance(InteractiveLevelObject o)
        {
            Debug.Log("Interactive Save");
            var d = new LevelObjectInstanceData(o.transform.position);
            var rotation = o.transform.rotation;
            d.q_x = rotation.x;
            d.q_y = rotation.y;
            d.q_z = rotation.z;
            d.q_w = rotation.w;
            d.type = o.levelObjectDataType;


            foreach (var receiverLocation in o.receivers.Keys)
            {
                Debug.Log("Adding ReceiverLocation " + receiverLocation);
                d.AddLocation(receiverLocation);
            }

            return d;
        }

        public static LevelObjectInstanceData FromInstance(LevelObject o)
        {
            var d = new LevelObjectInstanceData(o.transform.position);
            var rotation = o.transform.rotation;
            d.q_x = rotation.x;
            d.q_y = rotation.y;
            d.q_z = rotation.z;
            d.q_w = rotation.w;
            d.type = o.levelObjectDataType;


            return d;
        }


        public override string ToString()
        {
            return "Level Object Instance " + type + " " + x + " " + y + " " + z;
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

            public override string ToString()
            {
                return $"({X}, {Y}, {Z})";
            }

            public Vector3 ToVector()
            {
                return new Vector3(X, Y, Z);
            }
        }
    }
}