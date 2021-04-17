using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Linq;
using System;

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


    static void SaveLevelData(string pathToLevel)
    {
            Dictionary<Tuple<int, int>, RegionData> regions = ChunkManager.instance.FetchRegionData();
            LevelData levelData = new LevelData();

            SaveManager m = new SaveManager(SaveManager.StorageType.JSON);
            foreach (KeyValuePair<Tuple<int, int>, RegionData> region in regions)
            {
                levelData.regions.Add(region.Key,region.Value.id);
                m.Save(region.Value, pathToLevel+"/"+region.Value.id+".json", persistent: false);
            }

            m.Save(levelData, pathToLevel + "/Index.json", persistent: false);

    }

    public static void Save()
    {

	    LevelMetaData currentLevelMetaData = LevelManager.currentLevelMetaData;
	    if(currentLevelMetaData != null)
	    {
	    string folder = instance.levelFolder.GetPath()+currentLevelMetaData.localLevelId;
	    string metaDataPath = folder+"/MetaData.json";
	    FileManager.createFolder(folder,persistent: false);
	    currentLevelMetaData.Save(metaDataPath);
        SaveLevelData(folder);
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
