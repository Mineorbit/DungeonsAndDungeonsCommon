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

        public LevelMetaData[] localLevels;
        public LevelMetaData[] networkLevels;

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


        private static void LoadAllRegions()
        {
            foreach (var regionId in instance.levelData.regions.Values) ChunkManager.LoadRegion(regionId);
        }


        // Save immediately false for example in lobby
        public static void New(LevelMetaData levelMetaData, bool instantiateImmediately = true,
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


        public static void Load(LevelMetaData levelMetaData, Level.InstantiateType instantiateType)
        {
            if (levelMetaData == null) return;

            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();
            Level.instantiateType = instantiateType;


            var m = new SaveManager(SaveManager.StorageType.BIN);

            var levelDataPath = instance.levelFolder.GetPath() + levelMetaData.localLevelId + "/Index.json";

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
                var folder = instance.levelFolder.GetPath() + currentLevelMetaData.localLevelId;
                var metaDataPath = folder + "/MetaData.json";
                FileManager.createFolder(folder, false);
                if (metaData) currentLevelMetaData.Save(metaDataPath);
                if (levelData) SaveLevelData(folder);
            }


            UpdateLocalLevelList();
        }


        public static void Delete(LevelMetaData metaData)
        {
            if (metaData == null) return;
            var levelPath = instance.levelFolder.GetPath() + metaData.localLevelId;
            if (Directory.Exists(levelPath)) Directory.Delete(levelPath, true);
            UpdateLocalLevelList();
        }

        public static LevelMetaData GetNewLevelMetaData()
        {
            var levelMetaData = new LevelMetaData("NewMetaData");
            levelMetaData.localLevelId = GetFreeLocalId();
            return levelMetaData;
        }

        private static int GetFreeLocalId()
        {
            UpdateLocalLevelList();
            if (instance != null || instance.localLevels != null)
            {
                var localLevelIds = instance.localLevels.Select(x => x.localLevelId);
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
            instance.localLevels = new LevelMetaData[levelFolders.Length];
            for (var i = 0; i < levelFolders.Length; i++)
                instance.localLevels[i] = LevelMetaData.Load(levelFolders[i] + "/MetaData.json");
            levelListLoadedEvent.Invoke();
        }

        internal enum LoadType
        {
            All,
            Near
        }
    }
}