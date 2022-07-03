using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace com.mineorbit.dungeonsanddungeonscommon.editor
{
     
    [CustomEditor(typeof(LevelObjectData))]
    [CanEditMultipleObjects]
    public class LevelObjectDataEditor : Editor
    {
        private Vector2 scrollPos;
        public override void OnInspectorGUI()
        {
            var levelObjectData = target as LevelObjectData;

            EditorGUILayout.LabelField("ID",EditorStyles.boldLabel);
            levelObjectData.uniqueLevelObjectId = (ushort) EditorGUILayout.IntField("Unique ID", levelObjectData.uniqueLevelObjectId);
            levelObjectData.name = EditorGUILayout.TextField("Name",levelObjectData.name);
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("Instantiation Information",EditorStyles.boldLabel);
            
            if (!levelObjectData.Tiled)
            {
                levelObjectData.prefab = EditorGUILayout.ObjectField("Prefab", levelObjectData.prefab,typeof(GameObject));
            }
            else
            {
                if (levelObjectData.tileLevelObjectData == null)
                {
                    levelObjectData.tileLevelObjectData = ScriptableObject.CreateInstance<TileLevelObjectData>();
                }

                levelObjectData.tileLevelObjectData.material = (Material) EditorGUILayout.ObjectField("Material", levelObjectData.tileLevelObjectData.material,typeof(Material));
                levelObjectData.tileLevelObjectData.isFloor =
                    EditorGUILayout.Toggle("Is Floor", levelObjectData.tileLevelObjectData.isFloor);
            }
            
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Instantiation Rules",EditorStyles.boldLabel);
            
            // levelObjectData.color = EditorGUILayout.ObjectField("Color", levelObjectData.color, typeof(ScriptableObject));
            
            levelObjectData.dynamicInstantiable = EditorGUILayout.Toggle("Dynamic Instantiable", levelObjectData.dynamicInstantiable);

            levelObjectData.Buildable = EditorGUILayout.Toggle("Buildable", levelObjectData.Buildable);
            
            levelObjectData.inLevel = EditorGUILayout.Toggle("In Level", levelObjectData.inLevel);
            
            levelObjectData.Tiled = EditorGUILayout.Toggle("Tiled",levelObjectData.Tiled);
            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("Properties",EditorStyles.boldLabel);
            EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var prop in levelObjectData.properties)
            {
                prop.name = EditorGUILayout.TextField("Name", prop.name);
                prop.defaultValue = EditorGUILayout.TextField("Default Value", prop.defaultValue);
                EditorGUILayout.Separator();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                levelObjectData.properties.Add(new LevelObjectData.Property());
            }
            if (GUILayout.Button("Remove"))
            {
                levelObjectData.properties.RemoveAt(levelObjectData.properties.Count-1);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}