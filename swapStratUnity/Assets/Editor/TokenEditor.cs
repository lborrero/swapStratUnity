using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Token))]
public class TokenEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		Token myscript = (Token)target;


		EditorGUILayout.LabelField("Level", myscript.CurrentTokenState.ToString());

		if (GUILayout.Button ("Update Token"))
		{
			myscript.UpdateState ();
		}
	}
}
