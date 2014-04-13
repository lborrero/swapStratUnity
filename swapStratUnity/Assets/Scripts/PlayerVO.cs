using UnityEngine;
using System.Collections;

public class PlayerVO{
	static public int MAX_PLAYS = 8;
	public int currentAllowedPlays;
	public int currentTurnPlays;
	public int benchedTokens;
	public TokenBench playerTokenBench;

	public enum PlayerType
	{
		friend = 0,
		enemy
	}
	public PlayerType currentPlayerType;

	public void InitializePlayCount(PlayerType playerType, TokenBench tb)
	{
		Debug.Log ("InitializePlayCount: " + playerType);
		currentPlayerType = playerType;
		playerTokenBench = tb;
		playerTokenBench.InitialitializeBench (currentPlayerType);
		currentAllowedPlays = 1;
		currentTurnPlays = 1;
	}

	public void MoveMade()
	{
		if(currentTurnPlays < 0)
		{
			currentTurnPlays -= 1;
		}
	}

	public void StartPlayerTurn()
	{
		if(currentTurnPlays > 8)
		{
			currentAllowedPlays += 1;
			currentTurnPlays = currentAllowedPlays;
		}
	}
}
