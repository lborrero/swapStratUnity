using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Facet.Combinatorics;
using System.Text;
using System;
using System.Timers;

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

	sGameManager.TurnLoop previousTurnAction = sGameManager.TurnLoop.endLoopTurn;
	bool checkIfTryingToTakeTheSameAction(sGameManager.TurnLoop currentTurnAction)
	{
		bool returnValue = false;
		if(previousTurnAction == currentTurnAction)
		{
			DestroyTurnSequenceAll();
			returnValue = true;
		}
//		Debug.Log("Checking " + returnValue);
		return returnValue;
	}

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
//					Debug.Log("timeBomb2");
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
//					Debug.Log("timeBomb1");
					ThinkLoop ("asdf");
					takeDecision = false;
					StartCoroutine(GenerateEvent());
				}
			}
		}
	}

	void GenerateHearthStoneSequence()
	{
		if (!playFinalizationSequence) 
		{
			List<Tile> possibleTiles = GetCurrentlyEmptyTiles ();
			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes (possibleTiles);
			int numberOfMoves = (aiPt == PlayerVO.PlayerType.friend) ? (sb.sbm.player1.currentTurnMoveCount) : (sb.sbm.player2.currentTurnMoveCount);
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

		switch(ait)
		{
			case AiType.intermediate:
			{
				break;
			}
			case AiType.random:
			{
				break;
			} 
			case AiType.hearthstone:
			{
				GenerateHearthStoneSequence ();
				break;
			}
		}
		checkIfTryingToTakeTheSameAction(sb.sgm.currentTurnLoop);
		switch (sb.sgm.currentTurnLoop) 
		{
		case sGameManager.TurnLoop.selectATokenFromBench:
		{
			previousTurnAction = sGameManager.TurnLoop.selectATokenFromBench;
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
			previousTurnAction = sGameManager.TurnLoop.placeSelectedTokenFromBench;
			switch(ait)
			{
				case AiType.intermediate:
				{
					List<Tile> possibleTiles = GetCurrentlyEmptyTiles();
					System.Random rnd = new System.Random();
					sb.sbm.TileClicked(possibleTiles[rnd.Next(0, possibleTiles.Count-1)].tileId);

					TileSelectionSequence.Clear();
//					Debug.Log("GenerateTurnSequence: " + TileSelectionSequence.Count + " " + currentAiProcess);
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
//					Debug.Log("--------placeSelectedTokenFromBench");

					
					if(playFinalizationSequence)
					{
						if(sb.sbm.TileClicked((int)TileSelectionSequence[0].y))
						{
							DestroyTurnSequenceStep0();
						}
						else
						{
							DestroyTurnSequenceAll();
						}
					}
					else
					{
						List<Tile> possibleTiles = GetCurrentlyEmptyTiles();
						HearthStone_PlayUntilFold(GivePossibleTilesInOrderOfHeuristic(possibleTiles));
					}
				break;
				}
			}
			break;
		}
		case sGameManager.TurnLoop.selectATokenFromBoard:
		{
			previousTurnAction = sGameManager.TurnLoop.selectATokenFromBoard;
			switch(ait)
			{
				case AiType.intermediate:
				{
					if(currentAiProcess == AiProcesses.AiProcessCompleted)
					{
					if(TileSelectionSequence.Count> 0 && (int)TileSelectionSequence[0].x != -1)
					{
//						Debug.Log("AI selectATokenFromBoard" + (int)TileSelectionSequence[0].x);
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
//				Debug.Log("--------selectATokenFromBoard");
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
					//Here, because there's no possible finish, we are going to check which token should be selected to move.
					//For this, we want to check which token has the best move option. We do that by checking it's movement bubble.
					List<Tile> possibleTokensOnTile = GetMoveableTokensList();

					/*
					 * shortTermMemory represents a list of the space bubbles for each token that can be moved in the game 
					 * as an array of the id's composing those bubbles.
					 */
					List<List<int>> shortTermMemory = HearthStone_generateThePossiblityMovement(possibleTokensOnTile);

					int theTileIdWhereTheTokenToMoveIs = 0;
					int heuristic = -1;
					int tempHeuristic = -1;

					//let's loop through each tokens moveable space, and see which one has the best moves.
					for(int i = 0; i<shortTermMemory.Count; i++)
					{
						// take that space bubble and give them back to me the real tiles with their actual info.
						List<Tile> returnValueB = new List<Tile>();
						for(int j = 0; j<shortTermMemory[i].Count; j++)
						{
							for(int k = 0; k<sb.sbm.boardList.Count; k++)
							{
								if(shortTermMemory[i][j] == sb.sbm.boardList[k].tileId)//sb.sbm.boardList[k] represents the tiles actual info as Tile class.
								{
									returnValueB.Add(sb.sbm.boardList[k]);//the selected actual tiles are grouped in a list called returnValueB.
								}
							}
						}
						tempHeuristic = Evaluate_HearthStone_HeuristicForPossibilityTypes(HearthStone_ParsePossibilitesIntoTypes(returnValueB));
						
						
						//let's analyze which token you should move first by analyzing it's moveable space.
						//the better it's space offers, the higher the heuristic
						//the higher the heuristic, the better chance it has for being moved selected to move first.
//						tempHeuristic = Evaluate_HearthStone_HeuristicForPossibilityTypes(returnValueB);
//								Debug.Log ("returnValueB: " + returnValueB.Count);
						if(heuristic < tempHeuristic)
						{		
//									Debug.Log ("tempHeuristic: " + tempHeuristic);
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
			previousTurnAction = sGameManager.TurnLoop.moveSelectedToken;
			switch(ait)
			{
				case AiType.intermediate:
				{
				if(currentAiProcess == AiProcesses.AiProcessCompleted)
				{
					if(TileSelectionSequence.Count > 0)
					{
//						Debug.Log("AI moveSelectedToken: " + (int)TileSelectionSequence[0].y);
						if(sb.sbm.TileClicked((int)TileSelectionSequence[0].y))// place token on this tile id is found in the y value of the vector 2
						{
						DestroyTurnSequenceStep0();
						}
						else
						{
							DestroyTurnSequenceAll();
						}
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
	//				Debug.Log("--------moveSelectedToken");
					if(playFinalizationSequence)
					{
						if(TileSelectionSequence.Count > 0)
						{
							if(sb.sbm.TileClicked((int)TileSelectionSequence[0].y))
							{
								DestroyTurnSequenceStep0();
							}
							else
							{
								DestroyTurnSequenceAll();
								MoveSelectedPieceHearthstoneStyle(sb.sbm.currentlySelectedTile.tileId);
							}
						}
						else
						{
							//do this if no finalize sequence is available.		
							List<int> imediatePossibleTiles = sb.sbm.getTilesIdToMoveTo ();
							
							System.Random rnd = new System.Random();
							int returnedValue = imediatePossibleTiles[rnd.Next(0, imediatePossibleTiles.Count)];
							sb.sbm.TileClicked(returnedValue);
						}
					}
					else
					{
						MoveSelectedPieceHearthstoneStyle((int)TileSelectionSequence[0].x);
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

	void MoveSelectedPieceHearthstoneStyle(int currentlySelectedTokenTileId)
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		
		List<int> shortTermMemory = sb.sbm.getPotentialTilesIdToMoveToExcludingTheTileTheTokenIsIn (currentlySelectedTokenTileId);

		//transform a list of tile ids into a list of actual tiles from the board. (ToDo: make a general function of this, it get's used more than once).
		List<Tile> possibleTiles = new List<Tile>();
		for(int k = 0; k<sb.sbm.boardList.Count; k++)
		{
			for(int j = 0; j<shortTermMemory.Count; j++)
			{
				if(shortTermMemory[j] == sb.sbm.boardList[k].tileId)
				{
					possibleTiles.Add(sb.sbm.boardList[k]);//possibleTiles is the group of actual tiles from the board to which we can move to.
				}
			}
		}

		HearthStone_PlayUntilFold(GivePossibleTilesInOrderOfHeuristic (possibleTiles));
		TileSelectionSequence.Clear();
	}

	List<List<int>> GivePossibleTilesInOrderOfHeuristic(List<Tile> possibleTiles)
	{
		float[] heuristicValueForEachPossibleTileSpace = new float[possibleTiles.Count];
		int[] possibleTileIds = new int[possibleTiles.Count];
		Debug.Log ("-----");
		for (int m = 0; m < heuristicValueForEachPossibleTileSpace.Length; m++) 
		{
			float tempHeuristic = 0;
			tempHeuristic += (possibleTiles [m].currentTilePlayerType != aiPt && possibleTiles [m].currentGuardState == Tile.TileGuarded.guarded) ? (-1) : 0;
			tempHeuristic += (possibleTiles [m].currentTilePlayerType != aiPt && possibleTiles [m].currentGuardState == Tile.TileGuarded.taken) ? 2 : 0;
			//check match for almost guarded tiles.
			tempHeuristic += (sb.sbm.GiveMeAlmostGuardedTiles ().Exists(x => x.tileId == possibleTiles[m].tileId))?0.5f:0;
			tempHeuristic += (possibleTiles [m].currentTilePlayerType == PlayerVO.PlayerType.none) ? 1 : 0;
			tempHeuristic += (possibleTiles [m].currentTilePlayerType == aiPt && possibleTiles [m].currentGuardState == Tile.TileGuarded.taken) ? 0 : 0;
			tempHeuristic += (possibleTiles [m].currentTilePlayerType == aiPt && possibleTiles [m].currentGuardState == Tile.TileGuarded.guarded) ? (-1) : 0;
			Debug.Log ("heuristicValue: " + tempHeuristic);
			heuristicValueForEachPossibleTileSpace[m] = tempHeuristic;
			possibleTileIds [m] = possibleTiles [m].tileId;
		}


		//sorting them with highest heuristic
		Array.Sort(heuristicValueForEachPossibleTileSpace, possibleTileIds);
		Array.Reverse(heuristicValueForEachPossibleTileSpace);
		Array.Reverse(possibleTileIds);

		//tranform tiles array into an tileID array
		List<List<int>> segmentedIds = new List<List<int>>();
		float currentHeuristicValue = 99999;
		int currentsegmentIndex = -1;
		for (int i = 0; i < possibleTileIds.Length; i++) 
		{
			if (currentHeuristicValue != heuristicValueForEachPossibleTileSpace [i]) 
			{
				currentHeuristicValue = heuristicValueForEachPossibleTileSpace [i];
				segmentedIds.Add (new List<int> ());
				currentsegmentIndex++;
			}
			segmentedIds [currentsegmentIndex].Add (possibleTileIds [i]);
			//			Debug.Log ("Segment: " + currentsegmentIndex + " value: " + heuristicValueForEachPossibleTileSpace [i]);
		}

		return segmentedIds;
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

	List<Tile> GetDisabledTokensList()
	{
		List<Tile> possibleTokensOnTile = new List<Tile>();
		Token tmptoken;
		for(int i = 0; i<sb.sbm.boardList.Count; i++)
		{
			if (sb.sbm.boardList [i].currentTileType == Tile.TileType.occupied &&
			    sb.sbm.boardList [i].occupyingTokenPlayerType == aiPt) 
			{
				tmptoken = sb.sbm.getTokenFromTokenListWithIdAndType (sb.sbm.boardList [i].occupyingTokenId, sb.sbm.boardList [i].occupyingTokenPlayerType);
				if (tmptoken.currentTokenState == Token.TokenState.disabled)
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
//		Debug.Log ("clicks"  +TileSelectionSequence.Count + ": " + TileSelectionSequence.First().x + "," + TileSelectionSequence.First().y);
		TileSelectionSequence.RemoveAt (0);
		if(TileSelectionSequence.Count <= 0)
		{
			playFinalizationSequence = false;
		}
	}

	void DestroyTurnSequenceAll()
	{
//		Debug.Log ("DESTROY!");
		TileSelectionSequence.Clear ();
		playFinalizationSequence = false;
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

//	int Evaluate_HearthStone_HeuristicForPossibilityTypes(List<Tile> input)
	int Evaluate_HearthStone_HeuristicForPossibilityTypes(List<List<int>> input)
	{
		//here we are returning a heuristic to inform what space offers the best moves.
		int returnValue = 0;
//		Debug.Log("1): " + input[0].Count);
		if(input[0].Count > 0)
		{
			if(returnValue < ((aiPt == PlayerVO.PlayerType.enemy)?0:2))
			{
				returnValue = ((aiPt == PlayerVO.PlayerType.enemy)?0:2);
			}
		}

//		Debug.Log("2): " + input[1].Count);
		if(input[1].Count > 0)
		{
//			Debug.Log("b: " + 1);
			if(returnValue < 1)
			{
				returnValue = 1;
			}
		}

//		Debug.Log("3): " + input[2].Count);
		if(input[2].Count > 0)
		{
//			Debug.Log("c: " + ((aiPt == PlayerVO.PlayerType.enemy)?2:0));
			if(returnValue < ((aiPt == PlayerVO.PlayerType.enemy)?2:0))
			{
				returnValue = ((aiPt == PlayerVO.PlayerType.enemy)?2:0);
			}
		}

//		List<List<int>> data = new List<List<int>> ();
//		data = HearthStone_ParsePossibilitesIntoTypes(input);
//		//add the enemy tiles that can be changed and give them a 2 multiplier as a heuristic.
//		returnValue += (data [(aiPt == PlayerVO.PlayerType.enemy)?0:2].Count/*all enemy Tiles*/ - data [(aiPt == PlayerVO.PlayerType.enemy)?3:4].Count/*all enemy tile that are guarded*/) * 2/*a positive heuristic because it's better to remove enemy tiles.*/;
//		//add the empty tiles with a 1 multiplier as a heuristic.
//		returnValue += data [1].Count/*all neutral Tiles*/ * 1/*a neutral heuristic value for empty tiles.*/;

//		Debug.Log ("movement:" + input.Count + "; heuristic: " + returnValue);
		//total heuristic value for this pocket bubble.
		return returnValue;
	}

	List<List<int>> HearthStone_ParsePossibilitesIntoTypes(List<Tile> possibleTilesToPlayOn)
	{
		List<List<int>> returnValue = new List<List<int>> ();
		List<int> pointCountingEnemy = new List<int>(); //these are tileID
		List<int> pointCountingNone = new List<int>(); //these are tileID
		List<int> pointCountingFriend = new List<int>(); //these are tileID
		List<int> pointCountingEnemyThatAreGuarded = new List<int>(); //these are tileID
		List<int> pointCountingFriendThatAreGuarded = new List<int>(); //these are tileID
		for(int i = 0; i<possibleTilesToPlayOn.Count; i++)
		{
			switch(possibleTilesToPlayOn[i].currentTilePlayerType)
			{
			case PlayerVO.PlayerType.enemy:
				pointCountingEnemy.Add (possibleTilesToPlayOn [i].tileId);
				if (possibleTilesToPlayOn [i].currentGuardState == Tile.TileGuarded.guarded) 
				{
					pointCountingEnemyThatAreGuarded.Add (possibleTilesToPlayOn [i].tileId);
				}
				break;
			case PlayerVO.PlayerType.none:
				pointCountingNone.Add(possibleTilesToPlayOn[i].tileId);
				break;
			case PlayerVO.PlayerType.friend:
				pointCountingFriend.Add(possibleTilesToPlayOn[i].tileId);
				if (possibleTilesToPlayOn [i].currentGuardState == Tile.TileGuarded.guarded) 
				{
					pointCountingFriendThatAreGuarded.Add (possibleTilesToPlayOn [i].tileId);
				}
				break;
			}
		}
//		Debug.Log ("pointCountingEnemy: " + pointCountingEnemy.Count);
//		Debug.Log ("pointCountingNone: " + pointCountingNone.Count);
//		Debug.Log ("pointCountingFriend: " + pointCountingFriend.Count);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingFriend:pointCountingEnemy);
		returnValue.Add (pointCountingNone);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingEnemy:pointCountingFriend);

//		Debug.Log ("pointCountingEnemyThatAreGuarded: " + pointCountingEnemyThatAreGuarded.Count);
//		Debug.Log ("pointCountingFriendThatAreGuarded: " + pointCountingFriendThatAreGuarded.Count);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingEnemyThatAreGuarded:pointCountingFriendThatAreGuarded);
		returnValue.Add ((aiPt == PlayerVO.PlayerType.enemy)?pointCountingFriendThatAreGuarded:pointCountingEnemyThatAreGuarded);
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
					if(sb.sbm.TileClicked(playList[j][thisRnd]))//TileClicked returns true if the play can be made.
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
		for(int k=0; k<excludeFromPossibility.Count; k++)
		{
			currentPossibilityBoard[excludeFromPossibility[k].tileId] = 0;
		}
		return currentPossibilityBoard;
	}
	
	bool FinaliseMatch()
	{
		int numberOfEmptyTilesFilled = 0;
		List<PossibilityTiles> possibilityBubbleWithTiles = GeneratePossibilitiyBubbles ();
		List<Tile> tokenPositionOnBoard = new List<Tile>(GetMoveableTokensListEvenIfCurrentlyTheyCantMove());
//		tokenPositionOnBoard.AddRange (GetDisabledTokensList ());

		bool doesPlayerHaveTokensToMoveOnBench = (((aiPt == PlayerVO.PlayerType.friend)?sb.sbm.player1.HasAvailableTokensOnBench():sb.sbm.player2.HasAvailableTokensOnBench()));

		bool benchTokenAdded = false;

		for(int i=0; i<possibilityBubbleWithTiles.Count; i++)
		{
			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes(possibilityBubbleWithTiles[i].possibilities);
			List<int> tokenOrder = GenerateTokensOrderForBubble(emptyTilesIDsSplitByType, possibilityBubbleWithTiles[i], doesPlayerHaveTokensToMoveOnBench, benchTokenAdded, i == possibilityBubbleWithTiles.Count-1);
			List<int> currentPossibilityBoard = RepresentBoardInBinary(possibilityBubbleWithTiles[i].possibilities, tokenPositionOnBoard);

			List<int> solution = new List<int>();
			List<int> emptyTilesIds = new List<int>(emptyTilesIDsSplitByType[1]);
			if(emptyTilesIds.Count > 0)
			{
				// should be implemented as a queue.Enqueue(queue.Dequeue()) for better premormance, but that is someday.
				for(int j = 0; j<emptyTilesIds.Count; j++)
				{
					int last = emptyTilesIds.Last();
					emptyTilesIds.Insert(0, last);
					emptyTilesIds.RemoveAt(emptyTilesIds.Count-1);
					solution = GenerateMovementSequenceToFinalizeForBubble(tokenOrder, currentPossibilityBoard, possibilityBubbleWithTiles[i], emptyTilesIds);
					if(solution.Count > 0)
					{
						break;
					}
				}
			}
			//DoesSequenceHaveHangingTokensToMove();

			if(solution.Contains(-1))
			{
				TileSelectionSequence.InsertRange(0, TransformSolutionToSequence(solution, possibilityBubbleWithTiles[i], emptyTilesIDsSplitByType[1]));
				benchTokenAdded = true;
			}
			else
			{
				TileSelectionSequence.AddRange(TransformSolutionToSequence(solution, possibilityBubbleWithTiles[i], emptyTilesIDsSplitByType[1]));
			}

			numberOfEmptyTilesFilled += emptyTilesIDsSplitByType[1].Count;
		}// end of possibility bubble
		currentAiProcess = AiProcesses.AiProcessCompleted;
		return (TileSelectionSequence.Count >= numberOfEmptyTilesFilled);// ToDo: you still need a way to check that all permutation where possible.
	}

	List<int> GenerateMovementSequenceToFinalizeForBubble(List<int> listOrder, List<int> currentPossibilityBoard, PossibilityTiles possibilityBubbleWithTiles, List<int> emptyNeutralTilesForBubble)
	{
		bool canGenerate = false;
		List<int> solutionPermutation = new List<int>();
		solutionPermutation.Add (int.MinValue);
		List<List<int>> movementSequenceShortTermMemory = new List<List<int>>();// movementSequenceShortTermMemory are snapshots of the sequence of what the board has to look like for this turn
		movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));
		List<int> cestPareille = new List<int>();
		
		int indexWeAreChecking = 0;
		bool isConcluded = false;
		while(!isConcluded)
		{
			//canGiveAMatch
			int? tokenIndex = (int?)ReturnNextPossibleMoveableToken((solutionPermutation[indexWeAreChecking] == int.MinValue)? listOrder.First():solutionPermutation[indexWeAreChecking], 
			                                                 possibilityBubbleWithTiles.tokensForThisPossibilitySpace, 
			                                                 emptyNeutralTilesForBubble[indexWeAreChecking],
			                                                 new List<int>(movementSequenceShortTermMemory.Last()),
			                                                 solutionPermutation);

			if(tokenIndex != null)
			{ 
				List<int> evolvingPossibilityBoard = movementSequenceShortTermMemory.Last();

				solutionPermutation[indexWeAreChecking] = (int)tokenIndex;
				if(tokenIndex != -1)
				{
					evolvingPossibilityBoard[possibilityBubbleWithTiles.tokensForThisPossibilitySpace[(int)tokenIndex].tileId] = 1;
				}
				evolvingPossibilityBoard[emptyNeutralTilesForBubble[indexWeAreChecking]] = 0;
				movementSequenceShortTermMemory.Add(new List<int>(evolvingPossibilityBoard));
				indexWeAreChecking ++;
				if(indexWeAreChecking >= emptyNeutralTilesForBubble.Count)
				{
					//done finding a solution.
					isConcluded = true;
					canGenerate = true;
				}
				else
				{
					solutionPermutation.Add (int.MinValue);
				}
			}
			else
			{
				// failed
				solutionPermutation.RemoveAt(solutionPermutation.Count-1);
				movementSequenceShortTermMemory.RemoveAt(movementSequenceShortTermMemory.Count-1);
				indexWeAreChecking --; 
				if(indexWeAreChecking < 0)
				{
					//can't perform this at all.
					isConcluded = true;
				}
				continue;
			}
		}

		if(canGenerate)
		{
		}

		return solutionPermutation;
	}

	List<Vector2> TransformSolutionToSequence(List<int> solution, PossibilityTiles possibilityBubbleWithTiles, List<int> emptyNeutralTilesForBubble)
	{
		List<Vector2> sequence = new List<Vector2> ();
		for(int i =0; i<solution.Count; i++)
		{
			int tileToMove = (solution[i] == -1)?solution[i]:possibilityBubbleWithTiles.tokensForThisPossibilitySpace[solution[i]].tileId;
			int tileTotMoveTo = emptyNeutralTilesForBubble[i];
			sequence.Add(new Vector2(tileToMove, tileTotMoveTo));
		}
		return sequence;
	}

	int? ReturnNextPossibleMoveableToken(int startOfPoint, List<Tile> moveableTokens, int theTileIdToMoveTo, List<int> evolvingPossibilityBoard, List<int> tokenOrderThatAreAlreadyBeingPlayed)
	{
		int? returnValue = null;
		if(startOfPoint == -1 &&
		   !tokenOrderThatAreAlreadyBeingPlayed.Contains(-1))
		{
				returnValue = -1;
		}
		else
		{
			if(startOfPoint == -1)
			{
				startOfPoint = 0;
			}
			for(int i = startOfPoint; i<moveableTokens.Count; i++)
			{
				if(!tokenOrderThatAreAlreadyBeingPlayed.Contains(i) &&
					CanTokenMoveToTile(moveableTokens[i].tileId, theTileIdToMoveTo, evolvingPossibilityBoard))
				{
					returnValue = i;
					break;
				}
			}
		}
		return returnValue;
	}

	bool CestPareilleCantMoveAtTheEnd(List<int> cestPareille, List<int> possibilityBoard)
	{
		bool noTokensCanMove = true;
		for(int k = 0; k<cestPareille.Count; k++)// iterating through a tile permutation possibility
		{
			// can I make the following move?
			List<int> tempTest = possibilityBoard;
			tempTest[cestPareille[k]] = 1;
			
			List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (possibilityBoard, sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [cestPareille[k]].xPos, sb.sbm.boardList [cestPareille[k]].yPos);
			if(contiguousTiles.Count >= 1)
			{
				noTokensCanMove = false;
			}
		}
		return noTokensCanMove;
	}

	bool CanTokenMoveToTile(int tileIdForTokenToMove, int tileIdForTokenToMoveTo, List<int> evolvingPossibilityBoard)
	{
		//include the currently selected tile for the contiguous search
		evolvingPossibilityBoard[tileIdForTokenToMove] = 1;
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (evolvingPossibilityBoard, sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [tileIdForTokenToMove].xPos, sb.sbm.boardList [tileIdForTokenToMove].yPos);
		contiguousTiles.Sort ();		
		return(contiguousTiles.Contains (tileIdForTokenToMoveTo));
	}

	List<int> GetMoveableTokensTileIdsInThisBubble(List<Tile> possibleMoveableTokensOnBoard, List<int> currentPossibilityBoard)
	{
		List<int> currentMoveablTokensInThisBubble = new List<int> ();
		for(int k=0; k<possibleMoveableTokensOnBoard.Count; k++)
		{
			if(currentPossibilityBoard[possibleMoveableTokensOnBoard[k].tileId] == 1)
			{
				currentMoveablTokensInThisBubble.Add(possibleMoveableTokensOnBoard[k].tileId);
			}
		}
		return currentMoveablTokensInThisBubble;
	}

	List<int> GenerateTokensOrderForBubble(List<List<int>> emptyTilesIDsSplitByType, PossibilityTiles possibilityTiles, bool doesPlayerHaveTokensToMoveOnBench, bool benchTokenAdded, bool hasReachedLastBubble)
	{
		bool addBenchTokenInSequence = false;
		int numberOfTokensThePlayerHasForThisPossibilitySpace = possibilityTiles.numberOfTokensThatHaveThisSamePossibilityTiles;

		if(!benchTokenAdded &&
		   emptyTilesIDsSplitByType[1].Count >= numberOfTokensThePlayerHasForThisPossibilitySpace &&
		   doesPlayerHaveTokensToMoveOnBench)
		{
			addBenchTokenInSequence = true;
			numberOfTokensThePlayerHasForThisPossibilitySpace += 1;
		}
		if(!benchTokenAdded &&
		   hasReachedLastBubble /*i == possibilityBubbleWithTiles.Count-1*/ &&
		   doesPlayerHaveTokensToMoveOnBench && !addBenchTokenInSequence)
		{
			addBenchTokenInSequence = true;
			numberOfTokensThePlayerHasForThisPossibilitySpace += 1;
		}
		
		int r = possibilityTiles.numberOfTokensThatHaveThisSamePossibilityTiles + ((addBenchTokenInSequence)?1:0);
		List<int> tokenOrder = new List<int>();
		for(int j=0; j<r; j++)
		{
			tokenOrder.Add ((j + ((addBenchTokenInSequence)?-1:0)));
		}
		
		return tokenOrder;
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
