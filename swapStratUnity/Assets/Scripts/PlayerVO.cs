using UnityEngine;
using System.Collections;

public class PlayerVO{
	static public int MAX_PLAYS = 8;
	public int currentTurnMoveLimit;
	public int currentTurnMoveCount;

	public int currentTurnPointCount;

	public int benchedTokens;
	public TokenBench playerTokenBench;
	public bool hasPlacedPieceFromBench;
	public bool hasSelectedTokenFromBench;
	public bool hasSelectedTokenFromBoard;
	public bool hasMovedTokenFromBoard;

	public enum PlayerType
	{
		none = 0,
		friend,
		enemy
	}
	public PlayerType currentPlayerType;

	public void InitializePlayCount(PlayerType playerType, TokenBench tb)
	{
		currentPlayerType = playerType;
		playerTokenBench = tb;
		playerTokenBench.InitialitializeBench (currentPlayerType);
		currentTurnMoveLimit = 0;
		currentTurnMoveCount = 0;
		hasPlacedPieceFromBench = false;
		hasSelectedTokenFromBench = false;
		hasSelectedTokenFromBoard = false;
		hasMovedTokenFromBoard = false;
	}

	public bool HasAvailableMoves()
	{
		return (currentTurnMoveCount > 0);
	}

	public bool HasAvailableTokensOnBench()
	{
		return playerTokenBench.HasAvailableTokens();
	}

	public void MoveMade()
	{
		if(currentTurnMoveCount > 0)
		{
			currentTurnMoveCount -= 1;
		}
	}

	public void StartPlayerTurn()
	{
		if(currentTurnMoveLimit < 8)
		{
//			currentTurnMoveLimit += 1;
			currentTurnMoveLimit = playerTokenBench.numberOfTokensOnBoard() + 1;
			currentTurnMoveCount = currentTurnMoveLimit;
		}
		else
		{
			currentTurnMoveCount = currentTurnMoveLimit;
		}
	}
}
