using UnityEngine;
using System.Collections;

public class sGameManager : MonoBehaviour {

	private static sGameManager instance;
	
	private sGameManager() {}

	public enum GameState
	{
		placingTokens = 0,
		movingTokens
	}
	public GameState currentGameState = GameState.placingTokens;
	
	private static sGameManager mInstance;
	
	public static sGameManager Instance
	{
		get		
		{	
			if(mInstance == null)
			{
				GameObject go = new GameObject();
				mInstance = go.AddComponent<sGameManager>();
			}
			return mInstance;
		}
	}


}
