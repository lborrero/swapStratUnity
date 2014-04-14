using UnityEngine;
using System.Collections;

public class PlayerVO{
	static public int MAX_PLAYS = 8;
	public int currentAllowedPlays;
	public int currentTurnPlays;
	public int benchedTokens;
	public TokenBench playerTokenBench;
	public bool hasPlacedPieceFromBench;
	public bool hasSelectedTokenFromBench;
	public bool hasSelectedTokenFromBoard;
	public bool hasMovedTokenFromBoard;

	public enum PlayerType
	{
		friend = 0,
		enemy
	}
	public PlayerType currentPlayerType;

	public void InitializePlayCount(PlayerType playerType, TokenBench tb)
	{
		currentPlayerType = playerType;
		playerTokenBench = tb;
		playerTokenBench.InitialitializeBench (currentPlayerType);
		currentAllowedPlays = 1;
		currentTurnPlays = 1;
		hasPlacedPieceFromBench = false;
		hasSelectedTokenFromBench = false;
		hasSelectedTokenFromBoard = false;
		hasMovedTokenFromBoard = false;
	}

	public bool HasAvailableMoves()
	{
		return (currentTurnPlays > 0);
	}

	public void MoveMade()
	{
		if(currentTurnPlays > 0)
		{
			currentTurnPlays -= 1;
		}
	}

	public void StartPlayerTurn()
	{
		if(currentTurnPlays < 8)
		{
			currentAllowedPlays += 1;
			currentTurnPlays = currentAllowedPlays;
		}
	}
}
