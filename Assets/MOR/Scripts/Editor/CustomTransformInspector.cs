using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Transform), true)]
[CanEditMultipleObjects]
public class CustomTransformInspector : Editor
{

    //Unity's built-in editor
    Editor defaultEditor;
    Transform transform;

    void OnEnable()
    {
        //When this inspector is created, also create the built-in inspector
        defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
        transform = target as Transform;
    }

    void OnDisable()
    {
        //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
        //Also, make sure to call any required methods like OnDisable
        MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (disableMethod != null) {
            disableMethod.Invoke(defaultEditor, null);
        }

        DestroyImmediate(defaultEditor);
    }

    public override void OnInspectorGUI()
    {
        //EditorGUILayout.LabelField("Local Space | Reset:", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Local Space | Reset:");
            //GUILayout.Space(20);
            GUILayout.FlexibleSpace();            
            if (GUILayout.Button("P", GUILayout.MaxWidth(40), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Position");
                foreach (Transform t in Selection.transforms) {
                    t.localPosition = Vector3.zero;
                }
            }

            if (GUILayout.Button("R", GUILayout.MaxWidth(40), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Rotate");
                foreach (Transform t in Selection.transforms) {
                    t.localRotation = Quaternion.identity;
                }
            }

            if (GUILayout.Button("S", GUILayout.MaxWidth(40), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Scale");
                foreach (Transform t in Selection.transforms) {
                    t.localScale = Vector3.one;
                }

            }
            GUILayout.FlexibleSpace();            
            GUILayout.Space(20);
        }
        EditorGUILayout.EndHorizontal();        
        defaultEditor.OnInspectorGUI();
        //Show World Space Transform
        

        EditorGUILayout.LabelField("World Space", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Position", transform.position);
            GUI.enabled = true;
            if (GUILayout.Button("P", GUILayout.MaxWidth(30), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Position");
                foreach (Transform t in Selection.transforms) {
                    t.position = Vector3.zero;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Rotation", transform.eulerAngles);
            GUI.enabled = true;
            if (GUILayout.Button("R", GUILayout.MaxWidth(30), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Rotate");
                foreach (Transform t in Selection.transforms) {
                    t.rotation = Quaternion.identity;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        {
            GUI.enabled = false;
            EditorGUILayout.Vector3Field("Scale", transform.lossyScale);
            GUI.enabled = true;
            if (GUILayout.Button("S", GUILayout.MaxWidth(30), GUILayout.MaxHeight(16))) {
                Undo.RecordObjects(Selection.transforms,"Reset Scale");
                foreach (Transform t in Selection.transforms) {
                    Vector3 adjust = t.lossyScale;
                    adjust.x = 1 / adjust.x;
                    adjust.y = 1 / adjust.y;
                    adjust.z = 1 / adjust.z;
                    t.localScale = adjust;
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
    }
}