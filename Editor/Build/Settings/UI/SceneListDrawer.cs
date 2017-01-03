﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace SuperSystems.UnityBuild
{

[CustomPropertyDrawer(typeof(SceneList))]
public class SceneListDrawer : PropertyDrawer
{
    private bool show = false;
    private int index = 0;
    private SerializedProperty list = null;
    private List<SceneList.Scene> availableScenesList = null;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUILayout.BeginHorizontal();
        UnityBuildGUIUtility.DropdownHeader("SceneList", ref show, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        if (list == null)
        {
            list = property.FindPropertyRelative("enabledScenes");
        }

        if (availableScenesList == null)
        {
            PopulateSceneList();
        }

        if (show)
        {
            EditorGUILayout.BeginVertical(UnityBuildGUIUtility.dropdownContentStyle);

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty platformProperty = list.GetArrayElementAtIndex(i);

                string filePath = platformProperty.FindPropertyRelative("filePath").stringValue;
                string sceneName = Path.GetFileNameWithoutExtension(filePath);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextArea(sceneName + " (" + filePath + ")");

                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUILayout.Button("↑↑", UnityBuildGUIUtility.helpButtonStyle))
                {
                    list.MoveArrayElement(i, 0);
                }
                if (GUILayout.Button("↑", UnityBuildGUIUtility.helpButtonStyle))
                {
                    list.MoveArrayElement(i, i - 1);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(i == list.arraySize - 1);
                if (GUILayout.Button("↓", UnityBuildGUIUtility.helpButtonStyle))
                {
                    list.MoveArrayElement(i, i + 1);
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("X", UnityBuildGUIUtility.helpButtonStyle))
                {
                    list.DeleteArrayElementAtIndex(i);
                }

                property.serializedObject.ApplyModifiedProperties();

                PopulateSceneList();

                EditorGUILayout.EndHorizontal();
            }

            if (list.arraySize > 0)
            {
                GUILayout.Space(20);
            }

            if (availableScenesList.Count > 0)
            {   
                GUILayout.BeginHorizontal();

                string[] sceneStringList = new string[availableScenesList.Count];
                for (int i = 0; i < sceneStringList.Length; i++)
                {
                    sceneStringList[i] = Path.GetFileNameWithoutExtension(availableScenesList[i].filePath) + " (" + availableScenesList[i].filePath.Replace("/", "\\") + ")";
                }

                index = EditorGUILayout.Popup(index, sceneStringList, UnityBuildGUIUtility.popupStyle, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Add Scene", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(150)))
                {
                    int addedIndex = list.arraySize;
                    SceneList.Scene scene = availableScenesList[index];
                    list.InsertArrayElementAtIndex(addedIndex);
                    list.GetArrayElementAtIndex(addedIndex).FindPropertyRelative("filePath").stringValue = scene.filePath;

                    availableScenesList.RemoveAt(index);

                    index = 0;
                }

                GUILayout.EndHorizontal();
            }

            list.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Refresh Scene List", GUILayout.ExpandWidth(true)))
            {
                PopulateSceneList();
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUI.EndProperty();
    }

    private void PopulateSceneList()
    {
        if (availableScenesList == null)
            availableScenesList = new List<SceneList.Scene>();
        else
            availableScenesList.Clear();

        string[] allScenesGUID = AssetDatabase.FindAssets("t:scene");
        string[] allScenes = new string[allScenesGUID.Length];
        for (int i = 0; i < allScenesGUID.Length; i++)
        {
            allScenes[i] = AssetDatabase.GUIDToAssetPath(allScenesGUID[i]);
        }

        for (int i = 0; i < allScenes.Length; i++)
        {
            bool sceneAlreadyAdded = false;
            for (int j = 0; j < list.arraySize; j++)
            {
                if (Path.Equals(list.GetArrayElementAtIndex(j).FindPropertyRelative("filePath").stringValue, allScenes[i]))
                {
                    sceneAlreadyAdded = true;
                    break;
                }
            }

            if (!sceneAlreadyAdded)
            {
                SceneList.Scene scene = new SceneList.Scene();
                scene.filePath = allScenes[i];
                availableScenesList.Add(scene);
            }
        }
    }
}

}