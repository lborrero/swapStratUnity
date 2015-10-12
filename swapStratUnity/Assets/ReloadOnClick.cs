using UnityEngine;
using System.Collections;

public class ReloadOnClick : MonoBehaviour {
	public void ReloadScene()
	{
		Application.LoadLevel (Application.loadedLevel);
	}
}
