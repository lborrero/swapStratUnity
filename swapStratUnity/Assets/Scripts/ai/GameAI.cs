using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameAI : MonoBehaviour {

	SwapBoard sb;
	List<string> Memory = new List<string>();
	public PlayerVO.PlayerType aiPt = new PlayerVO.PlayerType ();

	// Use this for initialization
	void Start () {
		sb = gameObject.GetComponent<SwapBoard> ();
		Debug.Log (sb);
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
				Debug.Log ("AI: selectATokenFromBench");
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
				Debug.Log ("AI: placeSelectedTokenFromBench");
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
			System.Random rnd = new System.Random();
			sb.sbm.TileClicked(possibleTokensOnTile[rnd.Next(0, possibleTokensOnTile.Count)].tileId);
			break;
		}
		case sGameManager.TurnLoop.moveSelectedToken:
		{
			List<int> possibleTiles = sb.sbm.getTilesIdToMoveTo ();
			System.Random rnd = new System.Random();
			Debug.Log ("AI: " + possibleTiles.Count);
				int returnedValue = possibleTiles[rnd.Next(0, possibleTiles.Count)];
				sb.sbm.TileClicked(returnedValue);
			break;
		}
		case sGameManager.TurnLoop.endLoopTurn:
			Debug.Log ("AI: endLoopTurn");
			break;
		}
		return returnValue;
	}
}
