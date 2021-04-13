using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {

		public static LevelMetaData[] localLevels;
		public static LevelMetaData[] networkLevels;

        public static LevelDataManager instance;

        public FileStructureProfile levelFolder;

        public static UnityEvent levelLoadedEvent;

        public void Start()
        {
            if(instance != null)
            {
                Destroy(this);
            }
            instance = this;
            levelLoadedEvent = new UnityEvent();
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
	string folder = instance.levelFolder.GetPath()+currentLevelMetaData.localLevelId
	string path = folder+"/MetaData.json";
	FileManager.createFolder(folder,persistent: false);
	currentLevelMetaData.Save(path);
        }
	}
	
	public static void Delete(LevelMetaData metaData)
	{
	}
		
        public static LevelMetaData GetNewLevelMetaData()
        {
            LevelMetaData levelMetaData = LevelMetaData.CreateInstance<LevelMetaData>();
            levelMetaData.localLevelId = GetFreeLocalId();
            return levelMetaData;
        }

        static int GetFreeLocalId()
        {
            return 1;
        }
		
		public static void UpdateLocalLevelList(){
            Debug.Log("Reading local Levels");
            string path = instance.levelFolder.GetPath();
            string[] levelFolders = Directory.GetDirectories(path);
            localLevels = new LevelMetaData[levelFolders.Length];
            for(int i = 0;i<levelFolders.Length;i++)
            {
                localLevels[i] = LevelMetaData.Load(levelFolders[i]+"MetaData.json");
            }
		}

    }
}
