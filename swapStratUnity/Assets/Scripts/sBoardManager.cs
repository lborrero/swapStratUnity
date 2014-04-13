using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sBoardManager : MonoBehaviour
{
	private static sBoardManager instance;

	public SwapBoard boardView;
	public TokenBench enemyBench;
	public TokenBench friendlyBench;

	public List<Tile> boardList;
	public List<Token> tokenList;
	public int width;
	public int height;

	public Tile currentlySelectedTile;
	public Token currentlySelectedToken;
	
	private sBoardManager() {}
	
	public static sBoardManager Instance
	{
		get 
		{
			if (instance == null)
			{
				GameObject go = new GameObject("sBoardManager");
				instance = go.AddComponent<sBoardManager>();
				instance.boardList = new List<Tile>();
				instance.tokenList = new List<Token>();
			}
			return instance;
		}
	}

	public void TileClicked(int tileId)
	{
//		Debug.Log ("TileClicked: " + sGameManager.Instance.currentTurnLoop + " " + boardList [tileId].currentTileType);
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.placeSelectedTokenFromBench && boardList [tileId].currentTileType == Tile.TileType.empty)
		{
			boardView.AddTokenOnTile (tileId, Token.TokenType.friendly);
			SelectingTilesToMove(tileId);
			boardList [tileId].currentTileType = Tile.TileType.occupied;
			sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.moveSelectedToken;
		}
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken)
		{
			UnhighlightBoard ();
			tokenList[tokenList.Count-1].gameObject.transform.position = boardList[tileId].gameObject.transform.position;
			sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.placeSelectedTokenFromBench;
		}
		UpdateBoard();
	}

	public void TokenClicked(int tokenId, Token.TokenType tokt)
	{
		Debug.Log ("TokenClicked: " + sGameManager.Instance.currentTurnLoop);
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBench)
		{
			Debug.Log ("TokenClicked: A");
			if(tokt == Token.TokenType.benchFriendly)
			{
				Debug.Log ("TokenClicked: B");
				if(friendlyBench.isTokenUsed(tokenId))
				{
					Debug.Log ("TokenClicked: C");
					currentlySelectedToken.SetTokenId(tokenId);
					currentlySelectedToken.currentTokenType = Token.TokenType.friendly;
					friendlyBench.SelectAToken(tokenId);
				}
			}
			else
			{
				Debug.Log ("TokenClicked: D");
				if(enemyBench.isTokenUsed(tokenId))
				{
					Debug.Log ("TokenClicked: E");
					currentlySelectedToken.SetTokenId(tokenId);
					currentlySelectedToken.currentTokenType = Token.TokenType.enemy;
					enemyBench.SelectAToken(tokenId);
				}
			}
		}
	}

	void SelectingTilesToMove(int tileId)
	{
//		if(boardList [tileId].currentTileState == Tile.TileState.selected)
//		{
//			boardList [tileId].currentTileState = Tile.TileState.unselected;
//			boardList [tileId].currentTileVisualState = Tile.TileVisualState.unselected;
//		}
//		else
//		{
//			boardList [tileId].currentTileState = Tile.TileState.selected;
//			boardList [tileId].currentTileVisualState = Tile.TileVisualState.unselected;
//		}
		
		UnhighlightBoard ();

//		Debug.Log ("SelectingTilesToMove: " + ListsToStrings (boardListIntoBinaryList (Tile.TileType.empty)));
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (Tile.TileType.empty), width, height, boardList [tileId].xPos, boardList [tileId].yPos); 
//		Debug.Log ("SelectingTilesToMove: " + ListsToStrings (contiguousTiles));

		for(int i = 0; i<contiguousTiles.Count; i++)
		{
			boardList [contiguousTiles[i]].currentTileVisualState = Tile.TileVisualState.highlighted;
		}
	}

	void UnhighlightBoard()
	{
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList [i].currentTileState == Tile.TileState.selected)
				boardList [i].currentTileVisualState = Tile.TileVisualState.selected;
			if(boardList [i].currentTileState == Tile.TileState.unselected)
				boardList [i].currentTileVisualState = Tile.TileVisualState.unselected;
		}
	}

	void UpdateBoard()
	{
		for(int i = 0; i<boardList.Count; i++)
		{
			boardList [i].UpdateState ();
		}
	}

	List<int> boardListIntoBinaryList(Tile.TileType tileType)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == tileType)?1:0);
		}
		return tmpArray;
	}

	string ListsToStrings(List<int> list)
	{
		string toPrint = "";
		for(int i=0; i<list.Count; i++)
		{
			toPrint = toPrint + list[i] + ",";
		}
		return toPrint;
	}
}