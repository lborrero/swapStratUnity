using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sBoardManager : MonoBehaviour
{
	private static sBoardManager instance;
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

	public SwapBoard boardView;

	public PlayerVO player1 = new PlayerVO();
	public PlayerVO player2 = new PlayerVO();
	public PlayerVO currentPlayerTurn = new PlayerVO();

	public List<Tile> boardList;
	public List<Token> tokenList;
	public int board_width;
	public int board_height;

	public Tile currentlySelectedTile;
	public Token currentlySelectedToken;


	public void ContinueInnerGameAction()
	{
		sGameManager sgm = sGameManager.Instance;
//		Debug.Log ("ContinueInnerGameAction: " + sgm.currentInnerGameLoop);
		switch(sgm.currentInnerGameLoop)
		{
		case sGameManager.InnerGameLoop.playerOneTurn:
			if(!CheckToSeeIfGameIsFinished())
			{
				ResetPlayersTokens(currentPlayerTurn.currentPlayerType);
				sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerTwoTurn;
				currentPlayerTurn = player2;
				currentPlayerTurn.StartPlayerTurn();
			}
			else
			{
				sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.endInnerGameLoop;
				ContinueInnerGameAction();
			}
			break;
		case sGameManager.InnerGameLoop.playerTwoTurn:
			if(!CheckToSeeIfGameIsFinished())
			{
				ResetPlayersTokens(currentPlayerTurn.currentPlayerType);
				sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerOneTurn;
				currentPlayerTurn = player1;
				currentPlayerTurn.StartPlayerTurn();
			}
			else
			{
				sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.endInnerGameLoop;
				ContinueInnerGameAction();
			}
			break;
		case sGameManager.InnerGameLoop.endInnerGameLoop:
			if(player1.currentTurnPointCount > player2.currentTurnPointCount)
			{
				boardView.SetGameActionLabel("player 1 wins");
			}
			else
			{
				boardView.SetGameActionLabel("player 2 wins");
			}
			boardView.UpdateCounters();
			break;
		}
		UpdateHud ();
	}

	bool CheckToSeeIfGameIsFinished()
	{
		bool boardComplete = true;
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList[i].currentTilePlayerType == PlayerVO.PlayerType.none && boardList[i].currentTileType != Tile.TileType.nothing)
			{
				boardComplete = false;
			}
		}
