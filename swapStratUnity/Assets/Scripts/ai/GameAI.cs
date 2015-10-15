﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facet.Combinatorics;
using System.Text;
using System;

public class GameAI : MonoBehaviour {

	public enum AiType
	{
		random = 0,
		intermediate,
		hearthstone
	}

	SwapBoard sb;
	List<string> MediumMemory = new List<string>();
	public PlayerVO.PlayerType aiPt = new PlayerVO.PlayerType ();
	public AiType ait;

	public void SetAiPlayerType(int _aiPt)
	{
		aiPt = (PlayerVO.PlayerType)_aiPt;
	}

	enum AiProcesses
	{
		GeneratePossibilityBubblesOnTheBoard_Step = 0,
		PlayingThePossibilityBubbles_Loop,
		VerifyHighestHeuresticValue_Loop,
		CheckingMovementPermutationIsPossible_Loop,
		AiProcessCompleted
	}
	
	AiProcesses currentAiProcess;

	List<Vector2> TileSelectionSequence = new List<Vector2>();
	bool playFinalizationSequence = false;

	// Use this for initialization
	void Start () {
		sb = gameObject.GetComponent<SwapBoard> ();
//		Debug.Log (sb);
		PossibilityCalculator pc = new PossibilityCalculator ();
	}

	void Update()
	{
		if(sGameManager.Instance.currentInnerGameLoop != sGameManager.InnerGameLoop.endInnerGameLoop)
		{
			PercieveLoop ();
		}
	}

	bool takeDecision = true;
	IEnumerator GenerateEvent()
	{
		yield return new WaitForSeconds(0.5f);
		takeDecision = true;
	}

