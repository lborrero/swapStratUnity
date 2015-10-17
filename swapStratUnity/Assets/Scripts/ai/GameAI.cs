using UnityEngine;
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
		for(int k=0; k<excludeFromPossibility.Count; k++)
		{
			currentPossibilityBoard[excludeFromPossibility[k].tileId] = 0;
		}
		return currentPossibilityBoard;
	}
	
	bool FinaliseMatch()
	{
		int numberOfpermutationThatWerePossible = 0;
		List<PossibilityTiles> possibilityBubbleWithTiles = GeneratePossibilitiyBubbles ();
		List<Tile> possibleMoveableTokensOnBoard = new List<Tile>(GetMoveableTokensList());

		bool doesPlayerHaveTokensToMoveOnBench = (((aiPt == PlayerVO.PlayerType.friend)?sb.sbm.player1.HasAvailableTokensOnBench():sb.sbm.player2.HasAvailableTokensOnBench()));

		bool benchTokenAdded = false;

		for(int i=0; i<possibilityBubbleWithTiles.Count; i++)
		{

			List<List<int>> emptyTilesIDsSplitByType = HearthStone_ParsePossibilitesIntoTypes(possibilityBubbleWithTiles[i].possibilities);
			List<int> tokenOrder = GenerateTokensOrderForBubble(emptyTilesIDsSplitByType, possibilityBubbleWithTiles[i], doesPlayerHaveTokensToMoveOnBench, benchTokenAdded, i == possibilityBubbleWithTiles.Count-1);
			List<int> currentPossibilityBoard = RepresentBoardInBinary(possibilityBubbleWithTiles[i].possibilities, possibleMoveableTokensOnBoard);

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
					solution = GenerateMovementSequenceToFinalizeForBubble(tokenOrder, currentPossibilityBoard, possibilityBubbleWithTiles[i], emptyTilesIDsSplitByType[1]);
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
			numberOfpermutationThatWerePossible += 1;
		}// end of possibility bubble
		currentAiProcess = AiProcesses.AiProcessCompleted;
		int numberOfMoves = (aiPt == PlayerVO.PlayerType.friend) ? sb.sbm.player1.currentTurnMoveLimit : sb.sbm.player2.currentTurnMoveLimit;
		return (TileSelectionSequence.Count == numberOfMoves);// ToDo: you still need a way to check that all permutation where possible.
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
			int? tokenIndex = ReturnNextPossibleMoveableToken((solutionPermutation[indexWeAreChecking] == int.MinValue)? listOrder.First():solutionPermutation[indexWeAreChecking], 
			                                                 possibilityBubbleWithTiles.tokensForThisPossibilitySpace, 
			                                                 emptyNeutralTilesForBubble[indexWeAreChecking],
			                                                 movementSequenceShortTermMemory[indexWeAreChecking],
			                                                 solutionPermutation);

			List<int> evolvingPossibilityBoard = movementSequenceShortTermMemory.Last();

			if(tokenIndex != null)
			{ 
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
		if(emptyTilesIDsSplitByType[1].Count >= numberOfTokensThePlayerHasForThisPossibilitySpace &&
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
