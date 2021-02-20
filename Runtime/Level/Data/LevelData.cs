using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelData : ScriptableObject
    {
        Dictionary<Tuple<int,int>, RegionData> regions = new Dictionary<Tuple<int, int>, RegionData>();
    }
}
