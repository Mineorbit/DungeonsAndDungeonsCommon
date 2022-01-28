using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Google.Protobuf;
using NetLevel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Net;
using UnityEngine.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;


namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class HttpManager : MonoBehaviour
    {
        public Option levelServerURL;

        public string baseURL;
        public static HttpManager instance;

        public FileStructureProfile compressedLevelFiles;
        public FileStructureProfile levelFolders;

        private string token;
        static int port = 8000;

        class TokenData
        {
            public string access_token;
            string token_type;
        }

        private void Start()
        {
            if (instance != null)
                Destroy(this);
            instance = this;
            baseURL = (string) levelServerURL.Value;
            Login("Max", "Test123");
        }

        private IEnumerator LoginRoutine(string username, string password)
        {
            var url = baseURL + $":{port}/auth/token";
            var form = new WWWForm();
            form.AddField("grant_type", "");
            form.AddField("username", username);
            form.AddField("password", password);
            form.AddField("scope", "");
            form.AddField("client_id", "");
            form.AddField("client_secret", "");
            using (var www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    GameConsole.Log(www.error);
                else
                {
                    GameConsole.Log(www.downloadHandler.text);
                    TokenData t = JsonUtility.FromJson<TokenData>(www.downloadHandler.text);
                    token = t.access_token;
                }
            }
        }

        public void Login(string username, string password)
        {
            StartCoroutine(LoginRoutine(username, password));
        }


        
        public IEnumerator DownloadImage(int ulid,RawImage image)
        {
            string url = baseURL + $":{port}/level/pic/?proto_resp=false&ulid={ulid}";
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if(request.isNetworkError || request.isHttpError) 
                GameConsole.Log(request.error);
            else
                image.texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
        }

        private float disableTime = 0.1f;
        private Queue<Tuple<Canvas,bool>> cEnabled;
        private IEnumerator UploadLevel(NetLevel.LevelMetaData levelToUpload, string path, UnityAction<string> action)
        {

            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            cEnabled = new Queue<Tuple<Canvas, bool>>();
            foreach (Canvas c in canvases)
            {
                cEnabled.Enqueue(new Tuple<Canvas, bool>(c,c.enabled));
                c.enabled = false;
            }
            var url = baseURL +
                      $":{port}/level/?proto_resp=true&name={levelToUpload.FullName}&description={levelToUpload.Description}" +
                      $"&r={levelToUpload.AvailRed}&g={levelToUpload.AvailGreen}&b={levelToUpload.AvailBlue}&y={levelToUpload.AvailYellow}";




            var form = new WWWForm();
            var fileByte = File.ReadAllBytes(path);
            string tpath = LevelDataManager.GetLevelPath(levelToUpload) + "/Thumbnail.png";
            var tnByte = File.ReadAllBytes(tpath);
            form.AddBinaryData("thumbnail",tnByte, "thumbnail.png","image / png");
            form.AddBinaryData("levelFiles", fileByte, levelToUpload.UniqueLevelId + ".zip", "application / zip");
            
            action.Invoke("Uploading Level to "+url);
            using (var www = UnityWebRequest.Post(url, form))
            {
                www.SetRequestHeader("Authorization", $"Bearer {token}");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    action.Invoke(www.error);
                else
                    action.Invoke(www.downloadHandler.text);
            }
            
            Invoke("ResetCanvases",disableTime);
        }

        public void ResetCanvases()
        {
            while (cEnabled.Count > 0)
            {
                Tuple<Canvas,bool> b = cEnabled.Dequeue();
                b.Item1.enabled = b.Item2;
            }
        }
        
        public LevelMetaData[] levelMetaDatas;

        private IEnumerator FetchLevelList(UnityEvent<string> reportAction, UnityEvent listUpdatedEvent)
        {
            var uri = baseURL + $":{port}/level/all?proto_resp=true";


            reportAction.Invoke("Loading Level List");
            using (var www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                    reportAction.Invoke(www.error);
                else
                {
                    string data = www.downloadHandler.text;
                    GameConsole.Log($"Received {data}");
                    LevelMetaDataList list = LevelMetaDataList.Parser.ParseFrom(ByteString.CopyFromUtf8(data));
                    levelMetaDatas = list.Levels.ToArray();
                    GameConsole.Log($"Got list: {levelMetaDatas.Length}");
                    foreach (var x in levelMetaDatas)
                    {
                        GameConsole.Log("MetaData: " + x);
                    }

                    listUpdatedEvent.Invoke();
                }



            }
        }



        private string AssembleZip(string resultPath, string targetPath)
        {
            //AlertScreen.alert.Open("Removing old File");
            File.Delete(resultPath);
            //AlertScreen.alert.Open("Compressing Level");

            try
            {
                ZipFile.CreateFromDirectory(targetPath, resultPath,
                    CompressionLevel.Optimal, false);

            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }

            return resultPath;
        }

        private string DisassembleZip(string targetPath, string resultPath)
        {
            try
            {
                using (FileStream zipToOpen = new FileStream(targetPath, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries)
                        {
                            string path = Path.Combine(resultPath,zipArchiveEntry.FullName);
                            GameConsole.Log($"Extracting {path}");
                            zipArchiveEntry.ExtractToFile(path,true);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            return resultPath;
        }



        public static void StartUpload(NetLevel.LevelMetaData levelToUpload)
        {
            
            
            count++;
            var path = instance.AssembleZip(
                instance.compressedLevelFiles.GetPath() + "" + count + ".zip",
                LevelDataManager.GetLevelPath(levelToUpload));
            
            
            GameConsole.Log($"Uploading {levelToUpload} {LevelDataManager.GetLevelPath(levelToUpload)}");
            instance.StartCoroutine(instance.UploadLevel(levelToUpload,
                instance.compressedLevelFiles.GetPath() + "" + count + ".zip",
                (x) => { GameConsole.Log(x); }));
        }

        public static void FetchLevelList(UnityEvent listUpdatedEvent)
        {
            Debug.Log("Fetching level list");
            UnityEvent<string> a = new UnityEvent<string>();
            instance.StartCoroutine(instance.FetchLevelList(a, listUpdatedEvent));
        }


        private IEnumerator DownloadLevelRoute(LevelMetaData toDownload)
        {
            var uri = baseURL + $":{port}/level/download?proto_resp=false&ulid={toDownload.UniqueLevelId}";



            using (var www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {

                }
                else
                {
                    string savePath = instance.compressedLevelFiles.GetPath() + $"{toDownload.LocalLevelId}.zip";
                    string levelPath = instance.levelFolders.GetPath() + $"{toDownload.LocalLevelId}";
                    File.WriteAllText(savePath, www.downloadHandler.text);
                    MainCaller.Do(() => { instance.DisassembleZip(savePath, levelPath); });
                }



            }
        }

        private static int count;
        public static void DownloadLevel(LevelMetaData metaData)
        {
            count++;
            WebClient client = new WebClient();
            string savePath = instance.compressedLevelFiles.GetPath() + $"{count}.zip";
            string levelPath = LevelDataManager.SetupNewLevelFolder(metaData);
            var uri = instance.baseURL + $":{port}/level/download?proto_resp=false&ulid={metaData.UniqueLevelId}";
            LevelDataManager.SaveLevelMetaData(metaData, levelPath+"/MetaData.json");
            client.DownloadFile(uri, savePath);
            instance.DisassembleZip(savePath, levelPath);
            
        }
    }
}