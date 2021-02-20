using UnityEngine;
using UnityEditor;
using com.mineorbit.dungeonsanddungeonscommon;
namespace com.mineorbit.dungeonsanddungeonscommon.editor
{

public class MyWindow : EditorWindow
{
    static string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
        UnityEngine.Object[] datas = Resources.LoadAll<LevelObjectData>("LevelObjectData/");
        myString = string.Join<UnityEngine.Object>(",",datas);
            myString += datas.Length.ToString();
        // Get existing open window or if none, make a new one:
        MyWindow window = (MyWindow)EditorWindow.GetWindow(typeof(MyWindow));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();
    }
    }
}