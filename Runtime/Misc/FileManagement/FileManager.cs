﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

namespace com.mineorbit.dungeonsanddungeonscommon
{

    public class FileManager : MonoBehaviour
    {
        public FileStructureProfile root;
        void Awake()
        {
            Setup();
        }

        void Setup()
        {
            foreach (string s in List(root))
            {
                createFolder(s);
            }
        }


        string[] List(FileStructureProfile profile)
        {
            if (profile == null) return new string[0];
            string path = "/" + profile.name;
            List<string> paths = new List<string>();
            paths.Add(path);
            foreach (FileStructureProfile fS in profile.subStructures)
            {
                string[] subList = List(fS);
                string[] resultList = new string[subList.Length];
                int i = 0;
                foreach (string subpath in subList)
                {
                    resultList[i] = path + subpath;
                    i++;
                }
                paths.AddRange(resultList);
            }
            return paths.ToArray();
        }

        public static void deleteFolder(string path)
        {
            string filePath = Application.persistentDataPath + path;
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
            }
        }

        public static void createFolder(string path, bool persistent = true)
        {
            string filePath = (persistent ? Application.persistentDataPath : "") + path;

            UnityEngine.Debug.Log("Erstelle Ordner: " + filePath);
            try
            {
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                else UnityEngine.Debug.Log("Ordner gibt es schon");

            }
            catch (IOException ex)
            {
                UnityEngine.Debug.Log(ex.Message);
            }
        }
    }
}
