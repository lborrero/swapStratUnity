using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sGameManager : MonoBehaviour {

	private static sGameManager instance;
	private sGameManager() {}
	public static sGameManager Instance
	{
		get 
		{
			if (instance == null)
			{
				GameObject go = new GameObject("sGameManager");
				instance = go.AddComponent<sGameManager>();
				instance.currentGeneralGameState = GeneralGameState.startScreen;
				instance.currentInnerGameLoop = InnerGameLoop.playerOneTurn;
				instance.currentTurnLoop = TurnLoop.placeSelectedTokenFromBench;
			}
			return instance;
		}
	}

	public enum GeneralGameState
	{
		gameMode = 0,
		startScreen,
		endScreen,
		infoScreen
	}
	public GeneralGameState currentGeneralGameState;

	public enum InnerGameLoop
	{
		playerOneTurn = 0,
		playerTwoTurn,
		endInnerGameLoop
	}
	public InnerGameLoop currentInnerGameLoop;
	public int TurnCount;

	public void IncrementTurnCount()
	{
		TurnCount++;
	}

	public enum TurnLoop
	{
		selectATokenFromBench = 0,
		placeSelectedTokenFromBench,
		selectATokenFromBoard,
		moveSelectedToken,
		endLoopTurn
	}
	public TurnLoop currentTurnLoop = TurnLoop.selectATokenFromBench;
}
