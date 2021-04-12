using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FileStructureProfile", order = 1)]
    public class FileStructureProfile : ScriptableObject
    {
        public string name;
        public FileStructureProfile[] subStructures;
    }
}
