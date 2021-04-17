using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [Serializable]
    public class LevelData
    {
        public Dictionary<Tuple<int,int>,int> regions = new Dictionary<Tuple<int, int>, int>();
    }
}