	void PercieveLoop()
	{
		if(aiPt == PlayerVO.PlayerType.enemy)
		{
			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerTwoTurn) 
			{
//				Debug.Log("-");
				if(takeDecision)
				{
					ThinkLoop ("asdf");
					takeDecision = false;
					StartCoroutine(GenerateEvent());
				}
			}
		}
		if(aiPt == PlayerVO.PlayerType.friend)
		{
			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerOneTurn) 
			{
//				Debug.Log("AI P1");
				if(takeDecision)
				{
					ThinkLoop ("asdf");
					takeDecision = false;
					StartCoroutine(GenerateEvent());
				}
			}
		}
	}

	bool timeChecker = false;

	void GenerateHearthStoneSequence()
	{
		if (!playFinalizationSequence) 
		{
			List<Tile> possibleTiles = GetCurrentlyEmptyTiles ();
			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes (possibleTiles);
			int numberOfMoves = (aiPt == PlayerVO.PlayerType.friend) ? sb.sbm.player1.currentTurnMoveLimit : sb.sbm.player2.currentTurnMoveLimit;
			int aiScore = numberOfMoves + ((aiPt == PlayerVO.PlayerType.friend) ? sb.sbm.player1.currentTurnPointCount : sb.sbm.player2.currentTurnPointCount);
			int otherPlayerScore = (!(aiPt == PlayerVO.PlayerType.friend)) ? sb.sbm.player1.currentTurnPointCount : sb.sbm.player2.currentTurnPointCount;
		
			if (emptyTilesIDsSplitByType [1].Count <= numberOfMoves) {
				if (aiScore > otherPlayerScore) {
					if (CanAiFinalizeMatch ()) {
						if (FinaliseMatch ()) {
							playFinalizationSequence = true;
						}
					}
				}
			}
		}
	}

	bool ThinkLoop(string theGameObjectState)
	{
		bool returnValue = false;
		GenerateHearthStoneSequence ();
		switch (sb.sgm.currentTurnLoop) 
		{
		case sGameManager.TurnLoop.selectATokenFromBench:
		{
//				Debug.Log ("AI: selectATokenFromBench");
				PlayerVO tempPVO;
				if (aiPt == PlayerVO.PlayerType.enemy) {
					tempPVO = sb.sbm.player2;
				} 
				else 
				{
					tempPVO = sb.sbm.player1;
				}
				int tokensLeft = tempPVO.playerTokenBench.benchedTokens.Count;
			for (int i = 0; i < tokensLeft; i++)
			{		
				Token tk = tempPVO.playerTokenBench.benchedTokens[i].GetComponent<Token>();
				if (!tk.isTokenOnBoard && tk.currentTokenState != Token.TokenState.disabled) 
				{
					sb.sbm.TokenClicked(tk.tokenId, tk.currentTokenType);
					break;
				}
			}
			break;
		}
		case sGameManager.TurnLoop.placeSelectedTokenFromBench:
		{
//				Debug.Log ("AI: placeSelectedTokenFromBench");

			switch(ait)
			{
				case AiType.intermediate:
				{
					List<Tile> possibleTiles = GetCurrentlyEmptyTiles();
					System.Random rnd = new System.Random();
					sb.sbm.TileClicked(possibleTiles[rnd.Next(0, possibleTiles.Count-1)].tileId);

					TileSelectionSequence.Clear();
					GenerateTurnSequence();
					Debug.Log("GenerateTurnSequence: " + TileSelectionSequence.Count + " " + currentAiProcess);
					break;
				}
				case AiType.random:
				{
					List<Tile> possibleTiles = GetCurrentlyEmptyTiles();
					System.Random rnd = new System.Random();
					sb.sbm.TileClicked(possibleTiles[rnd.Next(0, possibleTiles.Count-1)].tileId);
					break;
				} 
				case AiType.hearthstone:
				{
					Debug.Log("--------placeSelectedTokenFromBench");
					
					if(playFinalizationSequence)
					{
						sb.sbm.TileClicked((int)TileSelectionSequence[0].y);
						DestroyTurnSequenceStep0();
					}
					else
					{
						List<Tile> possibleTiles = GetCurrentlyEmptyTiles();
						List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes(possibleTiles);
						HearthStone_PlayUntilFold(emptyTilesIDsSplitByType);
					}
				break;
				}
			}
			break;
		}
		case sGameManager.TurnLoop.selectATokenFromBoard:
		{
			switch(ait)
			{
				case AiType.intermediate:
				{
					if(currentAiProcess == AiProcesses.AiProcessCompleted)
					{
					if(TileSelectionSequence.Count> 0)
					{
						Debug.Log("AI selectATokenFromBoard" + (int)TileSelectionSequence[0].x);
						sb.sbm.TileClicked((int)TileSelectionSequence[0].x);// select token with this tile id is found in the x value of the vector 2
					}
					else
					{
						List<Tile> possibleTokensOnTile = GetMoveableTokensList();
						System.Random rnd = new System.Random();
						sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
					}
					}
					break;
				}
				case AiType.random:
				{
					//			Debug.Log ("AI: selectATokenFromBoard");
					List<Tile> possibleTokensOnTile = GetMoveableTokensListExcludingTheBenchTokenJustPlaced();
					System.Random rnd = new System.Random();
					sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
					break;
				}
				case AiType.hearthstone:
				{
				Debug.Log("--------selectATokenFromBoard");
				if(playFinalizationSequence)
				{
					if(TileSelectionSequence.Count> 0)
					{
						sb.sbm.TileClicked((int)TileSelectionSequence[0].x);
					}
					else
					{
						List<Tile> possibleTokensOnTile = GetMoveableTokensList();
						System.Random rnd = new System.Random();
						sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
					}
				}
				else
				{
					List<Tile> possibleTokensOnTile = GetMoveableTokensList();

					List<List<int>> shortTermMemory = HearthStone_generateThePossiblityMovement(possibleTokensOnTile);

					int theTileIdWhereTheTokenToMoveIs = 0;
					int heuristic = -1;
					int tempHeuristic = -1;

					for(int i = 0; i<shortTermMemory.Count; i++)
					{
						List<Tile> returnValueB = new List<Tile>();
						for(int j = 0; j<shortTermMemory[i].Count; j++)
						{
							for(int k = 0; k<sb.sbm.boardList.Count; k++)
							{
								if(shortTermMemory[i][j] == sb.sbm.boardList[k].tileId)
								{
									returnValueB.Add(sb.sbm.boardList[k]);
								}
							}
						}

						tempHeuristic = Evaluate_HearthStone_HeuristicForPossibilityTypes(HearthStone_ParsePossibilitesIntoTypes(returnValueB));
						Debug.Log (i + "heuristic: " + heuristic + " " + tempHeuristic);
						if(heuristic < tempHeuristic)
						{
							theTileIdWhereTheTokenToMoveIs = possibleTokensOnTile[i].tileId;
							heuristic = tempHeuristic;
						}
					}

					TileSelectionSequence.Add(new Vector2(theTileIdWhereTheTokenToMoveIs, 0));
					sb.sbm.TileClicked(theTileIdWhereTheTokenToMoveIs);
				}
				break;
				}
			}
			break;
		}
		case sGameManager.TurnLoop.moveSelectedToken:
		{
			switch(ait)
			{
				case AiType.intermediate:
				{
				if(currentAiProcess == AiProcesses.AiProcessCompleted)
				{
					if(TileSelectionSequence.Count > 0)
					{
					Debug.Log("AI moveSelectedToken: " + (int)TileSelectionSequence[0].y);
					sb.sbm.TileClicked((int)TileSelectionSequence[0].y);// place token on this tile id is found in the y value of the vector 2
					DestroyTurnSequenceStep0();
					}
					else
					{
						//--- Select Random Tile to move to ---
						List<int> imediatePossibleTiles = sb.sbm.getTilesIdToMoveTo ();
						
						System.Random rnd = new System.Random();
						int returnedValue = imediatePossibleTiles[rnd.Next(0, imediatePossibleTiles.Count)];
						sb.sbm.TileClicked(returnedValue);
					}
				}
					break;
				}
				case AiType.random:
				{
					//--- Select Random Tile to move to ---
					List<int> imediatePossibleTiles = sb.sbm.getTilesIdToMoveTo ();
//					Debug.Log ("AI: " + imediatePossibleTiles.Count);
					
					System.Random rnd = new System.Random();
					int returnedValue = imediatePossibleTiles[rnd.Next(0, imediatePossibleTiles.Count)];
					sb.sbm.TileClicked(returnedValue);
					break;
				}
			case AiType.hearthstone:
			{
				Debug.Log("--------moveSelectedToken");
				if(playFinalizationSequence)
				{
					if(TileSelectionSequence.Count > 0)
					{
					sb.sbm.TileClicked((int)TileSelectionSequence[0].y);
					DestroyTurnSequenceStep0();
					}
					else
					{
						List<int> imediatePossibleTiles = sb.sbm.getTilesIdToMoveTo ();
						
						System.Random rnd = new System.Random();
						int returnedValue = imediatePossibleTiles[rnd.Next(0, imediatePossibleTiles.Count)];
						sb.sbm.TileClicked(returnedValue);
					}
				}
				else
				{
					List<Tile> possibleTokensOnTile = new List<Tile>();

					List<int> shortTermMemory = sb.sbm.getPotentialTilesIdToMoveToExcludingTheTileTheTokenIsIn ((int)TileSelectionSequence[0].x);

					List<Tile> possibleTiles = new List<Tile>();
					for(int k = 0; k<sb.sbm.boardList.Count; k++)
					{
						for(int j = 0; j<shortTermMemory.Count; j++)
						{
							if(shortTermMemory[j] == sb.sbm.boardList[k].tileId)
							{
								possibleTiles.Add(sb.sbm.boardList[k]);
							}
						}
					}

					HearthStone_PlayUntilFold(HearthStone_ParsePossibilitesIntoTypes(possibleTiles));
					TileSelectionSequence.Clear();
				}
				break;
			}
			}
			break;
		}
		case sGameManager.TurnLoop.endLoopTurn:
//			Debug.Log ("AI: endLoopTurn");
			break;
		}
		return returnValue;
	}

	List<int> getTileIdsThatAreYourColor(List<int> possibilityBubble)
	{
		for (int i = possibilityBubble.Count - 1; i >= 0; --i) 
		{
			if (sb.sbm.boardList [possibilityBubble [i]].currentTilePlayerType != aiPt) 
			{
				possibilityBubble.RemoveAt(i);
			}
		}
		return possibilityBubble;
	}

	List<Tile> GetNonMoveableTokensList()
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		Token tmptoken;
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if (sb.sbm.boardList [i].currentTileType == Tile.TileType.occupied &&
			    sb.sbm.boardList [i].occupyingTokenPlayerType == aiPt) 
			{
				tmptoken = sb.sbm.getTokenFromTokenListWithIdAndType (sb.sbm.boardList [i].occupyingTokenId, sb.sbm.boardList [i].occupyingTokenPlayerType);
				if (tmptoken.hasTokenBeenMoved || tmptoken.currentTokenState == Token.TokenState.disabled)
				{
					possibleTokensOnTile.Add (sb.sbm.boardList [i]);
				}
			}
		}
		return possibleTokensOnTile;
	}

	List<Tile> GetMoveableTokensListExcludingTheBenchTokenJustPlaced()
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		Token tmptoken;
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if (sb.sbm.boardList [i].currentTileType == Tile.TileType.occupied &&
			    sb.sbm.boardList [i].occupyingTokenPlayerType == aiPt) 
			{
				tmptoken = sb.sbm.getTokenFromTokenListWithIdAndType (sb.sbm.boardList [i].occupyingTokenId, sb.sbm.boardList [i].occupyingTokenPlayerType);
				if (tmptoken.currentTokenState != Token.TokenState.disabled)
				{
					possibleTokensOnTile.Add (sb.sbm.boardList [i]);
				}
			}
		}
		return possibleTokensOnTile;
	}

	List<Tile> GetMoveableTokensListEvenIfCurrentlyTheyCantMove()
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		Token tmptoken;
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if (sb.sbm.boardList [i].currentTileType == Tile.TileType.occupied &&
			    sb.sbm.boardList [i].occupyingTokenPlayerType == aiPt) 
			{
				tmptoken = sb.sbm.getTokenFromTokenListWithIdAndType (sb.sbm.boardList [i].occupyingTokenId, sb.sbm.boardList [i].occupyingTokenPlayerType);
				if (tmptoken.currentTokenState != Token.TokenState.disabled)
				{
					possibleTokensOnTile.Add (sb.sbm.boardList [i]);
				}
			}
		}
		return possibleTokensOnTile;
	}

	List<Tile> GetMoveableTokensList()
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		Token tmptoken;
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if (sb.sbm.boardList [i].currentTileType == Tile.TileType.occupied &&
				sb.sbm.boardList [i].occupyingTokenPlayerType == aiPt) 
			{
				tmptoken = sb.sbm.getTokenFromTokenListWithIdAndType (sb.sbm.boardList [i].occupyingTokenId, sb.sbm.boardList [i].occupyingTokenPlayerType);
				if (!tmptoken.hasTokenBeenMoved && tmptoken.currentTokenState != Token.TokenState.disabled)
				{
					List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (sb.sbm.boardListIntoBinaryList (i), sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [i].xPos, sb.sbm.boardList [i].yPos); 
					if(contiguousTiles.Count > 1)
					{
						possibleTokensOnTile.Add (sb.sbm.boardList [i]);
					}
				}
			}
		}
		return possibleTokensOnTile;
	}

	void DestroyTurnSequenceStep0()
	{
		TileSelectionSequence.RemoveAt (0);
		if(TileSelectionSequence.Count <= 0)
		{
			playFinalizationSequence = false;
		}
		Debug.Log ("clicks"  +TileSelectionSequence.Count);
	}

	List<List<int>> HearthStone_generateThePossiblityMovement(List<Tile> possibleMoveableTokensOnBoard)
	{
		//--- Generate Possibility bubbles on the board ---
		List<List<int>> shortTermMemory = new List<List<int>>(); // goes through every moveable token and board with their moveable option, and have them stocked in the short term memory. This short term allows us to check how many possibility bubble there are.
		//shortTermMemory are an array of tile IDs.
		
		for (int i = 0; i < possibleMoveableTokensOnBoard.Count; i++) 
		{
			shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveToExcludingTheTileTheTokenIsIn (possibleMoveableTokensOnBoard[i].tileId)); //  goes through every moveable token and board with their moveable option
		}

		return shortTermMemory;
	}

	int Evaluate_HearthStone_HeuristicForPossibilityTypes(List<List<int>> input)
	{
		int returnValue = 0;
		Debug.Log("1): " + input[0].Count);
		if(input[0].Count > 0)
		{
			Debug.Log("a: " + ((aiPt == PlayerVO.PlayerType.enemy)?0:2));
			if(returnValue < ((aiPt == PlayerVO.PlayerType.enemy)?0:2))
			{
				returnValue = ((aiPt == PlayerVO.PlayerType.enemy)?0:2);
			}
		}

		Debug.Log("2): " + input[1].Count);
		if(input[1].Count > 0)
		{
//			Debug.Log("b: " + 1);
			if(returnValue < 1)
			{
				returnValue = 1;
			}
		}

		Debug.Log("3): " + input[2].Count);
		if(input[2].Count > 0)
		{
			Debug.Log("c: " + ((aiPt == PlayerVO.PlayerType.enemy)?2:0));
			if(returnValue < ((aiPt == PlayerVO.PlayerType.enemy)?2:0))
			{
				returnValue = ((aiPt == PlayerVO.PlayerType.enemy)?2:0);
			}
		}

		return returnValue;
	}

	List<List<int>> HearthStone_ParsePossibilitesIntoTypes(List<Tile> possibleTilesToPlayOn)
	{
		List<List<int>> returnValue = new List<List<int>> ();
		List<int> pointCountingEnemy = new List<int>(); //these are tileID
		List<int> pointCountingNone = new List<int>(); //these are tileID
		List<int> pointCountingFriend = new List<int>(); //these are tileID
		for(int i = 0; i<possibleTilesToPlayOn.Count; i++)
		{
			switch(possibleTilesToPlayOn[i].currentTilePlayerType)
			{
			case PlayerVO.PlayerType.enemy:
				pointCountingEnemy.Add(possibleTilesToPlayOn[i].tileId);
				break;
			case PlayerVO.PlayerType.none:

				pointCountingNone.Add(possibleTilesToPlayOn[i].tileId);
				break;
			case PlayerVO.PlayerType.friend:
				pointCountingFriend.Add(possibleTilesToPlayOn[i].tileId);
				break;
			}
		}
//		Debug.Log ("pointCountingEnemy: " + pointCountingEnemy.Count);
//		Debug.Log ("pointCountingNone: " + pointCountingNone.Count);
//		Debug.Log ("pointCountingFriend: " + pointCountingFriend.Count);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingFriend:pointCountingEnemy);
		returnValue.Add (pointCountingNone);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingEnemy:pointCountingFriend);
		return returnValue;
	}

	bool HearthStone_PlayUntilFold(List<List<int>> playList)
	{
		bool playedToken = false;
		System.Random rnd = new System.Random();
		for(int j = 0; j<playList.Count; j++)
		{
			if(!playedToken)
			{
				int thisRnd = 0;
				while(playList[j].Count >0)
				{
					thisRnd = rnd.Next(0, playList[j].Count-1);
					if(sb.sbm.TileClicked(playList[j][thisRnd]))
					{
						playList[j].RemoveAt(thisRnd);
						playedToken = true;
						break;
					}
				}
			}
			else
			{
				break;
			}
		}
		return playedToken;
	}

	public class PossibilityTiles
	{
		public int numberOfTokensThatHaveThisSamePossibilityTiles;
		public List<Tile> tokensForThisPossibilitySpace;
		public List<Tile> possibilities;
	}

	List<PossibilityTiles> GeneratePossibilitiyBubbles()
	{
		List<List<int>> shortTermMemory = new List<List<int>>(); // goes through every moveable token and board with their moveable option, and have them stocked in the short term memory. This short term allows us to check how many possibility bubble there are.
		//shortTermMemory are an array of tile IDs.
		
		List<Tile> possibleMoveableTokensOnBoard = new List<Tile>(GetMoveableTokensListEvenIfCurrentlyTheyCantMove());
		List<Tile> nonMoveableTokensOnBoard = new List<Tile>(GetNonMoveableTokensList());
		if(possibleMoveableTokensOnBoard.Count <= 0)
		{
			return null;
		}
		
		for (int i = 0; i < possibleMoveableTokensOnBoard.Count; i++) 
		{
			shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveToIncludingTilesWithOwnToken (possibleMoveableTokensOnBoard[i].tileId)); //  goes through every moveable token and board with their moveable option
		}
		
		for (int j = 0; j < shortTermMemory.Count; j++) 
		{
			shortTermMemory[j].Sort ();	
		}
		
		List<Vector2> possibilityBubbles = new List<Vector2> (); // x is the index that references this bubble in shortTermMemory, y is the number of shortTermMemory that are the same to this index.
		//possibilityBubbles doesn't contain tile IDs
		for (int l = 0; l < shortTermMemory.Count; l++) 
		{
			bool containThisPOssibility = false;
			for (int k = 0; k < possibilityBubbles.Count; k++) 
			{
				if (shortTermMemory [((int)possibilityBubbles[k].x)].All (shortTermMemory [l].Contains)) 
				{
					containThisPOssibility = true;
					possibilityBubbles [k] += new Vector2(0,1);
				}
			}
			if(!containThisPOssibility)
			{
				possibilityBubbles.Add (new Vector2 (l, 1));
			}
		}
		
		//--- Generate Possibility bubbles on the board with tiles ---
		List<PossibilityTiles> possibilityBubbleWithTiles = new List<PossibilityTiles> ();
		
		for(int j = 0; j<possibilityBubbles.Count; j++)
		{
			PossibilityTiles tempPoss = new PossibilityTiles ();
			tempPoss.numberOfTokensThatHaveThisSamePossibilityTiles = (int)possibilityBubbles[j].y;
			tempPoss.possibilities = new List<Tile>();
			for(int k = 0; k<shortTermMemory[((int)possibilityBubbles[j].x)].Count; k++)
			{
				tempPoss.possibilities.Add(sb.sbm.boardList[shortTermMemory[((int)possibilityBubbles[j].x)][k]]);
			}
			tempPoss.tokensForThisPossibilitySpace = new List<Tile>();
			for (int i = 0; i<possibleMoveableTokensOnBoard.Count; i++) //possibleMoveableTokensOnBoard.count == shortTermMemory.count
			{
				if(shortTermMemory[((int)possibilityBubbles[j].x)].Contains(possibleMoveableTokensOnBoard[i].tileId))
				{
					tempPoss.tokensForThisPossibilitySpace.Add(possibleMoveableTokensOnBoard[i]);
				}
			}
			possibilityBubbleWithTiles.Add(tempPoss);
		}

		// verify to see if there are any empty token bubble
		PossibilityTiles tempPossForEmptyBubble = new PossibilityTiles ();
		tempPossForEmptyBubble.numberOfTokensThatHaveThisSamePossibilityTiles = 0;
		tempPossForEmptyBubble.possibilities = new List<Tile>();
		List<Tile> possibleTiles = GetCurrentlyEmptyTiles ();

		for(int i=0; i<possibleTiles.Count; i++)
		{
			bool containsIt = false;
			for(int j=0; j<possibilityBubbleWithTiles.Count; j++)
			{
				if(possibilityBubbleWithTiles[j].possibilities.Contains(possibleTiles[i]))
				{
					containsIt = true;
					break;
				}
			}
			if(!containsIt)
			{
				tempPossForEmptyBubble.possibilities.Add(possibleTiles[i]);
			}
		}
		// add emtpy token bubble if there are any.
		if(tempPossForEmptyBubble.possibilities.Count > 0)
		{
			tempPossForEmptyBubble.tokensForThisPossibilitySpace = new List<Tile>();
			possibilityBubbleWithTiles.Add(tempPossForEmptyBubble);
		}

		return possibilityBubbleWithTiles;
	}

	List<Tile> GetCurrentlyEmptyTiles()
	{
		List<Tile> possibleTiles = new List<Tile>();
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if(sb.sbm.boardList[i].currentTileType == Tile.TileType.empty)
			{
				possibleTiles.Add(sb.sbm.boardList[i]);
			}
		}
		return possibleTiles;
	}

	bool CanAiFinalizeMatch()
	{
		List<PossibilityTiles> possibilityBubbleWithTiles = GeneratePossibilitiyBubbles ();

		bool doesPlayerHaveTokensToMoveOnBench = (((aiPt == PlayerVO.PlayerType.friend)?sb.sbm.player1.HasAvailableTokensOnBench():sb.sbm.player2.HasAvailableTokensOnBench()));
		int goodAmountOfTokenPerBubbleToFinalize = 1;
		int howManyNeedAnExtraBenchToken = 0;
		for(int i = 0; i<possibilityBubbleWithTiles.Count(); i++)
		{
			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes(possibilityBubbleWithTiles[i].possibilities);
			int numberOfTokensThePlayerHasForThisPossibilitySpace = possibilityBubbleWithTiles[i].numberOfTokensThatHaveThisSamePossibilityTiles;
			if(doesPlayerHaveTokensToMoveOnBench)
			{
				if(emptyTilesIDsSplitByType[1].Count > numberOfTokensThePlayerHasForThisPossibilitySpace)
				{
					howManyNeedAnExtraBenchToken += 1;
					numberOfTokensThePlayerHasForThisPossibilitySpace += 1;
				}
			}
			if(emptyTilesIDsSplitByType[1].Count <= numberOfTokensThePlayerHasForThisPossibilitySpace)
			{
				goodAmountOfTokenPerBubbleToFinalize &= 1;
			}
			else
			{
				goodAmountOfTokenPerBubbleToFinalize &= 0;
			}
		}
		return (goodAmountOfTokenPerBubbleToFinalize == 1);
	}

	List<int> RepresentBoardInBinary(List<Tile> possibilities, List<Tile> excludeFromPossibility)
	{
		List<int> currentPossibilityBoard = new List<int>();
		List<int> currentMoveablTokensInThisBubble = new List<int>();
		for(int k=0; k<sb.sbm.boardList.Count; k++)
		{
			if(possibilities.Contains(sb.sbm.boardList[k]))
			{
				currentPossibilityBoard.Add(1);
			}
			else
			{
				currentPossibilityBoard.Add(0);
			}
		}
//		for(int k=0; k<excludeFromPossibility.Count; k++)
//		{
//			if(currentPossibilityBoard[excludeFromPossibility[k].tileId] == 1)
//			{
//				currentMoveablTokensInThisBubble.Add(excludeFromPossibility[k].tileId);
//			}
//			currentPossibilityBoard[excludeFromPossibility[k].tileId] = 0;
//		}
		return currentPossibilityBoard;
	}
	
	bool FinaliseMatch()
	{
		int numberOfpermutationThatWerePossible = 0;
		List<Vector2> bubbleTileSelectionSequence = new List<Vector2>();
		List<PossibilityTiles> possibilityBubbleWithTiles = GeneratePossibilitiyBubbles ();
		List<Tile> possibleMoveableTokensOnBoard = new List<Tile>(GetMoveableTokensList());

		bool doesPlayerHaveTokensToMoveOnBench = (((aiPt == PlayerVO.PlayerType.friend)?sb.sbm.player1.HasAvailableTokensOnBench():sb.sbm.player2.HasAvailableTokensOnBench()));

		bool benchTokenAdded = false;

		for(int i=0; i<possibilityBubbleWithTiles.Count; i++)
		{
			bool addBenchTokenInSequence = false;
			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes(possibilityBubbleWithTiles[i].possibilities);
			int numberOfTokensThePlayerHasForThisPossibilitySpace = possibilityBubbleWithTiles[i].numberOfTokensThatHaveThisSamePossibilityTiles;
			if(emptyTilesIDsSplitByType[1].Count >= numberOfTokensThePlayerHasForThisPossibilitySpace &&
			   doesPlayerHaveTokensToMoveOnBench && !addBenchTokenInSequence)
			{
				addBenchTokenInSequence = true;
				numberOfTokensThePlayerHasForThisPossibilitySpace += 1;
			}
			if(!benchTokenAdded &&
			   i == possibilityBubbleWithTiles.Count-1 &&
			   doesPlayerHaveTokensToMoveOnBench && !addBenchTokenInSequence)
			{
				addBenchTokenInSequence = true;
				numberOfTokensThePlayerHasForThisPossibilitySpace += 1;
			}

			int r = possibilityBubbleWithTiles[i].numberOfTokensThatHaveThisSamePossibilityTiles + ((addBenchTokenInSequence)?1:0);
			char [] desiredTileIndexesToMoveTo = new char[r];
			for(int j=0; j<r; j++)
			{
				string ss = "" + (j + ((addBenchTokenInSequence)?-1:0));
				desiredTileIndexesToMoveTo[j] = ss[0];
			}
			Permutations<char> permutationsForMovementOrder = new Permutations<char>(desiredTileIndexesToMoveTo);

			bool isAbleToFormToThisPermutation = false;
			bool containsTheBenchMovement = false;

			List<int> currentPossibilityBoard = RepresentBoardInBinary(possibilityBubbleWithTiles[i].possibilities, possibleMoveableTokensOnBoard);
			List<int> currentMoveablTokensInThisBubble = new List<int>();
			
			for(int k=0; k<possibleMoveableTokensOnBoard.Count; k++)
			{
				if(currentPossibilityBoard[possibleMoveableTokensOnBoard[k].tileId] == 1)
				{
					currentMoveablTokensInThisBubble.Add(possibleMoveableTokensOnBoard[k].tileId);
				}
				currentPossibilityBoard[possibleMoveableTokensOnBoard[k].tileId] = 0;
			}

			for(int j = 0; (j<permutationsForMovementOrder.Count && j<1000); j++)
			{
				IList<char> permutationBeingVerified = permutationsForMovementOrder.ElementAt(j);
			
				// take the order
				// have the pieces move to that order
				// have tile 1 go to the where the 1 is found in the permutations order.
				List<List<int>> movementSequenceShortTermMemory = new List<List<int>>();// movementSequenceShortTermMemory are snapshots of the sequence of what the board has to look like for this turn
				
				//I Need a representation of the board for this possibility bubble;
				List<int> evolvingPossibilityBoard = new List<int>(currentPossibilityBoard);
				movementSequenceShortTermMemory.Add(new List<int>(evolvingPossibilityBoard));

				int permutationsIncrement = 0;

				int isAbleToMoveToAllPositionsForThisPermutation = 1;
				List<int> cestPareille = new List<int>();

				for(int k=0; k<emptyTilesIDsSplitByType[1].Count; k++)
				{
					if(permutationBeingVerified[permutationsIncrement] == '-') // '-' for '-1'
					{
						containsTheBenchMovement = true;
						int tileIdForBenchTokenToMoveTo = emptyTilesIDsSplitByType[1][k];
						bubbleTileSelectionSequence.Insert(0, new Vector2(-1, tileIdForBenchTokenToMoveTo));
						evolvingPossibilityBoard[tileIdForBenchTokenToMoveTo] = 0;
						permutationsIncrement++;
						continue;
					}
					// end of setting up bench token movement.

					//	Debug.Log("asdfasdf: " + possibleMoveableTokensOnBoard.Count + " " + permutationBeingVerified.Count + " " + (int)(permutationBeingVerified[permutationsIncrement] - 48));
					int tileIdForTokenToMove = currentMoveablTokensInThisBubble[(int)(permutationBeingVerified[permutationsIncrement] - 48)];// 48 is how to convert char values to int values
					// permutationBeingVerified length is exactly the same length as the currently available tokens that can be moved on the board.
					permutationsIncrement++;
					int tileIdForTokenToMoveTo = emptyTilesIDsSplitByType[1][k];
					
					//include the currently selected tile for the contiguous search
					evolvingPossibilityBoard[tileIdForTokenToMove] = 1;
					
					// can I make the following move?
					List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (evolvingPossibilityBoard, sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [tileIdForTokenToMove].xPos, sb.sbm.boardList [tileIdForTokenToMove].yPos);
					contiguousTiles.Sort ();	

					if(contiguousTiles.Contains(tileIdForTokenToMoveTo))
					{
						evolvingPossibilityBoard[tileIdForTokenToMoveTo] = 0; // I make the new token position unavailable
						
						if(tileIdForTokenToMove != tileIdForTokenToMoveTo)
						{
							bubbleTileSelectionSequence.Add(new Vector2(tileIdForTokenToMove, tileIdForTokenToMoveTo));
						}
						else
						{
							cestPareille.Add(tileIdForTokenToMoveTo);
						}
						// stack the new board state
						movementSequenceShortTermMemory.Add(new List<int>(evolvingPossibilityBoard));
						
						isAbleToMoveToAllPositionsForThisPermutation &= 1;
					}
					else
					{
						isAbleToMoveToAllPositionsForThisPermutation &= 0;
						bubbleTileSelectionSequence.Clear();
						break;
					}
				}// end of looping through emptyTile with this permutation
				if(isAbleToMoveToAllPositionsForThisPermutation == 0)
				{
					continue; // continue to next permutation.
				}
				if(cestPareille.Count > 0)
				{
					for(int k = 0; k<cestPareille.Count; k++)// iterating through a tile permutation possibility
					{
						// can I make the following move?
						string ssssQ = "";
						for(int p=0; p<movementSequenceShortTermMemory.Last().Count; p++)
						{
							ssssQ += movementSequenceShortTermMemory.Last()[p];
						}
						
						List<int> tempTest = movementSequenceShortTermMemory.Last();
						tempTest[cestPareille[k]] = 1;
						
						List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (movementSequenceShortTermMemory.Last(), sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [cestPareille[k]].xPos, sb.sbm.boardList [cestPareille[k]].yPos);
						if(contiguousTiles.Count >= 1)
						{
							isAbleToMoveToAllPositionsForThisPermutation &= 0;
						}
					}
				}//end of c'est pareille
				
				if(isAbleToMoveToAllPositionsForThisPermutation == 1)
				{
					Debug.Log("YES!! isAbleToFormToThisPermutation");
					isAbleToFormToThisPermutation = true;
					break;
				}
			}// end of permutation

			if(isAbleToFormToThisPermutation)
			{
				if(containsTheBenchMovement)
				{
					TileSelectionSequence.InsertRange(0, bubbleTileSelectionSequence);
					bubbleTileSelectionSequence.Clear();
					benchTokenAdded = true;
				}
				else
				{
					TileSelectionSequence.AddRange(bubbleTileSelectionSequence);
					bubbleTileSelectionSequence.Clear();
				}
				numberOfpermutationThatWerePossible += 1;
			}
		}// end of possibility bubble
		currentAiProcess = AiProcesses.AiProcessCompleted;
		int numberOfMoves = (aiPt == PlayerVO.PlayerType.friend) ? sb.sbm.player1.currentTurnMoveLimit : sb.sbm.player2.currentTurnMoveLimit;
		return (TileSelectionSequence.Count == numberOfMoves);// ToDo: you still need a way to check that all permutation where possible.
	}

	void GenerateTurnSequence()
	{
		//--- Generate Possibility bubbles on the board ---
		List<List<int>> shortTermMemory = new List<List<int>>(); // goes through every moveable token and board with their moveable option, and have them stocked in the short term memory. This short term allows us to check how many possibility bubble there are.
		//shortTermMemory are an array of tile IDs.

		List<Tile> possibleMoveableTokensOnBoard = new List<Tile>(GetMoveableTokensList());
		Debug.Log ("Moveable Token: " + possibleMoveableTokensOnBoard.Count);
		List<Tile> nonMoveableTokensOnBoard = new List<Tile>(GetNonMoveableTokensList());
		Debug.Log ("None moveable Token: " + nonMoveableTokensOnBoard.Count);
		if(possibleMoveableTokensOnBoard.Count <= 0)
		{
			return;
		}
		
		for (int i = 0; i < possibleMoveableTokensOnBoard.Count; i++) 
		{
			shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveToIncludingTilesWithOwnToken (possibleMoveableTokensOnBoard[i].tileId)); //  goes through every moveable token and board with their moveable option
		}
		
		//		Debug.Log ("AI: " + possibleMoveableTokensOnBoard.Count + " " + shortTermMemory.Count);
		
