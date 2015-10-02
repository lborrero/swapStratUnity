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
		intermediate
	}

	SwapBoard sb;
	List<string> MediumMemory = new List<string>();
	public PlayerVO.PlayerType aiPt = new PlayerVO.PlayerType ();
	public AiType ait;

	List<Vector2> TileSelectionSequence = new List<Vector2>();

	// Use this for initialization
	void Start () {
		sb = gameObject.GetComponent<SwapBoard> ();
//		Debug.Log (sb);
		PossibilityCalculator pc = new PossibilityCalculator ();
	}

	void Update()
	{
		PercieveLoop ();
	}

	void PercieveLoop()
	{
		if(aiPt == PlayerVO.PlayerType.enemy)
		{
			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerTwoTurn) 
			{
				Debug.Log("AI P2");
				ThinkLoop ("asdf");
			}
		}
		if(aiPt == PlayerVO.PlayerType.friend)
		{
			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerOneTurn) 
			{
				Debug.Log("AI P1");
				ThinkLoop ("asdf");
			}
		}
	}

	bool timeChecker = false;

	bool ThinkLoop(string theGameObjectState)
	{
		bool returnValue = false;
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
			List<Tile> possibleTiles = new List<Tile>();
			for(int i = 0; i<sb.sbm.boardList.Count; i++)
			{
				if(sb.sbm.boardList[i].currentTileType == Tile.TileType.empty)
				{
					possibleTiles.Add(sb.sbm.boardList[i]);
				}
			}

			System.Random rnd = new System.Random();
			sb.sbm.TileClicked(possibleTiles[rnd.Next(0, possibleTiles.Count-1)].tileId);

			switch(ait)
			{
				case AiType.intermediate:
				{
					GenerateTurnSequence();
					Debug.Log("GenerateTurnSequence: " + TileSelectionSequence.Count + " " + currentAiProcess);
					break;
				}
				case AiType.random:
				{
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
						Debug.Log("AI selectATokenFromBoard" + (int)TileSelectionSequence[0].x);
						sb.sbm.TileClicked((int)TileSelectionSequence[0].x);// select token with this tile id is found in the x value of the vector 2
					}
					break;
				}
				case AiType.random:
				{
					//			Debug.Log ("AI: selectATokenFromBoard");
					List<Tile> possibleTokensOnTile = GetMoveableTokensList();
					System.Random rnd = new System.Random();
					sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
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
					Debug.Log("AI moveSelectedToken: " + (int)TileSelectionSequence[0].y);
					sb.sbm.TileClicked((int)TileSelectionSequence[0].y);// place token on this tile id is found in the y value of the vector 2
					DestroyTurnSequenceStep0();
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
				if (!tmptoken.hasTokenBeenMoved) 
				{
					possibleTokensOnTile.Add (sb.sbm.boardList [i]);
				}
			}
		}
		return possibleTokensOnTile;
	}

	void DestroyTurnSequenceStep0()
	{
		TileSelectionSequence.RemoveAt (0);
		Debug.Log ("clicks"  +TileSelectionSequence.Count);
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

	void GenerateTurnSequence()
	{
		//--- Generate Possibility bubbles on the board ---
		List<List<int>> shortTermMemory = new List<List<int>>(); // goes through every moveable token and board with their moveable option, and have them stocked in the short term memory. This short term allows us to check how many possibility bubble there are.
		//shortTermMemory are an array of tile IDs.

		List<Tile> possibleMoveableTokensOnBoard = new List<Tile>(GetMoveableTokensList());
		
		for (int i = 0; i < possibleMoveableTokensOnBoard.Count; i++) 
		{
			shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveTo (possibleMoveableTokensOnBoard[i].tileId)); //  goes through every moveable token and board with their moveable option
		}
		
		//		Debug.Log ("AI: " + possibleMoveableTokensOnBoard.Count + " " + shortTermMemory.Count);
		
		string ps = "";
		for (int j = 0; j < shortTermMemory.Count; j++) 
		{
			shortTermMemory[j].Sort ();	
			for (int k = 0; k < shortTermMemory[j].Count; k++)
			{
				ps += shortTermMemory [j] [k] + ",";
			}
			ps += "\n";
		}
//					Debug.Log (ps);
		
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
		for (int m = 0; (m < possibilityBubbles.Count); m++) 
		{	
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
			for(int j=0; j<P1.Count; j++)
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
			for(int j=0; j<P1.Count; j++)
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

				string sssJ = "";
				for(int w=0; w<winnerCharsWhereYouWantToMoveTo.Count; w++)
				{
					sssJ += winnerCharsWhereYouWantToMoveTo[w];
				}
				Debug.Log("sssJ: " + sssJ);

				// create all the possible orders of the movement.

				// transform all the tokens into incremental values 1.2...5.... // This is done so they can be indexed and have various orders.
				char [] desiredTileIndexesToMoveTo = new char[r]; // r here is the number of tokens to place
				Debug.Log("Number of pieces to move in this bubble. " + r);
				for(int i=0; i<r; i++)
				{
					string ss = "" + i;
					desiredTileIndexesToMoveTo[i] = ss[0];
//					Debug.Log(">"+i + " " + desiredTileIndexesToMoveTo[i]);
				}

				Permutations<char> permutationsForMovementOrder = new Permutations<char>(desiredTileIndexesToMoveTo);
				string format2 = "Permutations of {{1 2 3}} without repetition; size = {0}";
				Debug.Log(String.Format(format2, permutationsForMovementOrder.Count));
				
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

				string ssssA = "";
				for(int i=0; i<currentPossibilityBoard.Count; i++)
				{
					ssssA += currentPossibilityBoard[i];
				}
				Debug.Log("ssssA: " + ssssA);

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

				string ssssB = "";
				for(int i=0; i<currentPossibilityBoard.Count; i++)
				{
					ssssB += currentPossibilityBoard[i];
				}
				Debug.Log("ssssB: " + ssssB);

				movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));
				
				int isAbleToFormToThisPermutation = 1;

				int pass_tileIdForTokenToMove = 0;
				int pass_tileIdForTokenToMoveTo = 0;
				for(int q = 0; q<permutationsForMovementOrder.Count; q++)
				{
					IList<char> permutationBeingVerified = permutationsForMovementOrder.ElementAt(q);
					int permutationsIncrement = 0;
					
					int isAbleToMoveToAllPositions = 1;
					for(int i = 0; i<winnerCharsWhereYouWantToMoveTo.Count; i++)// iterating through a tile permutation possibility
					{
						if(winnerCharsWhereYouWantToMoveTo[i] == 't')// this is the space to move to from the winning combination
						{
							string ssssE = "";
							for(int u=0; u<currentMoveablTokensInThisBubble.Count; u++)
							{
								ssssE += currentMoveablTokensInThisBubble[u] + ", ";
							}
							Debug.Log("ssssE" + i + ": " + ssssE);

							//	Debug.Log("asdfasdf: " + possibleMoveableTokensOnBoard.Count + " " + permutationBeingVerified.Count + " " + (int)(permutationBeingVerified[permutationsIncrement] - 48));
							int tileIdForTokenToMove = currentMoveablTokensInThisBubble[(int)(permutationBeingVerified[permutationsIncrement] - 48)];// 48 is how to convert char values to int values
							// permutationBeingVerified length is exactly the same length as the currently available tokens that can be moved on the board.
							permutationsIncrement++;
							int tileIdForTokenToMoveTo = shortTermMemory[(int)possibilityBubbles [m].x][i];

							//include the currently selected tile for the contiguous search
							currentPossibilityBoard[tileIdForTokenToMove] = 1;
								
							string ssssC = "";
							for(int p=0; p<currentPossibilityBoard.Count; p++)
							{
								ssssC += currentPossibilityBoard[p];
							}
							Debug.Log("ssssC" + i + ": " + ssssC);
							
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
								this.TileSelectionSequence.Add(new Vector2(tileIdForTokenToMove, tileIdForTokenToMoveTo));

								string ssssF = "";
								for(int p=0; p<currentPossibilityBoard.Count; p++)
								{
									ssssF += currentPossibilityBoard[p];
								}
								Debug.Log("ssssF" + i + ": " + ssssF);

			                    // stack the new board state
			                    movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));

								isAbleToMoveToAllPositions &= 1;
			                }
			                else
							{
								pass_tileIdForTokenToMove = tileIdForTokenToMove;
								pass_tileIdForTokenToMoveTo = tileIdForTokenToMoveTo;
								isAbleToMoveToAllPositions &= 0;
							}
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
						Debug.Log("NOOO WHY?!?! isAbleToFormToThisPermutation" + ": " + pass_tileIdForTokenToMove + ", " + pass_tileIdForTokenToMoveTo + ", " + winnerCharsWhereYouWantToMoveTo.Count);
						isAbleToFormToThisPermutation &= 0;
					}
				}

				if(isAbleToFormToThisPermutation == 1)
				{
					break;
				}
				else
				{
					string ssssG = "";
					for(int p=0; p<winnerPermutationIndex.Count; p++)
					{
						ssssG += winnerPermutationIndex[p];
					}
					Debug.Log("FAILED! " + t + " " + winnerPermutationIndex.Count + ": " + ssssG);
				}
			}
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
