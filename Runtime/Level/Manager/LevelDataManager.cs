using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {
        public static LevelDataManager instance;

        public static UnityEvent levelLoadedEvent;
        public static UnityEvent levelListLoadedEvent;


        public static Dictionary<string, LevelObjectData> levelObjectDatas;

        public NetLevel.LevelMetaData[] localLevels;
        public NetLevel.LevelMetaData[] networkLevels;

        public FileStructureProfile levelFolder;

        public LevelData levelData;

        internal LoadType loadType = LoadType.All;

        public void Awake()
        {
            if (instance != null) Destroy(this);


            levelObjectDatas = LevelObjectData.GetAllByUniqueType();

            instance = this;
            levelLoadedEvent = new UnityEvent();
            levelListLoadedEvent = new UnityEvent();

            UpdateLocalLevelList();
        }



        // Save immediately false for example in lobby
        public static void New(NetLevel.LevelMetaData levelMetaData, bool instantiateImmediately = true,
            bool saveImmediately = true)
        {
            if (instantiateImmediately)
            {
                LevelManager.Clear();
                LevelManager.currentLevelMetaData = levelMetaData;
                LevelManager.Instantiate();
            }

            instance.levelData = new LevelData();

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();
            if (saveImmediately)
                Save(true, false);
        }

        public static void New(bool saveImmediately = true)
        {
            var newMetaData = GetNewLevelMetaData();
            New(newMetaData, saveImmediately: saveImmediately);
        }


        public static void Load(NetLevel.LevelMetaData levelMetaData, Level.InstantiateType instantiateType)
        {
            if (levelMetaData == null) return;

            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();
            Level.instantiateType = instantiateType;


            var m = new SaveManager(SaveManager.StorageType.BIN);

            var levelDataPath = instance.levelFolder.GetPath() + levelMetaData.LocalLevelId + "/Index.json";

            instance.levelData = m.Load<LevelData>(levelDataPath);

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();


            LoadAllRegions();

            LevelManager.currentLevel.GenerateNavigation(true);

            levelLoadedEvent.Invoke();
        }


        private static void SaveLevelData(string pathToLevel)
        {
            var regions = ChunkManager.instance.FetchRegionData();
            instance.levelData = new LevelData();

            var c = new SaveManager(SaveManager.StorageType.BIN);

            foreach (var region in regions)
            {
                instance.levelData.regions.Add(region.Key, region.Value.id);
                c.Save(region.Value, pathToLevel + "/" + region.Value.id + ".bin", false);
            }

            c.Save(instance.levelData, pathToLevel + "/Index.json", false);
        }

        public static void Save(bool metaData = true, bool levelData = true)
        {
            var currentLevelMetaData = LevelManager.currentLevelMetaData;
            if (currentLevelMetaData != null)
            {
                var folder = instance.levelFolder.GetPath() + currentLevelMetaData.LocalLevelId;
                var metaDataPath = folder + "/MetaData.json";
                FileManager.createFolder(folder, false);
                if (metaData) SaveLevelMetaData(currentLevelMetaData,metaDataPath);
                if (levelData) SaveLevelData(folder);
            }


            UpdateLocalLevelList();
        }


        public static void Delete(NetLevel.LevelMetaData metaData)
        {
            if (metaData == null) return;
            var levelPath = instance.levelFolder.GetPath() + metaData.LocalLevelId;
            if (Directory.Exists(levelPath)) Directory.Delete(levelPath, true);
            UpdateLocalLevelList();
        }

        
        
        public static NetLevel.LevelMetaData LoadLevelMetaData(string path)
        {
            // Add file safety check
            var metaData = ProtoSaveManager.Load<NetLevel.LevelMetaData>(path);
            return metaData;
        }

        public static void SaveLevelMetaData(NetLevel.LevelMetaData metaData,string path)
        {
            metaData.AvailBlue = LevelManager.currentLevel.spawn[0] != null;
            metaData.AvailYellow = LevelManager.currentLevel.spawn[1] != null;
            metaData.AvailRed = LevelManager.currentLevel.spawn[2] != null;
            metaData.AvailGreen = LevelManager.currentLevel.spawn[3] != null;

            ProtoSaveManager.Save(path,metaData);
        }
        
        public static NetLevel.LevelMetaData GetNewLevelMetaData()
        {
            var levelMetaData = new NetLevel.LevelMetaData();
            levelMetaData.FullName = "NewLevelMetaData";
            levelMetaData.LocalLevelId = GetFreeLocalId();
            return levelMetaData;
        }

        private static int GetFreeLocalId()
        {
            UpdateLocalLevelList();
            if (instance != null || instance.localLevels != null)
            {
                var localLevelIds = instance.localLevels.Select(x => x.LocalLevelId);
                var i = 0;
                while (localLevelIds.Contains(i))
                    i++;
                return i;
            }

            return -1;
        }

        
        
        public static void UpdateLocalLevelList()
        {
            if (instance == null || instance.levelFolder == null)
            {
                Debug.Log("Level Folder not set for LevelDataManager or instance not set");
                return;
            }

            var path = instance.levelFolder.GetPath();
            Debug.Log("Looking for local levels in " + path);
            var levelFolders = Directory.GetDirectories(path);
            instance.localLevels = new NetLevel.LevelMetaData[levelFolders.Length];
            for (var i = 0; i < levelFolders.Length; i++)
                instance.localLevels[i] = LoadLevelMetaData(levelFolders[i] + "/MetaData.json");
            levelListLoadedEvent.Invoke();
        }

        
        
        private static void LoadAllRegions()
        {
            foreach (var regionId in instance.levelData.regions.Values) ChunkManager.LoadRegion(regionId);
        }

        
        internal enum LoadType
        {
            All,
            Near
        }
    }
}