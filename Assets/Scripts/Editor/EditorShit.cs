/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MyScript))]
public class MyScriptEditor : Editor
{
	override public void OnInspectorGUI()
	{
		var myScript = target as MyScript;
		
		myScript.someBool= GUILayout.Toggle(myScript.someBool, "Some Bool");
		
		if (myScript.someBool) {
			myScript.someFloat = EditorGUILayout.FloatField ("Soem Float:", myScript.someFloat);
		}
		
	}
}
*/