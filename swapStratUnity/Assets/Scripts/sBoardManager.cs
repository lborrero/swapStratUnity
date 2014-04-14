using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sBoardManager : MonoBehaviour
{
	private static sBoardManager instance;

	public SwapBoard boardView;

	public PlayerVO player1 = new PlayerVO();
	public PlayerVO player2 = new PlayerVO();
	public PlayerVO currentPlayerTurn = new PlayerVO();

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

	void ContinueGameAction()
	{
		sGameManager sgm = sGameManager.Instance;
		Debug.Log ("currentTurnLoop: " + sgm.currentTurnLoop);
		switch(sgm.currentTurnLoop)
		{
		case sGameManager.TurnLoop.selectATokenFromBench:
			if(currentPlayerTurn.hasSelectedTokenFromBench)
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.placeSelectedTokenFromBench;
			}
			break;
		case sGameManager.TurnLoop.placeSelectedTokenFromBench:
			if(currentPlayerTurn.hasPlacedPieceFromBench && currentPlayerTurn.HasAvailableMoves())
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBoard;
			}
			else
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
				ContinueGameAction();
			}
			break;
		case sGameManager.TurnLoop.selectATokenFromBoard:
			if(currentPlayerTurn.hasSelectedTokenFromBoard)
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.moveSelectedToken;
			}
			break;
		case sGameManager.TurnLoop.moveSelectedToken:
			if(currentPlayerTurn.hasMovedTokenFromBoard && currentPlayerTurn.HasAvailableMoves())
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBoard;
				currentPlayerTurn.hasSelectedTokenFromBoard = false;
				currentPlayerTurn.hasMovedTokenFromBoard = false;
			}
			else if(currentPlayerTurn.hasMovedTokenFromBoard)
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
				ContinueGameAction();
			}
			break;
		case sGameManager.TurnLoop.endLoopTurn:
			currentPlayerTurn.hasSelectedTokenFromBench = false;
			currentPlayerTurn.hasPlacedPieceFromBench = false;
			currentPlayerTurn.hasSelectedTokenFromBoard = false;
			currentPlayerTurn.hasMovedTokenFromBoard = false;
			sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBench;
			break;
		}
		boardView.SetGameActionLabel(sgm.currentTurnLoop.ToString());
	}

	public void TileClicked(int tileId)
	{
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.placeSelectedTokenFromBench && boardList [tileId].currentTileType == Tile.TileType.empty && !currentPlayerTurn.hasPlacedPieceFromBench)
		{
			boardView.AddTokenOnTile (tileId);
			SelectingTilesToMove(tileId);
			boardList [tileId].currentTileType = Tile.TileType.occupied;
			currentPlayerTurn.hasPlacedPieceFromBench = true;
			currentPlayerTurn.MoveMade();
			boardView.UpdateCounters();
			ContinueGameAction();
		}
//		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken)
//		{
//			UnhighlightBoard ();
//			tokenList[tokenList.Count-1].gameObject.transform.position = boardList[tileId].gameObject.transform.position;
//			sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.placeSelectedTokenFromBench;
//		}
		UpdateBoard();
	}

	public void TokenClicked(int tokenId, Token.TokenType tokt)
	{
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBench)
		{
			if(currentPlayerTurn.playerTokenBench.isTokenUsed(tokenId))
			{
				currentlySelectedToken.SetTokenId(tokenId);
				currentlySelectedToken.currentTokenType = (currentPlayerTurn.currentPlayerType == PlayerVO.PlayerType.friend) ? Token.TokenType.friendly : Token.TokenType.enemy;
				currentPlayerTurn.playerTokenBench.SelectAToken(tokenId);
				currentPlayerTurn.hasSelectedTokenFromBench = true;
			}
			HighlighEmptyTiles();
			ContinueGameAction();
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

	void HighlighEmptyTiles()
	{
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList[i].currentTileType == Tile.TileType.empty)
			{
				boardList[i].currentTileVisualState = Tile.TileVisualState.highlighted;
				boardList[i].UpdateState();
			}
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