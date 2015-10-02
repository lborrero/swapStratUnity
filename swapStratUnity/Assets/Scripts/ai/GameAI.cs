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
				ThinkLoop ("asdf");
			}
		}
		if(aiPt == PlayerVO.PlayerType.friend)
		{
			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerOneTurn) 
			{
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
					Debug.Log("GenerateTurnSequence: " +TileSelectionSequence.Count);
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
						sb.sbm.TileClicked((int)TileSelectionSequence[0].x);// select token with this tile id is found in the x value of the vector 2
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
					sb.sbm.TileClicked((int)TileSelectionSequence[0].y);// place token on this tile id is found in the y value of the vector 2
					DestroyTurnSequenceStep0();
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
	}

	void GenerateTurnSequence()
	{
		//--- Generate Possibility bubbles on the board ---
		List<List<int>> shortTermMemory = new List<List<int>>();
		List<Tile> possibleTokensOnTile = new List<Tile>(GetMoveableTokensList());
		
		for (int i = 0; i < possibleTokensOnTile.Count; i++) 
		{
			shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveTo (possibleTokensOnTile[i].tileId));
		}
		
					Debug.Log ("AI: " + possibleTokensOnTile.Count + " " + shortTermMemory.Count);
		
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
					Debug.Log (ps);
		
		List<Vector2> possibilityBubbles = new List<Vector2> ();
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

		
		
		//Playing the possibility bubbles
		for (int m = 0; (m < possibilityBubbles.Count); m++) 
		{	
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
			int n = shortTermMemory[(int)possibilityBubbles [m].x].Count;
			int r = (int)possibilityBubbles [m].y;
			
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

//			Debug.Log("winner: " + winnerPermutationIndex + " " + mostAmountOfRedTiles);
			/// End Permutation Validation ----
			/// winnerPermutationIndex is the permutation index that has the most red tiles from P1. 

			bool ableToFindAPossibleCombination = false;
			for(int t = winnerPermutationIndex.Count-1; (t>=0 && !ableToFindAPossibleCombination); t--)
			{
				// Now check what tile ID the winning permutation proposes.
				IList<char> winnerCharsWhereYouWantToMoveTo = P1.ElementAt(winnerPermutationIndex[t]); // winnerChars looks something like this: teeeeteeeeeteeettee
				
				// create all the possible orders of the movement.
				char [] desiredTileIndexesToMoveTo = new char[r];
				for(int i=0; i<r; i++)
				{
					string ss = "" + i;
					desiredTileIndexesToMoveTo[i] = ss[0];
					Debug.Log(">"+i + " " + desiredTileIndexesToMoveTo[i]);
				}
				
				Permutations<char> permutations = new Permutations<char>(desiredTileIndexesToMoveTo);
				string format2 = "Permutations of {{A A C}} without repetition; size = {0}";
				Debug.Log(String.Format(format2, permutations.Count));
				
				// now check if it's possible to create such a combination, through movement
				foreach(IList<char> p in permutations) //this here is the order of movement permutations for the current available pieces.
				{
					string ssdb = "";
					for(int j=0; j<p.Count; j++)
					{
						int bar = Convert.ToInt32(new string(p[j], 1));
						ssdb += p[j];
					}
				}
				
				// take the order
				// have the pieces move to that order
				// have tile 1 go to the where the 1 is found in the permutations order.
				List<List<int>> movementSequenceShortTermMemory = new List<List<int>>();// movementSequenceShortTermMemory are snapshots of the sequence of what the board has to look like for this turn
				
				//I Need a representation of the board;
				List<int> currentPossibilityBoard = new List<int>();
				for(int i=0; i<sb.sbm.boardList.Count; i++)
				{
					currentPossibilityBoard.Add(0);
				}
				
				//make board from possibility bubble
				for(int i=0; i<shortTermMemory[(int)possibilityBubbles [m].x].Count; i++)
				{
					currentPossibilityBoard[shortTermMemory[(int)possibilityBubbles [m].x][i]] = 1;
				}
				//remove the currently moveable tiles with tokens on them;
				for(int i=0; i<possibleTokensOnTile.Count; i++)
				{
					currentPossibilityBoard[possibleTokensOnTile[i].tileId] = 0;
				}
				Debug.Log(sb.sbm.ListsToStrings(currentPossibilityBoard));
				
				movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));
				
				bool isAbleToFormToThisPermutation = false;
				
				for(int q = 0; q<permutations.Count; q++)
				{
					IList<char> permutationBeingVerified = permutations.ElementAt(q);
					int permutationsIncrement = 0;
					
					bool isAbleToMoveToThisPosition = true;
					for(int i = 0; i<winnerCharsWhereYouWantToMoveTo.Count; i++)// iterating through a tile permutation possibility
					{
						if(winnerCharsWhereYouWantToMoveTo[i] == 't')// checking if this is the space to move to from the winning combination
						{
							//						Debug.Log("asdfasdf: " + possibleTokensOnTile.Count + " " + permutationBeingVerified.Count + " " + (int)(permutationBeingVerified[permutationsIncrement] - 48));
							int tileIdForTokenToMove = possibleTokensOnTile[(int)(permutationBeingVerified[permutationsIncrement] - 48)].tileId;// 43 is how to convert char values to int values
							sb.sbm.TileClicked(tileIdForTokenToMove); // select the token you want to move. permutationBeingVerified length is exactly the same length as the currently available tokens that can be moved on the board.
							permutationsIncrement++;
							int tileIdForTokenToMoveTo = shortTermMemory[(int)possibilityBubbles [m].x][i]; // i is related to the index ot shortTermMemory where the tileIds are stored. since it's related to winnerCharsWhereYouWantToMoveTo, this it the tile we want to move to.
							
							//include the currently selected tile for the contiguous search
							currentPossibilityBoard[tileIdForTokenToMove] = 1;
								
							// can I make the following move?
							List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (currentPossibilityBoard, sb.sbm.board_width, sb.sbm.board_height, sb.sbm.boardList [tileIdForTokenToMove].xPos, sb.sbm.boardList [tileIdForTokenToMove].yPos);
							if(contiguousTiles.Contains(tileIdForTokenToMoveTo))
							{
								currentPossibilityBoard[tileIdForTokenToMoveTo] = 0; // I make the new token position unavailable
								this.TileSelectionSequence.Add(new Vector2(tileIdForTokenToMove, tileIdForTokenToMoveTo));
			                    // stack the new board state
			                    movementSequenceShortTermMemory.Add(new List<int>(currentPossibilityBoard));
			                }
			                else
							{
								isAbleToMoveToThisPosition = false;
								break;
							}
						}
					}
					if(isAbleToMoveToThisPosition)
					{
						isAbleToFormToThisPermutation = true;
						break;
					}
				}

				ableToFindAPossibleCombination = isAbleToFormToThisPermutation;
				if(ableToFindAPossibleCombination)
				{
					break;
				}
			}
		}
	}
}
