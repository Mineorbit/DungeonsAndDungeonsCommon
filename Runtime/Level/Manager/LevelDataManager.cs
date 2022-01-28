using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetLevel;
using UnityEngine;
using UnityEngine.Events;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class LevelDataManager : MonoBehaviour
    {
        public static LevelDataManager instance;

        public static UnityEvent levelLoadedEvent;
        public static UnityEvent levelListUpdatedEvent;


        public static Dictionary<int, LevelObjectData> levelObjectDatas;

        public LevelMetaData[] localLevels;
        public LevelMetaData[] networkLevels;

        public FileStructureProfile levelFolder;

        public LevelData levelData;

        internal LoadType loadType = LoadType.All;

        public void Awake()
        {
            if (instance != null) Destroy(this);



            instance = this;
            levelObjectDatas = LevelObjectData.GetAllByUniqueType();
            levelLoadedEvent = new UnityEvent();
            levelListUpdatedEvent = new UnityEvent();

            UpdateLocalLevelList();
        }



        // Save immediately false for example in lobby
        public static void New(LevelMetaData levelMetaData, bool instantiateImmediately = true,
            bool saveImmediately = true)
        {
            //Moved for now
            GameConsole.Log("Creating new level "+levelMetaData);
            if (instantiateImmediately)
            {
                LevelManager.Clear();
                LevelManager.currentLevelMetaData = levelMetaData;
                LevelManager.Instantiate();
            }
            

            instance.levelData = new LevelData();

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();
            if (saveImmediately)
                SaveLevelMetaData(LevelManager.currentLevelMetaData,true);
            
            
        }
        public static void Load(NetLevel.LevelMetaData levelMetaData, Level.InstantiateType instantiateType)
        {
            if (levelMetaData == null) return;

            LevelManager.Clear();
            LevelManager.currentLevelMetaData = levelMetaData;
            LevelManager.Instantiate();
            Level.instantiateType = instantiateType;


            var m = new SaveManager(SaveManager.StorageType.BIN);

            var levelDataPath = GetLevelPath(levelMetaData) + "/Index.json";
            GameConsole.Log("Loading LevelData: "+levelDataPath);
            instance.levelData = m.Load<LevelData>(levelDataPath);

            ChunkManager.instance.regionLoaded = new Dictionary<int, bool>();


            LoadAllRegions();

            LevelManager.currentLevel.GenerateNavigation(true);

            levelLoadedEvent.Invoke();
        }


        static void SaveLevelData(string pathToLevel)
        {
            var regions = ChunkManager.instance.FetchRegionData();
            instance.levelData = new LevelData();

            var c = new SaveManager(SaveManager.StorageType.BIN);

            foreach (var region in regions)
            {
                instance.levelData.regions.Add(region.Key, region.Value.Id);
                ProtoSaveManager.Save( pathToLevel + "/" + region.Value.Id + ".bin", region.Value);
            }

            c.Save(instance.levelData, pathToLevel + "/Index.json", false);
        }

        private static float disableTime = 0.1f;
        public static Canvas[] canvases;
        public static bool[] enablings = new bool[32];
        public GameObject cursorGameObject;
        private bool cursorGameObjectEnabled;
        public static void Save(bool metaData = true, bool levelData = true, LevelMetaData extraSaveMetaData = null)
        {
            canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            int i = 0;
            foreach (Canvas c in canvases)
            {
                enablings[i] = c.gameObject.activeSelf;
                c.gameObject.SetActive(false);
                i++;
            }

            instance.cursorGameObjectEnabled = instance.cursorGameObject.activeSelf;
            instance.cursorGameObject.SetActive(false);
            
            var currentLevelMetaData = LevelManager.currentLevelMetaData;
            
            if (currentLevelMetaData != null)
            {
                var metaDataPath = GetLevelPath(currentLevelMetaData);
                FileManager.createFolder(metaDataPath, false);
                if (metaData) SaveLevelMetaData(currentLevelMetaData,metaDataPath + "/MetaData.json");
                
                ScreenCapture.CaptureScreenshot(metaDataPath+"/Thumbnail.png");
                if (levelData) SaveLevelData(metaDataPath);
            }
            
            if (extraSaveMetaData != null)
            {
                var metaDataPath = GetLevelPath(extraSaveMetaData);
                FileManager.createFolder(GetLevelPath(extraSaveMetaData), false);
                if (metaData) SaveLevelMetaData(extraSaveMetaData,metaDataPath + "/MetaData.json");
                ScreenCapture.CaptureScreenshot(metaDataPath+"/Thumbnail.png");
            }
            UpdateLocalLevelList();
            instance.Invoke("ResetCanvases", disableTime);
        }


        public void ResetCanvases()
        {
            int i = 0;
            foreach (Canvas c in canvases)
            {
                c.gameObject.SetActive(enablings[i]);
                i++;
            }

            instance.cursorGameObject.SetActive(cursorGameObjectEnabled);
        }
        
        public static void Delete(NetLevel.LevelMetaData metaData)
        {
            if (metaData == null) return;
            var levelPath = GetLevelPath(metaData);
            if (Directory.Exists(levelPath)) Directory.Delete(levelPath, true);
            UpdateLocalLevelList();
        }

        
        
        static NetLevel.LevelMetaData LoadLevelMetaData(string path)
        {
            // Add file safety check
            var metaData = ProtoSaveManager.Load<NetLevel.LevelMetaData>(path);
            return metaData;
        }

        public static void SaveLevelMetaData(NetLevel.LevelMetaData metaData,string path)
        {
            if(LevelManager.currentLevel != null)
            {
                metaData.AvailBlue = LevelManager.currentLevel.spawn[0] != null;
                metaData.AvailYellow = LevelManager.currentLevel.spawn[1] != null;
                metaData.AvailRed = LevelManager.currentLevel.spawn[2] != null;
                metaData.AvailGreen = LevelManager.currentLevel.spawn[3] != null;
            }
            ProtoSaveManager.Save(path,metaData);
        }


        public static string SetupNewLevelFolder(LevelMetaData metaData)
        {
            metaData.LocalLevelId = GetFreeLocalLevelId();
            string lPath = GetLevelPath(metaData);
            FileManager.createFolder(lPath,persistent: false);
            return lPath;
        }
        public static void SaveLevelMetaData(LevelMetaData metaData, bool newLevel = false)
        {
            string path = "";
            if (newLevel)
            {
                path = SetupNewLevelFolder(metaData)+"/MetaData.json";
            }
            else
            {
                path = GetLevelPath(metaData)+"/MetaData.json";;
            }
            SaveLevelMetaData(metaData,path);
        }
        
        public static NetLevel.LevelMetaData GetNewLevelMetaData()
        {
            var levelMetaData = new LevelMetaData();
            levelMetaData.FullName = "NewLevelMetaData";
            return levelMetaData;
        }

        public static int GetFreeLocalLevelId()
        {
            int id = 0;
            if (instance != null || instance.localLevels != null)
            {
                var localLevelIds = instance.localLevels.Select(x => x.LocalLevelId);
                id = 0;
                while (localLevelIds.Contains(id))
                    id++;
            }
            else
            {
                id = -1;
            }

            return id;
        }
        

        public static string GetLevelPath(LevelMetaData levelMetaData)
        {
            string path = instance.levelFolder.GetPath() + levelMetaData.FullName + ((levelMetaData.LocalLevelId != 0) ? levelMetaData.LocalLevelId.ToString() : "");
            return path;
        }

        
        
        public static void UpdateLocalLevelList()
        {
            if (instance == null || instance.levelFolder == null)
            {
                GameConsole.Log("Level Folder not set for LevelDataManager or instance not set");
                return;
            }

            var path = instance.levelFolder.GetPath();
            GameConsole.Log("Looking for local levels in " + path);
            var levelFolders = Directory.GetDirectories(path);
            instance.localLevels = new NetLevel.LevelMetaData[levelFolders.Length];
            for (var i = 0; i < levelFolders.Length; i++)
            {
                GameConsole.Log($"Loading {levelFolders[i]}");
                instance.localLevels[i] = LoadLevelMetaData(levelFolders[i] + "/MetaData.json");
                
            }
            levelListUpdatedEvent.Invoke();
        }

        
        
        private static void LoadAllRegions()
        {
            if(instance.levelData != null)
            {
                foreach (var regionId in instance.levelData.regions.Values) ChunkManager.LoadRegion(regionId);
            }
            else
            {
                GameConsole.Log("There currently is no LevelData to load");
            }
        }

        
        internal enum LoadType
        {
            All,
            Near
        }
    }
}