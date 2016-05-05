using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GameSetupVO))]
public class GameSetupEditor : Editor {

	enum tileType
	{
		red = 0,
		blue,
		empty,
		nothing
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		GameSetupVO myGSVO = (GameSetupVO) target;

//		EditorGUI.IntField (Rect(0,0, 100,100), "Experience", myGSVO.random);

//		EditorGUILayout.BeginHorizontal("Box", GUILayout.ExpandWidth(false));
//
//		EditorGUILayout.BeginVertical ("square", GUILayout.ExpandWidth(false));
//
//		EditorGUILayout.IntField (Rect(0,35,position.width,15),"1", myGSVO.random, GUILayout.ExpandWidth(false));
//		EditorGUILayout.IntField ("1", myGSVO.random, GUILayout.ExpandWidth(false));
//		EditorGUILayout.EndVertical ();
//
//		EditorGUILayout.IntField ("1", myGSVO.random, GUILayout.ExpandWidth(false));
//		EditorGUILayout.IntField ("1", myGSVO.random, GUILayout.ExpandWidth(false));
//		EditorGUILayout.EndHorizontal();

		if(GUILayout.Button("BuildObject"))
		{
			myGSVO.someFunction ();
		}
	}


}
