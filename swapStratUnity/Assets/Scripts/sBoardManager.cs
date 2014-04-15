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

	public void ContinueInnerGameAction()
	{
		sGameManager sgm = sGameManager.Instance;
		Debug.Log ("ContinueInnerGameAction: " + sgm.currentInnerGameLoop);
		switch(sgm.currentInnerGameLoop)
		{
		case sGameManager.InnerGameLoop.playerOneTurn:
			sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerTwoTurn;
			currentPlayerTurn = player2;
			currentPlayerTurn.StartPlayerTurn();
			break;
		case sGameManager.InnerGameLoop.playerTwoTurn:
			sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerOneTurn;
			currentPlayerTurn = player1;
			currentPlayerTurn.StartPlayerTurn();
			break;
		}
		UpdateHud ();
	}

	void ContinueInnerGameTurnAction()
	{
		sGameManager sgm = sGameManager.Instance;
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
				HighlightCurrentPlayerMovableToken();
			}
			else
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
				ContinueInnerGameTurnAction();
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
				HighlightCurrentPlayerMovableToken();
			}
			else if(currentPlayerTurn.hasMovedTokenFromBoard && !currentPlayerTurn.HasAvailableMoves())
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
				ContinueInnerGameTurnAction();
			}
			break;
		case sGameManager.TurnLoop.endLoopTurn:
			currentPlayerTurn.hasSelectedTokenFromBench = false;
			currentPlayerTurn.hasPlacedPieceFromBench = false;
			currentPlayerTurn.hasSelectedTokenFromBoard = false;
			currentPlayerTurn.hasMovedTokenFromBoard = false;
			sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBench;
			ContinueInnerGameAction();
			break;
		}
		UpdateHud ();
	}

	public void UpdateHud()
	{
		boardView.UpdateCounters ();
		boardView.SetGameActionLabel(sGameManager.Instance.currentInnerGameLoop.ToString() + " " + sGameManager.Instance.currentTurnLoop.ToString());
	}

	public void TileClicked(int tileId)
	{
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.placeSelectedTokenFromBench && 
		   boardList [tileId].currentTileType == Tile.TileType.empty && 
		   !currentPlayerTurn.hasPlacedPieceFromBench)
		{
			boardView.AddTokenOnTile (tileId);
			HighlightingTilesToMoveTo(tileId);
			boardList [tileId].currentTileType = Tile.TileType.occupied;
			boardList[tileId].occupyingTokenId = currentlySelectedToken.tokenId;
			boardList [tileId].occupyingTokenPlayerType = currentlySelectedToken.tokenPlayerType;
			currentPlayerTurn.hasPlacedPieceFromBench = true;
			currentPlayerTurn.MoveMade();
			boardView.UpdateCounters();
			ContinueInnerGameTurnAction();
		}
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBoard && 
		        boardList[tileId].currentTileType == Tile.TileType.occupied && 
		        boardList[tileId].occupyingTokenPlayerType == currentPlayerTurn.currentPlayerType &&
		        !currentPlayerTurn.hasSelectedTokenFromBoard)
		{

			currentlySelectedTile = boardList[tileId];

			Token tmptoken = getTokenFromTokenListWithIdAndType(boardList[tileId].occupyingTokenId, boardList[tileId].occupyingTokenPlayerType);
			currentlySelectedToken.SetTokenId(tmptoken.tokenId);
			currentlySelectedToken.currentTokenType = (tmptoken.tokenPlayerType == PlayerVO.PlayerType.friend) ? Token.TokenType.friendly : Token.TokenType.enemy;

			HighlightingTilesToMoveTo(tileId);
			boardList[tileId].currentTileState = Tile.TileState.selected;
			boardList[tileId].currentTileVisualState = Tile.TileVisualState.selected;

			currentPlayerTurn.hasSelectedTokenFromBoard = true;

			ContinueInnerGameTurnAction();
		}
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken)
		{
			boardList [tileId].currentTileType = Tile.TileType.occupied;
			boardList [tileId].occupyingTokenId = currentlySelectedToken.tokenId;
			boardList [tileId].occupyingTokenPlayerType = currentlySelectedToken.tokenPlayerType;

			currentlySelectedTile.currentTileState = Tile.TileState.unselected;
			currentlySelectedTile.currentTileVisualState = Tile.TileVisualState.unselected;
			currentlySelectedTile.currentTileType = Tile.TileType.empty;
			currentlySelectedTile.occupyingTokenId = -1;
			currentlySelectedTile.occupyingTokenPlayerType = PlayerVO.PlayerType.none;

			currentlySelectedTile = boardList [tileId];

			UnhighlightBoard ();

			Token tmptoken = getTokenFromTokenListWithIdAndType(currentlySelectedToken.tokenId, currentlySelectedToken.tokenPlayerType);
			tmptoken.gameObject.transform.position = boardList[tileId].gameObject.transform.position;

			currentPlayerTurn.hasMovedTokenFromBoard = true;
			currentPlayerTurn.MoveMade();

			ContinueInnerGameTurnAction();
		}
		UpdateBoard();
	}

	Token getTokenFromTokenListWithIdAndType(int tokenId, PlayerVO.PlayerType token_pt)
	{
		int tId = -1;
		for(int i =0; i<tokenList.Count; i++)
		{
			if(tokenList[i].tokenId == tokenId && tokenList[i].tokenPlayerType == token_pt)
			{
				tId = i;
			}
		}
		return tokenList[tId];
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
			ContinueInnerGameTurnAction();
		}
	}

	void HighlightingTilesToMoveTo(int tileId)
	{	
		UnhighlightBoard ();

		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (), width, height, boardList [tileId].xPos, boardList [tileId].yPos); 
		for(int i = 0; i<contiguousTiles.Count; i++)
		{
			boardList [contiguousTiles[i]].currentTileVisualState = Tile.TileVisualState.highlighted;
		}
	}

	void HighlightCurrentPlayerMovableToken()
	{
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList[i].currentTileType == Tile.TileType.occupied && boardList[i].occupyingTokenPlayerType == currentPlayerTurn.currentPlayerType)
			{
				boardList[i].currentTileVisualState = Tile.TileVisualState.highlighted;
				boardList[i].UpdateState();
			}
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

	List<int> boardListIntoBinaryList()
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == currentlySelectedTile.tileId)?1:0);
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