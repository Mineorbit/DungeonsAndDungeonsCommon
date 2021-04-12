using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
namespace com.mineorbit.dungeonsanddungeonscommon
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/FileStructureProfile", order = 1)]
    public class FileStructureProfile : ScriptableObject
    {
        public string name;
        public FileStructureProfile parent;
        public List<FileStructureProfile> subStructures;

        TaskCompletionSource<bool> listReady = new TaskCompletionSource<bool>();
        
        void OnEnable()
        {
            EnterInParent();
        }
        async void EnterInParent()
        {
            subStructures = new List<FileStructureProfile>();
            listReady.SetResult(true);
            if (parent != null)
            {
            await parent.listReady.Task;
            parent.subStructures.Add(this);
            }
        }

        public string GetPath()
        {
            if(parent == null)
            {
                return Application.persistentDataPath + "/" + name + "/";
            }else
            {
                return parent.GetPath() + name + "/";
            }
        }

       
    }
}