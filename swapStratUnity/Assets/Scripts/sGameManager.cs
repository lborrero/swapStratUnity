using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sGameManager : MonoBehaviour {

	private static sGameManager instance;
	
	private sGameManager() {}

	public enum GeneralGameState
	{
		gameMode = 0
	}
	public GeneralGameState currentGeneralGameState;

	public enum InnerGameLoop
	{
		playerOneTurn = 0,
		playerTwoTurn
	}
	public InnerGameLoop currentInnerGameLoop;

	public enum TurnLoop
	{
		selectATokenFromBench = 0,
		placeSelectedTokenFromBench,
		selectATokenFromBoard,
		moveSelectedToken
	}
	public TurnLoop currentTurnLoop = TurnLoop.selectATokenFromBench;
	
	public static sGameManager Instance
	{
		get 
		{
			if (instance == null)
			{
				GameObject go = new GameObject("sGameManager");
				instance = go.AddComponent<sGameManager>();
				instance.currentGeneralGameState = GeneralGameState.gameMode;
				instance.currentInnerGameLoop = InnerGameLoop.playerOneTurn;
				instance.currentTurnLoop = TurnLoop.placeSelectedTokenFromBench;
			}
			return instance;
		}
	}


}
