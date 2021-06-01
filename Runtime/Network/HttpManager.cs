using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class HttpManager : MonoBehaviour
    {
        public Option levelServerURL;

        public string baseURL;

    
        private void Start()
        {
            baseURL = (string) levelServerURL.Value;
        }

        class Zip
    {
        // https://www.codeproject.com/Tips/319438/How-to-Compress-Decompress-directories
        
        public delegate void ProgressDelegate(string sMessage);
         static void CompressFile(string sDir, string sRelativePath, GZipStream zipStream)
    {
      //Compress file name
      char[] chars = sRelativePath.ToCharArray();
      zipStream.Write(BitConverter.GetBytes(chars.Length), 0, sizeof(int));
      foreach (char c in chars)
        zipStream.Write(BitConverter.GetBytes(c), 0, sizeof(char));

      //Compress file content
      byte[] bytes = File.ReadAllBytes(Path.Combine(sDir, sRelativePath));
      zipStream.Write(BitConverter.GetBytes(bytes.Length), 0, sizeof(int));
      zipStream.Write(bytes, 0, bytes.Length);
    }

    static bool DecompressFile(string sDir, GZipStream zipStream, ProgressDelegate progress)
    {
      //Decompress file name
      byte[] bytes = new byte[sizeof(int)];
      int Readed = zipStream.Read(bytes, 0, sizeof(int));
      if (Readed < sizeof(int))
        return false;

      int iNameLen = BitConverter.ToInt32(bytes, 0);
      bytes = new byte[sizeof(char)];
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < iNameLen; i++)
      {
        zipStream.Read(bytes, 0, sizeof(char));
        char c = BitConverter.ToChar(bytes, 0);
        sb.Append(c);
      }
      string sFileName = sb.ToString();
      if (progress != null)
        progress(sFileName);

      //Decompress file content
      bytes = new byte[sizeof(int)];
      zipStream.Read(bytes, 0, sizeof(int));
      int iFileLen = BitConverter.ToInt32(bytes, 0);

      bytes = new byte[iFileLen];
      zipStream.Read(bytes, 0, bytes.Length);

      string sFilePath = Path.Combine(sDir, sFileName);
      string sFinalDir = Path.GetDirectoryName(sFilePath);
      if (!Directory.Exists(sFinalDir))
        Directory.CreateDirectory(sFinalDir);

      using (FileStream outFile = new FileStream(sFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        outFile.Write(bytes, 0, iFileLen);

      return true;
    }

    public static void CompressDirectory(string sInDir, string sOutFile, ProgressDelegate progress)
    {
      string[] sFiles = Directory.GetFiles(sInDir, "*.*", SearchOption.AllDirectories);
      int iDirLen = sInDir[sInDir.Length - 1] == Path.DirectorySeparatorChar ? sInDir.Length : sInDir.Length + 1;

      using (FileStream outFile = new FileStream(sOutFile, FileMode.Create, FileAccess.Write, FileShare.None))
      using (GZipStream str = new GZipStream(outFile, CompressionMode.Compress))
        foreach (string sFilePath in sFiles)
        {
          string sRelativePath = sFilePath.Substring(iDirLen);
          if (progress != null)
            progress(sRelativePath);
          CompressFile(sInDir, sRelativePath, str);
        }
    }

    public static void DecompressToDirectory(string sCompressedFile, string sDir, ProgressDelegate progress)
    {
      using (FileStream inFile = new FileStream(sCompressedFile, FileMode.Open, FileAccess.Read, FileShare.None))
      using (GZipStream zipStream = new GZipStream(inFile, CompressionMode.Decompress, true))
        while (DecompressFile(sDir, zipStream, progress));
    }

    }
    
    

    private IEnumerator UploadLevel(LevelMetaData levelToUpload,string path, UnityAction<string> action)
    {
        var url = baseURL+"/upload";
        
        
        var fileByte = File.ReadAllBytes(path);
        var form = new WWWForm();
        form.AddField("name", levelToUpload.FullName);
        form.AddBinaryData("level", fileByte, levelToUpload.uniqueLevelId + ".zip", "application / zip");

        action.Invoke("Uploading Level");
        using (var www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
                action.Invoke(www.error);
            else
                action.Invoke(www.downloadHandler.text);
        }
    }


    public void StartUpload()
    {
        var path = AssembleZip("","");
        StartCoroutine(UploadLevel(null,"path",null));
    }

    private string AssembleZip(string resultPath, string targetPath)
    {
        //AlertScreen.alert.Open("Removing old File");
        File.Delete(resultPath);
        //AlertScreen.alert.Open("Compressing Level");
        
        Zip.CompressDirectory(targetPath,resultPath, (x) =>
        {
            //AlertScreen.alert.Open("Compressing "+x);
        });
        return resultPath;
    }
    }
}