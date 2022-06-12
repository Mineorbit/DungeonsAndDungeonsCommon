using System;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class Constants
    {
    //Properties
        

        
        public enum Color
        {
            Blue = 0,
            Red = 1,
            Green = 2,
            Yellow = 3
            
        }

        
        private static UnityEngine.Color[] colors = new[] { 
            UnityEngine.Color.blue,
            UnityEngine.Color.red, 
            UnityEngine.Color.green,
            UnityEngine.Color.yellow};
        
        public static UnityEngine.Color ToColor(Color c)
        {
            if((int) c < colors.Length)
                return colors[(int) c];
            return UnityEngine.Color.white;
        }

    }
}