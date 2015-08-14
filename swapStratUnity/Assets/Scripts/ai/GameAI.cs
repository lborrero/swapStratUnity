using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameAI : MonoBehaviour {

	SwapBoard sb;
	List<string> Memory = new List<string>();
	PlayerVO.PlayerType aiPt = new PlayerVO.PlayerType ();

	// Use this for initialization
	void Start () {
	
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
//		if(aiPt == PlayerVO.PlayerType.friend)
//		{
//			if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerOneTurn) 
//			{
//				ThinkLoop ("asdf");
//			}
//		}
	}

	bool ThinkLoop(string theGameObjectState)
	{
		bool returnValue = false;
		switch (sb.sgm.currentTurnLoop) 
		{
		case sGameManager.TurnLoop.selectATokenFromBench:
		{
			System.Random rnd = new System.Random ();
			rnd.Next (0, sb.sbm.player2.benchedTokens - 1);
			int tokensLeft = sb.sbm.player2.playerTokenBench.benchedTokens.Count;
			for (int i = 0; i < tokensLeft; i++)
			{
				Token tk = sb.sbm.player2.playerTokenBench.benchedTokens[i].GetComponent<Token>();
				if (tk.isTokenOnBoard && tk.currentTokenState != Token.TokenState.disabled) 
				{
					sb.sbm.TokenClicked(tk.tokenId, tk.currentTokenType);
					break;
				}
			}
			break;
		}
		case sGameManager.TurnLoop.placeSelectedTokenFromBench:
		{
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
			List<Tile> possibleTokensOnTile = new List<Tile>();
			for(int i = 0; i<sb.sbm.boardList.Count; i++)
			{
				if(sb.sbm.boardList[i].currentTileType == Tile.TileType.occupied && sb.sbm.boardList[i].occupyingTokenPlayerType == aiPt)
				{
					possibleTokensOnTile.Add(sb.sbm.boardList[i]);
				}
			}
			System.Random rnd = new System.Random();
			sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count-1)].tileId);
			break;
		}
		case sGameManager.TurnLoop.moveSelectedToken:
		{
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
		case sGameManager.TurnLoop.endLoopTurn:
			break;
		}
		return returnValue;
	}
}
