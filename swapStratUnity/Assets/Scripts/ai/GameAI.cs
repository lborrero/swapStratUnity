using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameAI : MonoBehaviour {

	SwapBoard sb;
	List<string> Memory = new List<string>();
	public PlayerVO.PlayerType aiPt = new PlayerVO.PlayerType ();

	// Use this for initialization
	void Start () {
		sb = gameObject.GetComponent<SwapBoard> ();
		Debug.Log (sb);
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
			break;
		}
		case sGameManager.TurnLoop.selectATokenFromBoard:
		{
//			Debug.Log ("AI: selectATokenFromBoard");
			List<Tile> possibleTokensOnTile = GetMoveableTokensList();
			System.Random rnd = new System.Random();
			sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
			break;
		}
		case sGameManager.TurnLoop.moveSelectedToken:
		{
				List<List<int>> shortTermMemory = new List<List<int>>();
				List<Tile> possibleTokensOnTile = new List<Tile>(GetMoveableTokensList());
			for (int i = 0; i < possibleTokensOnTile.Count; i++) 
			{
					shortTermMemory.Add(sb.sbm.getPotentialTilesIdToMoveTo (possibleTokensOnTile[i].tileId));
			}
				Debug.Log ("AI: " + possibleTokensOnTile.Count + " " + shortTermMemory.Count);

				string ps = "";
				//print
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

				for (int m = 0; m < possibilityBubbles.Count; m++) 
				{
					
					Debug.Log ("possibilityBubbles: " + shortTermMemory[(int)possibilityBubbles [m].x].Count + ":: " + possibilityBubbles [m].x + " " + possibilityBubbles [m].y + " = " + PossibilityCalculator.NumberOfPossibilities(33,1) + ": " + PossibilityCalculator.NumberOfPossibilities(shortTermMemory[(int)possibilityBubbles [m].x].Count, (int)possibilityBubbles [m].y));
				}

			List<int> imediatePossibleTiles = sb.sbm.getTilesIdToMoveTo ();
			Debug.Log ("AI: " + imediatePossibleTiles.Count);

			System.Random rnd = new System.Random();
				int returnedValue = imediatePossibleTiles[rnd.Next(0, imediatePossibleTiles.Count)];
				sb.sbm.TileClicked(returnedValue);
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
}
