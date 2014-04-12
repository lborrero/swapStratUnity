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
	
	public static sGameManager Instance
	{
		get 
		{
			if (instance == null)
			{
				instance = new sGameManager();
			}
			return instance;
		}
	}


}
