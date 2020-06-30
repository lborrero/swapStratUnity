using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor 
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		Tile myscript = (Tile)target;
		if (GUILayout.Button ("Update Tile"))
		{
			myscript.UpdateState ();
		}
	}
}