//		string ps = "";
//		for (int j = 0; j < shortTermMemory.Count; j++) 
//		{
//			shortTermMemory[j].Sort ();	
//			for (int k = 0; k < shortTermMemory[j].Count; k++)
//			{
//				ps += shortTermMemory [j] [k] + ",";
//			}
//			ps += "\n";
//		}
//		Debug.Log (ps);
		
		List<Vector2> possibilityBubbles = new List<Vector2> (); // x is the index that references this bubble in shortTermMemory, y is the number of shortTermMemory that are the same to this index.
		//possibilityBubbles doesn't contain tile IDs
		possibilityBubbles.Add(new Vector2(0, 1));
		for (int l = 1; l < shortTermMemory.Count; l++) 
		{
			if (shortTermMemory [l - 1].All (shortTermMemory [l].Contains)) 
			{
				possibilityBubbles [possibilityBubbles.Count - 1] += new Vector2(0,1);
			} 
			else 
			{
				possibilityBubbles.Add (new Vector2 (l, 1));
			}
		}

		// -- end of generating possibility bubble.

//		for (int m = 0; (m < possibilityBubbles.Count); m++) 
//		{	
//			Debug.Log ("possibilityBubbles.Count: " + possibilityBubbles.Count + " " + possibilityBubbles[m].x + " " + possibilityBubbles[m].y);
//		}
		//Playing the possibility bubbles
		Debug.Log ("possibilityBubbles.Count: " + possibilityBubbles.Count);
		for (int m = 0; (m < possibilityBubbles.Count); m++) 
		{	
			List<Vector2> bubbleTileSelectionSequence = new List<Vector2>();
			Debug.Log ("possibilityBubbles: " + m);
			// Debug.Log ("possibilityBubbles: " + shortTermMemory[(int)possibilityBubbles [m].x/*the index in shortTermMemory containing the possibilit tiles array*/ ].Count /*tiles in this possibility bubble*/ + ":: " + possibilityBubbles [m].x + " " + possibilityBubbles [m].y /*number of tokens in this possibility bubble*/ + " = " + PossibilityCalculator.NumberOfPossibilities(shortTermMemory[(int)possibilityBubbles [m].x].Count, (int)possibilityBubbles [m].y));
			// m possibilityBubbles increment value. 
			// possibilityBubbles [m].x is the ID in the short term memory.
			// the shortTermMemory retains all the potential tiles for each token
			// PossibilityCalculator.NumberOfPermutationsWithOutRepetition(shortTermMemory[(int)possibilityBubbles [m].x].Count, (int)possibilityBubbles [m].y);
			
			char [] checkerForRedTile = new char[shortTermMemory[(int)possibilityBubbles [m].x].Count];
			for(int i = 0; i<checkerForRedTile.Length; i++)
			{
				if(sb.sbm.boardList[shortTermMemory[(int)possibilityBubbles [m].x][i]].currentTilePlayerType == aiPt)
				{
					checkerForRedTile[i] = 'r';
				}
				else
				{
					checkerForRedTile[i] = 'e';
				}
			}
			
			//--Start Permutation Validation ----
			//The number of ways of obtaining an ordered subset of r elements from a set of n elements.
			int n = shortTermMemory[(int)possibilityBubbles [m].x].Count;
			int r = (int)possibilityBubbles [m].y; //r are elements from a set of n elements.
			Debug.Log("Number of pieces to move in this bubble. " + r);
			
			char[] inputSet = new char[n];
			for (int i = 0; i < r; i++)
			{
				inputSet[i] = 't';
			}
			for (int i = r; i < n; i++)
			{
				inputSet[i] = 'e';
			}
			
			Permutations<char> P1 = new Permutations<char>(inputSet, 
			                                               GenerateOption.WithoutRepetition);
			string format1 = "Permutations of {{A A C}} without repetition; size = {0}";
			Debug.Log(String.Format(format1, P1.Count));
			
			//-- loop and check transformed tile to red. BASICALLY, check what permutation giver the highest heuristic score
			int mostAmountOfRedTiles = 0;
			int tempIncrement = 0;
			List<int> winnerPermutationIndex = new List<int>();
			for(int j=0; (j<P1.Count && j<100); j++)
			{
				IList<char> p = P1.ElementAt(j);
				tempIncrement = 0;
				for(int i = 0; i<p.Count; i++)
				{
					if(p[i] == 't' || checkerForRedTile[i] == 'r')
					{
						tempIncrement++;
					}
				}
				if(tempIncrement > mostAmountOfRedTiles)
				{
					mostAmountOfRedTiles = tempIncrement;
					winnerPermutationIndex.Add(j);
				}
			}
			for(int j=0; (j<P1.Count && j<100); j++)
			{
				if(!winnerPermutationIndex.Contains(j))
				{
					winnerPermutationIndex.Insert(0, j);
				}
			}

//			Debug.Log("winner: " + winnerPermutationIndex + " " + mostAmountOfRedTiles);
			/// End Permutation Validation ----
			/// winnerPermutationIndex is the permutation index that has the most red tiles from P1. 

			//This loop attempts to check if it is possible to perform the actual winnerPermutation
			for(int t = winnerPermutationIndex.Count-1; t>=0; t--)
			{
				// Now check what tile ID the winning permutation proposes.
				IList<char> winnerCharsWhereYouWantToMoveTo = P1.ElementAt(winnerPermutationIndex[t]); // winnerChars looks something like this: teeeeteeeeeteeettee

//				string sssJ = "";
//				for(int w=0; w<winnerCharsWhereYouWantToMoveTo.Count; w++)
//				{
//					sssJ += winnerCharsWhereYouWantToMoveTo[w];
//				}
//				Debug.Log("sssJ: " + sssJ);

				// create all the possible orders of the movement.

				// transform all the tokens into incremental values 1.2...5.... // This is done so they can be indexed and have various orders.
				char [] desiredTileIndexesToMoveTo = new char[r]; // r here is the number of tokens to place
				for(int i=0; i<r; i++)
				{
					string ss = "" + i;
					desiredTileIndexesToMoveTo[i] = ss[0];
//					Debug.Log(">"+i + " " + desiredTileIndexesToMoveTo[i]);
				}

				Permutations<char> permutationsForMovementOrder = new Permutations<char>(desiredTileIndexesToMoveTo);
				string format2 = "Permutations of {{1 2 3}} without repetition; size = {0}";
//				Debug.Log(String.Format(format2, permutationsForMovementOrder.Count));
				
				// now check if it's possible to create such a combination, through movement
//				foreach(IList<char> p in permutationsForMovementOrder) //this here is the order of movement permutations for the current available pieces.
//				{
//					string ssdb = "";
//					for(int j=0; j<p.Count; j++)
//					{
//						int bar = Convert.ToInt32(new string(p[j], 1));
//						ssdb += p[j];
//					}
//					Debug.Log("ssdb: " + ssdb);
//				}

				
				// take the order
				// have the pieces move to that order
				// have tile 1 go to the where the 1 is found in the permutations order.
				List<List<int>> movementSequenceShortTermMemory = new List<List<int>>();// movementSequenceShortTermMemory are snapshots of the sequence of what the board has to look like for this turn
				
				//I Need a representation of the board;
				List<int> currentPossibilityBoard = new List<int>();
				List<int> currentMoveablTokensInThisBubble = new List<int>();
				for(int i=0; i<sb.sbm.boardList.Count; i++)
				{
					currentPossibilityBoard.Add(0);
				}
				
				//make board from possibility bubble
				for(int i=0; i<shortTermMemory[(int)possibilityBubbles [m].x].Count; i++)
				{
					currentPossibilityBoard[shortTermMemory[(int)possibilityBubbles [m].x][i]] = 1;
				}

//				string ssssA = "";
//				for(int i=0; i<currentPossibilityBoard.Count; i++)
//				{
//					ssssA += currentPossibilityBoard[i];
//				}
//				Debug.Log("ssssA: " + ssssA);

				//remove the currently moveable tiles with tokens on them;
				for(int i=0; i<possibleMoveableTokensOnBoard.Count; i++)
				{
					if(currentPossibilityBoard[possibleMoveableTokensOnBoard[i].tileId] == 1)
					{
						currentMoveablTokensInThisBubble.Add(possibleMoveableTokensOnBoard[i].tileId);
					}
					currentPossibilityBoard[possibleMoveableTokensOnBoard[i].tileId] = 0;
				}
//				Debug.Log(sb.sbm.ListsToStrings(currentPossibilityBoard));


				// remove any tiles with nonmoveable tokens on them.
				for(int i=0; i<nonMoveableTokensOnBoard.Count; i++)
				{
					currentPossibilityBoard[nonMoveableTokensOnBoard[i].tileId] = 0;
				}


//				string ssssB = "";
//				for(int i=0; i<currentPossibilityBoard.Count; i++)
//				{
//					ssssB += currentPossibilityBoard[i];
//				}
//				Debug.Log("ssssB: " + ssssB);

				movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));
				
				int isAbleToFormToThisPermutation = 1;

				int pass_tileIdForTokenToMove = 0;
				int pass_tileIdForTokenToMoveTo = 0;

				string ssssK = "";
				for(int i = 0; i<winnerCharsWhereYouWantToMoveTo.Count; i++)// iterating through a tile permutation possibility
				{
					if(winnerCharsWhereYouWantToMoveTo[i] == 't')// this is the space to move to from the winning combination. Looks like 'eteet'
					{
						ssssK += shortTermMemory[(int)possibilityBubbles [m].x][i] + ", ";
					}
				}
				Debug.Log(t + " ssssK Objective: " + ssssK);

				for(int q = 0; q<permutationsForMovementOrder.Count; q++)
				{
					IList<char> permutationBeingVerified = permutationsForMovementOrder.ElementAt(q);
					int permutationsIncrement = 0;
					
					int isAbleToMoveToAllPositions = 1;
					List<int> cestPareille = new List<int>();

					for(int i = 0; i<winnerCharsWhereYouWantToMoveTo.Count; i++)// iterating through a tile permutation possibility
					{
						if(winnerCharsWhereYouWantToMoveTo[i] == 't')// this is the space to move to from the winning combination. Looks like 'eteet'
						{
//							string ssssE = "";
//							for(int u=0; u<currentMoveablTokensInThisBubble.Count; u++)
//							{
//								ssssE += currentMoveablTokensInThisBubble[u] + ", ";
//							}
//							Debug.Log("ssssE " + i + "currentMoveablTokensInThisBubble: " + ssssE);

							//	Debug.Log("asdfasdf: " + possibleMoveableTokensOnBoard.Count + " " + permutationBeingVerified.Count + " " + (int)(permutationBeingVerified[permutationsIncrement] - 48));
							int tileIdForTokenToMove = currentMoveablTokensInThisBubble[(int)(permutationBeingVerified[permutationsIncrement] - 48)];// 48 is how to convert char values to int values
							// permutationBeingVerified length is exactly the same length as the currently available tokens that can be moved on the board.
							permutationsIncrement++;
							int tileIdForTokenToMoveTo = shortTermMemory[(int)possibilityBubbles [m].x][i];

							//include the currently selected tile for the contiguous search
							currentPossibilityBoard[tileIdForTokenToMove] = 1;
								
//							string ssssC = "";
//							for(int p=0; p<currentPossibilityBoard.Count; p++)
//							{
//								ssssC += currentPossibilityBoard[p];
//							}
//							Debug.Log("ssssC" + i + ": " + ssssC);
							
							// can I make the following move?
							List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (currentPossibilityBoard, sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [tileIdForTokenToMove].xPos, sb.sbm.boardList [tileIdForTokenToMove].yPos);

//							string ssssD = "";
//							for(int u=0; u<contiguousTiles.Count; u++)
//							{
//								ssssD += contiguousTiles[u] + ", ";
//							}
//							Debug.Log("ssssD" + i + ": " + ssssD);

							if(contiguousTiles.Contains(tileIdForTokenToMoveTo))
							{
								currentPossibilityBoard[tileIdForTokenToMoveTo] = 0; // I make the new token position unavailable

								if(tileIdForTokenToMove != tileIdForTokenToMoveTo)
								{
									bubbleTileSelectionSequence.Add(new Vector2(tileIdForTokenToMove, tileIdForTokenToMoveTo));
								}
								else
								{
									cestPareille.Add(tileIdForTokenToMoveTo);
//									Debug.Log ("C'est Pareille!!!! " + tileIdForTokenToMoveTo);
								}

//								string ssssF = "";
//								for(int p=0; p<currentPossibilityBoard.Count; p++)
//								{
//									ssssF += currentPossibilityBoard[p];
//								}
//								Debug.Log("ssssF" + i + ": " + ssssF);

			                    // stack the new board state
			                    movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));

								isAbleToMoveToAllPositions &= 1;
			                }
			                else
							{
								pass_tileIdForTokenToMove = tileIdForTokenToMove;
								pass_tileIdForTokenToMoveTo = tileIdForTokenToMoveTo;
								isAbleToMoveToAllPositions &= 0;
								bubbleTileSelectionSequence.Clear();
								break;
							}
						}
					}

					if(cestPareille.Count > 0)
					{
//						Debug.Log("cestPareille.Count): " + cestPareille.Count);
						for(int i = 0; i<cestPareille.Count; i++)// iterating through a tile permutation possibility
						{
							// can I make the following move?
							string ssssQ = "";
							for(int p=0; p<movementSequenceShortTermMemory.Last().Count; p++)
							{
								ssssQ += movementSequenceShortTermMemory.Last()[p];
							}

							List<int> tempTest = movementSequenceShortTermMemory.Last();
							tempTest[cestPareille[i]] = 1;

							List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (movementSequenceShortTermMemory.Last(), sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [cestPareille[i]].xPos, sb.sbm.boardList [cestPareille[i]].yPos);
							if(contiguousTiles.Count >= 1)
							{
								isAbleToMoveToAllPositions &= 0;
							}
//							Debug.Log("ssssQ " + " " + contiguousTiles.Count + " " + cestPareille[i] + " " + ssssQ);
						}

					}

					if(isAbleToMoveToAllPositions == 1)
					{
						Debug.Log("YES!! isAbleToFormToThisPermutation");
						isAbleToFormToThisPermutation &= 1;
						break;
					}
					else
					{
//						Debug.Log("NOOO WHY?!?! isAbleToFormToThisPermutation" + ": " + pass_tileIdForTokenToMove + ", " + pass_tileIdForTokenToMoveTo + ", " + winnerCharsWhereYouWantToMoveTo.Count);
//						Debug.Log("NO");
						isAbleToFormToThisPermutation &= 0;
					}
				}

				if(isAbleToFormToThisPermutation == 1)
				{
					break;
				}
				else
				{
//					string ssssG = "";
//					for(int p=0; p<winnerPermutationIndex.Count; p++)
//					{
//						ssssG += winnerPermutationIndex[p];
//					}
//					Debug.Log("FAILED!!!! " + t + " " + winnerPermutationIndex.Count + ": " + ssssG);
//					Debug.Log(t + " ---------------FAILED!!!!---------------");
				}
			}
			TileSelectionSequence.AddRange(bubbleTileSelectionSequence);
		}
		currentAiProcess = AiProcesses.AiProcessCompleted;
	}

	int convert6x6GridIndexToA8x8GridIndex(int index66)// i don't want to have to do this.
	{
		int howManyRows = (int)Mathf.Floor ((float)index66 / 6f);
		int colomns6x6 = index66 - (howManyRows * 6);
		int colomns8x8 = colomns6x6 + 1;
		int value = 8 + howManyRows*8 + colomns8x8;
//		Debug.Log ("conversions: " + index66 + " " + value);
		return value;
	}
}
