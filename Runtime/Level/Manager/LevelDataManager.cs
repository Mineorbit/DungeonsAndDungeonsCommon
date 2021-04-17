using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {

		public LevelMetaData[] localLevels;
		public LevelMetaData[] networkLevels;

        public static LevelDataManager instance;

        public FileStructureProfile levelFolder;

        public static UnityEvent levelLoadedEvent;
        public static UnityEvent levelListLoadedEvent;

        public void Start()
        {
            if(instance != null)
            {
                Destroy(this);
            }
            instance = this;
            levelLoadedEvent = new UnityEvent();
            levelListLoadedEvent = new UnityEvent();

            UpdateLocalLevelList();
        }

       

        public static void New(LevelMetaData levelMetaData, bool instantiateImmediately = true)
        {
            if(instantiateImmediately)
            { 
            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
			LevelManager.Instantiate();
            }
            Save();
        }
		public static void New()
        {
            LevelMetaData newMetaData = GetNewLevelMetaData();
			New(newMetaData);
        }
		
		public static void Load(LevelMetaData levelMetaData)
        {
            if (levelMetaData == null) return;
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();


            levelLoadedEvent.Invoke();
        }
		
		
		
    public static void Save()
    {

	    LevelMetaData currentLevelMetaData = LevelManager.currentLevelMetaData;
	    if(currentLevelMetaData != null)
	    {
	    string folder = instance.levelFolder.GetPath()+currentLevelMetaData.localLevelId;
	    string path = folder+"/MetaData.json";
	    FileManager.createFolder(folder,persistent: false);
	    currentLevelMetaData.Save(path);
        }
            UpdateLocalLevelList();
	}
	
	public static void Delete(LevelMetaData metaData)
	{
	}
		
        public static LevelMetaData GetNewLevelMetaData()
        {
            LevelMetaData levelMetaData = new LevelMetaData("NewMetaData");
            levelMetaData.localLevelId = GetFreeLocalId();
            return levelMetaData;
        }

        static int GetFreeLocalId()
        {
            UpdateLocalLevelList();
            var localLevelIds = instance.localLevels.Select( x=> x.localLevelId);
            int i = 0;
            while (localLevelIds.Contains(i))
                i++;
            return i;
        }
		
		public static void UpdateLocalLevelList(){
            string path = instance.levelFolder.GetPath();
            string[] levelFolders = Directory.GetDirectories(path);
            instance.localLevels = new LevelMetaData[levelFolders.Length];
            for(int i = 0;i<levelFolders.Length;i++)
            {
                instance.localLevels[i] = LevelMetaData.Load(levelFolders[i]+"/MetaData.json");
            }
            levelListLoadedEvent.Invoke();
		}

    }
}