//		Debug.Log ("CheckToSeeIfGameIsFinished: " + boardComplete); 
		return boardComplete;
	}

	void ResetPlayersTokens(PlayerVO.PlayerType pt)
	{
		for(int i = 0; i<tokenList.Count; i++)
		{
			if(tokenList[i].tokenPlayerType == pt)
			{
				tokenList[i].hasTokenBeenMoved = false;
			}
		}
	}

	bool doesPlayerHaveMoveableTokens(PlayerVO player)
	{
		bool yesHeDoes = false;
		Token tmptoken = getTokenFromTokenListWithIdAndType(currentlySelectedToken.tokenId, currentlySelectedToken.tokenPlayerType);
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList[i].currentTileType == Tile.TileType.occupied && boardList[i].occupyingTokenPlayerType == player.currentPlayerType)
			{
				tmptoken = getTokenFromTokenListWithIdAndType(boardList[i].occupyingTokenId, boardList[i].occupyingTokenPlayerType);
				if(!tmptoken.hasTokenBeenMoved)
				{
					List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (i), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
					if(contiguousTiles.Count > 1)
					{
						yesHeDoes = true;
					}
				}
			}
		}
		return yesHeDoes;
	}

	void ContinueInnerGameTurnAction()
	{
		sGameManager sgm = sGameManager.Instance;
//		Debug.Log ("ContinueInnerGameTurnAction: " + sgm.currentTurnLoop);
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
				if(doesPlayerHaveMoveableTokens(currentPlayerTurn))
				{
					sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBoard;
					HighlightCurrentPlayerMovableToken();
				}
				else
				{
					sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
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
			if(currentPlayerTurn.hasMovedTokenFromBoard && currentPlayerTurn.HasAvailableMoves() && doesPlayerHaveMoveableTokens(currentPlayerTurn))
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBoard;
				currentPlayerTurn.hasSelectedTokenFromBoard = false;
				currentPlayerTurn.hasMovedTokenFromBoard = false;
				HighlightCurrentPlayerMovableToken();
			}
			else if(currentPlayerTurn.hasMovedTokenFromBoard && (!currentPlayerTurn.HasAvailableMoves() || !doesPlayerHaveMoveableTokens(currentPlayerTurn)))
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

			currentPlayerTurn.currentTurnPointCount += CountTilesForPlayer(currentPlayerTurn.currentPlayerType);

			ContinueInnerGameAction();
			break;
		}
		UpdateHud ();
	}

	public void UpdateHud()
	{
		boardView.UpdateCounters ();
		if(sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.endInnerGameLoop)
		{
			if(player1.currentTurnPointCount > player2.currentTurnPointCount)
			{
				boardView.SetGameActionLabel("player 1 wins");
			}
			else if (player1.currentTurnPointCount < player2.currentTurnPointCount)
			{
				boardView.SetGameActionLabel("player 2 wins");
			}
			else
			{
				boardView.SetGameActionLabel("tie game");
			}
		}
		else
		{
			boardView.SetGameActionLabel(sGameManager.Instance.currentInnerGameLoop.ToString() + " " + sGameManager.Instance.currentTurnLoop.ToString());
		}
	}

	public void TileClicked(int tileId)
	{
		if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.placeSelectedTokenFromBench && 
		   boardList [tileId].currentTileType == Tile.TileType.empty && 
		   !currentPlayerTurn.hasPlacedPieceFromBench)
		{
			if(boardList[tileId].currentTileType != Tile.TileType.nothing)
			{
				boardView.AddTokenOnTile (tileId);
				HighlightingTilesToMoveTo(tileId);

				boardList [tileId].currentTileType = Tile.TileType.occupied;
				boardList [tileId].occupyingTokenId = currentlySelectedToken.tokenId;
				boardList [tileId].occupyingTokenPlayerType = currentlySelectedToken.tokenPlayerType;
				boardList [tileId].currentTilePlayerType = currentlySelectedToken.tokenPlayerType;

				currentPlayerTurn.hasPlacedPieceFromBench = true;
				currentPlayerTurn.MoveMade();
				boardView.UpdateCounters();

				if(CheckToSeeIfGameIsFinished())
				{
					sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
				else
				{
					ContinueInnerGameTurnAction();
				}
			}
		}
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBoard && 
		        boardList[tileId].currentTileType == Tile.TileType.occupied && 
		        boardList[tileId].occupyingTokenPlayerType == currentPlayerTurn.currentPlayerType &&
		        !currentPlayerTurn.hasSelectedTokenFromBoard)
		{
			List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (tileId), board_width, board_height, boardList [tileId].xPos, boardList [tileId].yPos); 
			if(contiguousTiles.Count > 1 && boardList[tileId].currentTileType != Tile.TileType.nothing)
			{
				Token tmptoken = getTokenFromTokenListWithIdAndType(boardList[tileId].occupyingTokenId, boardList[tileId].occupyingTokenPlayerType);
				if(!tmptoken.hasTokenBeenMoved)
				{
					currentlySelectedTile = boardList[tileId];

					currentlySelectedToken.SetTokenId(tmptoken.tokenId);
					currentlySelectedToken.currentTokenType = (tmptoken.tokenPlayerType == PlayerVO.PlayerType.friend) ? Token.TokenType.friendly : Token.TokenType.enemy;

					HighlightingTilesToMoveTo(tileId);
					boardList[tileId].currentTileState = Tile.TileState.selected;
					boardList[tileId].currentTileVisualState = Tile.TileVisualState.selected;

					currentPlayerTurn.hasSelectedTokenFromBoard = true;

					ContinueInnerGameTurnAction();
				}
			}
		}
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken)
		{
			if(currentlySelectedTile.tileId != tileId && 
			   boardList[tileId].currentTileType != Tile.TileType.occupied && 
			   boardList[tileId].currentTileType != Tile.TileType.nothing && 
			   boardList[tileId].currentTileVisualState == Tile.TileVisualState.highlighted)
			{
				AstarPathfinding asp = new AstarPathfinding();
				asp.GenerateBoard(boardListIntoBinaryListForPathFinding(currentlySelectedTile.tileId, tileId), board_width, currentlySelectedTile.tileId, tileId);
				asp.ComputePathSequence();
				List<int> pathSequenceList = asp.TraceBackPath();

				boardList [tileId].currentTileType = Tile.TileType.occupied;
				boardList [tileId].occupyingTokenId = currentlySelectedToken.tokenId;
				boardList [tileId].occupyingTokenPlayerType = currentlySelectedToken.tokenPlayerType;
				boardList [tileId].currentTilePlayerType = currentlySelectedToken.tokenPlayerType;

				currentlySelectedTile.currentTileState = Tile.TileState.unselected;
				currentlySelectedTile.currentTileVisualState = Tile.TileVisualState.unselected;
				currentlySelectedTile.currentTileType = Tile.TileType.empty;
				currentlySelectedTile.occupyingTokenId = -1;
				currentlySelectedTile.occupyingTokenPlayerType = PlayerVO.PlayerType.none;

				currentlySelectedTile = boardList [tileId];

				UnhighlightBoard ();

				Token tmptoken = getTokenFromTokenListWithIdAndType(currentlySelectedToken.tokenId, currentlySelectedToken.tokenPlayerType);

				List<Vector3> pathPositionSequenceList = new List<Vector3>();
				for(int i=0; i<pathSequenceList.Count; i++)
				{
					pathPositionSequenceList.Add(boardList[pathSequenceList[i]].gameObject.transform.position);
				}
				tmptoken.moveThrough(pathPositionSequenceList);
				tmptoken.hasTokenBeenMoved = true;

				currentPlayerTurn.hasMovedTokenFromBoard = true;
				currentPlayerTurn.MoveMade();

				if(CheckToSeeIfGameIsFinished())
				{
					sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
				else
				{
					ContinueInnerGameTurnAction();
				}
			}
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

		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (currentlySelectedTile.tileId), board_width, board_height, boardList [tileId].xPos, boardList [tileId].yPos); 
		for(int i = 0; i<contiguousTiles.Count; i++)
		{
			boardList [contiguousTiles[i]].currentTileVisualState = Tile.TileVisualState.highlighted;
		}
	}

	void HighlightCurrentPlayerMovableToken()
	{
		Token tmptoken = getTokenFromTokenListWithIdAndType(currentlySelectedToken.tokenId, currentlySelectedToken.tokenPlayerType);

		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList[i].currentTileType == Tile.TileType.occupied && boardList[i].occupyingTokenPlayerType == currentPlayerTurn.currentPlayerType)
			{
				tmptoken = getTokenFromTokenListWithIdAndType(boardList[i].occupyingTokenId, boardList[i].occupyingTokenPlayerType);
				if(!tmptoken.hasTokenBeenMoved)
				{
					List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (i), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
					if(contiguousTiles.Count > 1)
					{
						boardList[i].currentTileVisualState = Tile.TileVisualState.highlighted;
						boardList[i].UpdateState();
					}
				}
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

	int CountTilesForPlayer(PlayerVO.PlayerType pt)
	{
		int counter = 0;
		for(int i = 0; i<boardList.Count; i++)
		{
			if(boardList [i].currentTilePlayerType == pt)
				counter++;
		}
		return counter;
	}

	void UpdateBoard()
	{
		for(int i = 0; i<boardList.Count; i++)
		{
			boardList [i].UpdateState ();
		}
	}

	List<int> boardListIntoBinaryList(int centerTileToCheck)//currentlySelectedTile.tileId
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == centerTileToCheck)?1:0);
		}
		return tmpArray;
	}

	List<int> boardListIntoBinaryListForPathFinding(int source_id, int target_id)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == source_id || i == target_id)?0:1);
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