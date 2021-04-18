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

        Dictionary<int, bool> regionLoaded = new Dictionary<int, bool>();

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


        public static void LoadRegion(int regionId)
        {
            if (instance.regionLoaded.ContainsKey(regionId) && instance.regionLoaded[regionId]) return;
            string pathToLevel = instance.levelFolder.GetPath()+LevelManager.currentLevelMetaData.localLevelId;
            Debug.Log("Region "+regionId + " loaded at "+pathToLevel);
            SaveManager c = new SaveManager(SaveManager.StorageType.BIN);
            RegionData regionData = c.Load<RegionData>(pathToLevel + "/" + regionId + ".bin");

            List<LevelObjectInstanceData> levelObjectInstances = new List<LevelObjectInstanceData>();

            foreach (ChunkData chunkData in regionData.chunkDatas.Values)
            {
                levelObjectInstances.AddRange(chunkData.levelObjects);
            }

            Dictionary<string, LevelObjectData> levelObjectDatas = LevelObjectData.GetAllBuildableByUniqueType();

            foreach (LevelObjectInstanceData i in levelObjectInstances)
            {
                LevelObjectData d;
                if(levelObjectDatas.TryGetValue(i.type,out d))
                {
                    LevelManager.currentLevel.Add(d,i.GetLocation(), i.GetRotation());
                }
            }

            instance.regionLoaded[regionId] = true;

        }

        public static void LoadAllRegions()
        {
            foreach (int regionId in instance.levelData.regions.Values)
            {
                LoadRegion(regionId);
            }
        }

        public static void ChangeLevelLoading()
        {
            Debug.Log("Load Type changed "+ LevelManager.currentLevel.loadType);
            if (LevelManager.currentLevel.loadType == Level.LoadType.All)
            {
                LoadAllRegions();
            }
            else if (LevelManager.currentLevel.loadType == Level.LoadType.Target)
            {

            }
        }

        void UpdateLiveLevelLoading()
        {
            if(LevelManager.currentLevel != null)
            { 
            if (LevelManager.currentLevel.loadType == Level.LoadType.All)
            {

            }
            else if (LevelManager.currentLevel.loadType == Level.LoadType.Target)
            {

            }
            }
        }

        public void Update()
        {
            UpdateLiveLevelLoading();
        }

        public static void New(LevelMetaData levelMetaData, bool instantiateImmediately = true)
        {
            if(instantiateImmediately)
            { 
            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
			LevelManager.Instantiate();
            }

            instance.levelData = new LevelData();

            instance.regionLoaded = new Dictionary<int, bool>();

            Save(metaData: true, levelData: false);
        }
		public static void New()
        {
            LevelMetaData newMetaData = GetNewLevelMetaData();
			New(newMetaData);
        }
		
		public static void Load(LevelMetaData levelMetaData)
        {
            if (levelMetaData == null) return;

            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();



            SaveManager m = new SaveManager(SaveManager.StorageType.BIN);

            string levelDataPath = instance.levelFolder.GetPath() + levelMetaData.localLevelId + "/Index.json";

            instance.levelData = m.Load<LevelData>(levelDataPath);

            instance.regionLoaded = new Dictionary<int, bool>();


            LoadAllRegions();

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
