using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEngine;
using UnityEngine.InputSystem;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    
public class OptionManager : MonoBehaviour
{
    private static Dictionary<string, OptionHandler> optionHandlers;

    public static Dictionary<string, Option> options;

    private static string optionFilePath;
    

    public FileStructureProfile settingsFolder;

    public static OptionManager instance;

    public PlayerInput[] playerInputs;
    
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        
        optionFilePath = settingsFolder.GetPath() + "settings.txt";
        SetupOptions();
        if (File.Exists(optionFilePath))
            Load();
        else
            SetupOptionsFile();

       
        
        LoadKeyBindings();
    }

    private async void SetupOptionsFile()
    {
        await FileManager.foldersCreated.Task;
        Save();
    }

    public string[] bindingNames;
    
    public void LoadKeyBindings()
    {
        for (int i = 0; i < 3; i++)
        {
            LoadKeyBinding(i);
        }
    }

    public void LoadKeyBinding(int i)
    {

        string path = settingsFolder.GetPath() + bindingNames[i];
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            playerInputs[i].actions.LoadBindingOverridesFromJson(data);
        }
    }
    
    public static void SaveKeyBindings()
    {
        for (int i = 0; i < 3; i++)
        {
            instance.SaveKeyBinding(i);
        }
    }

    public void SaveKeyBinding(int i)
    {
        string path = settingsFolder.GetPath() + bindingNames[i];
        File.Delete(path);
        try
        {
            File.WriteAllText(path,playerInputs[i].actions.SaveBindingOverridesAsJson());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private void SetupOptions()
    {
        optionHandlers = new Dictionary<string, OptionHandler>();
        options = new Dictionary<string, Option>();
        var optionsLoaded = Resources.LoadAll("options", typeof(Option));
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        List<Type> types = new List<Type>();
        foreach (var x in assemblies)
        {
            types.AddRange(x.GetTypes().ToList().FindAll((t) =>  t.BaseType == typeof(OptionHandler)));
        }
        
        foreach (var o in optionsLoaded)
        {
            var opt = (Option) o;
            options.Add(opt.OptionTag, opt);
        }



        foreach (var t in types)
        {
                optionHandlers.Add(t.Name, Activator.CreateInstance(t) as OptionHandler);
        }
        
        foreach (var opt in options.Values)
            if (opt.optionHandlerName != string.Empty)
                if (optionHandlers.ContainsKey(opt.optionHandlerName))
                    opt.optionHandler = optionHandlers[opt.optionHandlerName];
    }


    public static void Load()
    {
        var counter = 0;
        string line;

        var file = new StreamReader(optionFilePath);
        while ((line = file.ReadLine()) != null)
        {
            var kV = line.Split('=');
            if (kV.Length == 2)
            {
                var tag = kV[0];
                var value = kV[1];
                if (options.ContainsKey(tag)) options[tag].Value = value;
            }

            counter++;
        }

        file.Close();
        ApplyOptions();
    }

    private static void ApplyOptions()
    {
        foreach (var opt in options.Values) opt.Value = opt.Value;
    }

    public static void Save()
    {
        File.Delete(optionFilePath);
        var writer = new StreamWriter(optionFilePath, true);
        foreach (var o in options.Values) writer.WriteLine(o.OptionTag + "=" + o.Value);
        writer.Close();
        ApplyOptions();
    }
}
}