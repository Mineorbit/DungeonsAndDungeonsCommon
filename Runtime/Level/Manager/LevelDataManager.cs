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

        public LevelData levelData;

        internal enum LoadType { All, Near };

        internal LoadType loadType = LoadType.All;

        public void Awake()
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




        static void LoadAllRegions()
        {
            foreach (int regionId in instance.levelData.regions.Values)
            {
                ChunkManager.LoadRegion(regionId);
            }
        }

        static void ChangeLevelLoading()
        {
            if (instance.loadType == LoadType.All)
            {
                LoadAllRegions();
            }
            else 
            if (instance.loadType == LoadType.Near)
            {
                //At least one load Target
            }
        }

        // Save immediately false for example in lobby
        public static void New(LevelMetaData levelMetaData, bool instantiateImmediately = true, bool saveImmediately = true)
        {
            if(instantiateImmediately)
            { 
            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
			LevelManager.Instantiate();
            }

            instance.levelData = new LevelData();

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();
            if(saveImmediately)
            Save(metaData: true, levelData: false);
        }
		public static void New(bool saveImmediately = true)
        {
            LevelMetaData newMetaData = GetNewLevelMetaData();
			New(newMetaData,saveImmediately: saveImmediately);
        }
		
		public static void Load(LevelMetaData levelMetaData, Level.InstantiateType instantiateType)
        {
            if (levelMetaData == null) return;

            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();
            Level.instantiateType = instantiateType;



            SaveManager m = new SaveManager(SaveManager.StorageType.BIN);

            string levelDataPath = instance.levelFolder.GetPath() + levelMetaData.localLevelId + "/Index.json";

            instance.levelData = m.Load<LevelData>(levelDataPath);

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();


            LoadAllRegions();

            LevelManager.currentLevel.GenerateNavigation(force: true);

            levelLoadedEvent.Invoke();
        }


    static void SaveLevelData(string pathToLevel)
    {
            Dictionary<Tuple<int, int>, RegionData> regions = ChunkManager.instance.FetchRegionData();
            instance.levelData = new LevelData();

            SaveManager c = new SaveManager(SaveManager.StorageType.BIN);

            foreach (KeyValuePair<Tuple<int, int>, RegionData> region in regions)
            {
                instance.levelData.regions.Add(region.Key,region.Value.id);
                c.Save(region.Value, pathToLevel+"/"+region.Value.id+".bin", persistent: false);
            }

            c.Save(instance.levelData, pathToLevel + "/Index.json", persistent: false);

    }

    public static void Save(bool metaData = true, bool levelData = true)
    {

	    LevelMetaData currentLevelMetaData = LevelManager.currentLevelMetaData;
	    if(currentLevelMetaData != null)
	    {
	    string folder = instance.levelFolder.GetPath()+currentLevelMetaData.localLevelId;
	    string metaDataPath = folder+"/MetaData.json";
	    FileManager.createFolder(folder,persistent: false);
        if(metaData) currentLevelMetaData.Save(metaDataPath);
        if(levelData) SaveLevelData(folder);
        }


        UpdateLocalLevelList();
	}

	
	    public static void Delete(LevelMetaData metaData)
	    {
            if (metaData == null) return;
            string levelPath = instance.levelFolder.GetPath() + metaData.localLevelId.ToString();
            if (Directory.Exists(levelPath))
            {
                Directory.Delete(levelPath,true);
            }
            UpdateLocalLevelList();
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
            if (instance != null || instance.localLevels != null)
            { 
            var localLevelIds = instance.localLevels.Select( x=> x.localLevelId);
            int i = 0;
            while (localLevelIds.Contains(i))
                i++;
            return i;
            }
            return -1;
        }
		
		public static void UpdateLocalLevelList(){
            if(instance == null || instance.levelFolder == null)
            {
                Debug.Log("Level Folder not set for LevelDataManager or instance not set");
                return;
            }
            string path = instance.levelFolder.GetPath();
            Debug.Log("Looking for local levels in "+path);
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
