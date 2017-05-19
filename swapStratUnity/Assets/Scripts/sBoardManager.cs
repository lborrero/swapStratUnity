using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
//		Debug.Log (ConvertBoardToStrings (boardList));
//		LoadBoardFormString("Turn=R;B=10;R=4;Board=00,00,00,00,00,00,00,00,00,21,12,10,10,10,10,00,00,10,21,22,10,10,10,00,00,10,22,10,10,10,10,00,00,10,10,10,10,11,10,00,00,10,10,10,11,21,10,00,00,10,10,10,10,10,11,00,00,00,00,00,00,00,00,00,;");
		UpdateBoard();

		sGameManager sgm = sGameManager.Instance;
//		Debug.Log ("ContinueInnerGameAction: " + sgm.currentInnerGameLoop);
		switch(sgm.currentInnerGameLoop)
		{
		case sGameManager.InnerGameLoop.playerOneTurn:
			if(!CheckToSeeIfGameIsFinished())
			{
				if(currentPlayerTurn.currentTurnMoveCount == currentPlayerTurn.currentTurnMoveLimit &&
				   currentPlayerTurn.currentTurnMoveLimit != 0)
				{
					
					sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.endInnerGameLoop;
					ContinueInnerGameAction();
				}
				else
				{
					//Ends player turn
					ResetPlayersTokens(currentPlayerTurn.currentPlayerType);
					sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerTwoTurn;
					sgm.IncrementTurnCount();
					currentPlayerTurn = player2;
					currentPlayerTurn.StartPlayerTurn();
					ContinueInnerGameTurnAction();
					CheckForGuardedTiles ();
				}
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
				if(currentPlayerTurn.currentTurnMoveCount == currentPlayerTurn.currentTurnMoveLimit &&
				   currentPlayerTurn.currentTurnMoveLimit != 0)
				{
					sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.endInnerGameLoop;
					ContinueInnerGameAction();
				}
				else
				{
					ResetPlayersTokens(currentPlayerTurn.currentPlayerType);
					sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.playerOneTurn;
					sgm.IncrementTurnCount();
					currentPlayerTurn = player1;
					currentPlayerTurn.StartPlayerTurn();
					ContinueInnerGameTurnAction();
					CheckForGuardedTiles ();
				}
			}
			else
			{
				sgm.currentInnerGameLoop = sGameManager.InnerGameLoop.endInnerGameLoop;
				ContinueInnerGameAction();
			}
			break;
		case sGameManager.InnerGameLoop.endInnerGameLoop:
			boardView.mm.UpdateGameState((int)sGameManager.GeneralGameState.endScreenWithPoints);
			boardView.UpdateCounters();
			break;
		}
//		Debug.Log ("ContinueInnerGameAction: " + sgm.currentInnerGameLoop);
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

		bool zero = currentPlayerTurn.hasMovedTokenFromBoard; 
 	    bool another = !currentPlayerTurn.HasAvailableMoves();
		bool first = (currentPlayerTurn.currentTurnMoveCount == currentPlayerTurn.currentTurnMoveLimit);
		bool second = !doesPlayerHaveMoveableTokens (currentPlayerTurn);
		bool third = !currentPlayerTurn.HasAvailableTokensOnBench ();

		if(zero && another && first && second && third)
		{
			boardComplete = true;
		}

		return boardComplete;
	}

	void CheckForGuardedTiles()
	{
		//Debug.Log ("CheckForGuardedTiles");
		for(int i = 0; i<boardList.Count; i++)
		{
			if (boardList [i].currentGuardState == Tile.TileGuarded.taken || boardList [i].currentGuardState == Tile.TileGuarded.guarded) 
			{
				//Debug.Log (ListsToStrings (getFullBoardListIntoBinaryList ()));
				//Debug.Log (ListsToStrings (boardListIntoBinaryOfSamePlayerType (i, boardList [i].currentTilePlayerType)));
				List<int> cardinalTiles = ContiguousBlockSearch.returnCardianlTilesFromTile (getFullBoardListIntoBinaryList(), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
				List<int> cardinalTilesWithColor = ContiguousBlockSearch.returnCardianlTilesFromTile (boardListIntoBinaryOfSamePlayerType (i, boardList [i].currentTilePlayerType), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
				//Debug.Log (ListsToStrings (cardinalTiles));
				//Debug.Log (ListsToStrings (cardinalTilesWithColor));
				//Debug.Log (i + " " + cardinalTiles.Count + " " + cardinalTilesWithColor.Count);
				if (cardinalTiles.Count == cardinalTilesWithColor.Count) {
					boardList [i].currentGuardState = Tile.TileGuarded.guarded;
				} 
				else 
				{
					boardList [i].currentGuardState = Tile.TileGuarded.taken;
				}
			}
		}
	}

	public List<Tile> GiveMeAlmostGuardedTiles()
	{
		List<Tile> returnValue = new List<Tile> ();
//		Debug.Log ("CheckForGuardedTiles");
		for(int i = 0; i<boardList.Count; i++)
		{
			if (boardList [i].currentGuardState == Tile.TileGuarded.taken && boardList [i].currentTilePlayerType == currentPlayerTurn.currentPlayerType) 
			{
				List<int> cardinalTiles = ContiguousBlockSearch.returnCardianlTilesFromTile (getFullBoardListIntoBinaryList(), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
				int initialCardinalCount = cardinalTiles.Count;
				int totalThatAreTheSamePlayerType = 0;
//				List<int> cardinalTilesWithColor = ContiguousBlockSearch.returnCardianlTilesFromTile (boardListIntoBinaryOfSamePlayerType (i, boardList [i].currentTilePlayerType), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
//				List<int> cardinalTilesWithoutColor = ContiguousBlockSearch.returnCardianlTilesFromTile (boardListIntoBinaryOfUntakenTiles (i), board_width, board_height, boardList [i].xPos, boardList [i].yPos);
//				string s = "";
				for (int j = cardinalTiles.Count-1; j >= 0; j--) 
				{
					if (boardList [cardinalTiles [j]].currentTilePlayerType == currentPlayerTurn.currentPlayerType) 
					{
						totalThatAreTheSamePlayerType++;
					}

					if (boardList [cardinalTiles [j]].currentTilePlayerType == PlayerVO.PlayerType.none ||
					    (boardList [cardinalTiles [j]].currentTilePlayerType != currentPlayerTurn.currentPlayerType &&
					    boardList [cardinalTiles [j]].currentTileType == Tile.TileType.empty)) {
//						s += "" + cardinalTiles [j] + boardList [cardinalTiles [j]].currentGuardState + boardList [cardinalTiles [j]].currentTilePlayerType + ",";
					} else {
						cardinalTiles.RemoveAt (j);
					}
				}
//				Debug.Log("cardinalCount: " + s);
				if ((initialCardinalCount - totalThatAreTheSamePlayerType) == 1 && cardinalTiles.Count > 0) 
				{
					Debug.Log ("count: " + cardinalTiles.Count);
					returnValue.Add (boardList [cardinalTiles[0]]);
				}
			}
		}
		return returnValue;
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

	bool isThisTokenMoveable(Token tkn)
	{
		bool yesItIs = false;
		for(int i = 0; i<boardList.Count; i++)
		{

			if(boardList[i].currentTileType == Tile.TileType.occupied && 
			   boardList[i].occupyingTokenId == tkn.tokenId && 
			   boardList[i].occupyingTokenPlayerType == tkn.tokenPlayerType)
			{
				List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (i), board_width, board_height, boardList [i].xPos, boardList [i].yPos); 
				if(contiguousTiles.Count > 1)
				{
					yesItIs = true;
				}
			}
		}
		return yesItIs;
	}
	
	void ContinueInnerGameTurnAction()
	{
		sGameManager sgm = sGameManager.Instance;
//		Debug.Log (sgm.currentTurnLoop);
		switch(sgm.currentTurnLoop)
		{
		case sGameManager.TurnLoop.selectATokenFromBench:
			if(currentPlayerTurn.hasSelectedTokenFromBench)
			{
				sgm.currentTurnLoop = sGameManager.TurnLoop.placeSelectedTokenFromBench;
			}
			else
			{
				if(!currentPlayerTurn.HasAvailableTokensOnBench())
				{
					sgm.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBoard;
					HighlightCurrentPlayerMovableToken();
					if(!doesPlayerHaveMoveableTokens(currentPlayerTurn))
					{
						sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
						ContinueInnerGameTurnAction();
					}
				}
				else
				{
					currentPlayerTurn.playerTokenBench.UpdateTokenBenchDisplay(TokenBench.benchState.suggestAToken);
				}
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
//					Debug.Log("A");
					sgm.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
			}
			else
			{
//				Debug.Log("B");
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
			else if(CheckIfPlayerCanMove())
			{
//				Debug.Log("C");
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

//			currentPlayerTurn.currentTurnPointCount += CountTilesForPlayer(currentPlayerTurn.currentPlayerType);

			ContinueInnerGameAction();
			break;
		}
		player1.currentTurnPointCount = CountTilesForPlayer(player1.currentPlayerType);
		player2.currentTurnPointCount = CountTilesForPlayer(player2.currentPlayerType);
		//		Debug.Log ("ContinueInnerGameTurnAction: " + sgm.currentTurnLoop);
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

	public bool TileClicked(int tileId)//returns value false if tile selected doesn't meet any criteria
	{
		bool returnValue = false;
		//placing piece from the bench
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
				if (boardList [tileId].currentGuardState != Tile.TileGuarded.guarded) 
				{
					boardList [tileId].currentGuardState = Tile.TileGuarded.taken;
					boardList [tileId].currentTilePlayerType = currentlySelectedToken.tokenPlayerType;
				}


				//this makes the token that was added to the board not be moveable for this turn.
				Token tmptoken = getTokenFromTokenListWithIdAndType(currentlySelectedToken.tokenId, currentlySelectedToken.tokenPlayerType);
				tmptoken.hasTokenBeenMoved = true;

				currentPlayerTurn.hasPlacedPieceFromBench = true;
				currentPlayerTurn.MoveMade();
				boardView.UpdateCounters();

				if(CheckToSeeIfGameIsFinished())
				{
//					Debug.Log("D");
					sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
				else
				{
					ContinueInnerGameTurnAction();
				}

				returnValue = true;
			}
		}
		//select a token to move from the board
		else if((sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken || sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBoard) && 
		        boardList[tileId].currentTileType == Tile.TileType.occupied && 
		        boardList[tileId].occupyingTokenPlayerType == currentPlayerTurn.currentPlayerType /*&&
		        !currentPlayerTurn.hasSelectedTokenFromBoard*/)
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

					SelectOneTokenOnBoardById(tmptoken);

					currentPlayerTurn.hasSelectedTokenFromBoard = true;

					ContinueInnerGameTurnAction();

					returnValue = true;
				}
			}
		}
		//move selected token
		else if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.moveSelectedToken)
		{
			if(CheckIfCanMoveToTileId(tileId))
			{
				AstarPathfinding asp = new AstarPathfinding();
				asp.GenerateBoard(boardListIntoBinaryListForPathFinding(currentlySelectedTile.tileId, tileId), board_width, currentlySelectedTile.tileId, tileId);
				asp.ComputePathSequence();
				List<int> pathSequenceList = asp.TraceBackPath();

				boardList [tileId].currentTileType = Tile.TileType.occupied;
				boardList [tileId].occupyingTokenId = currentlySelectedToken.tokenId;
				boardList [tileId].occupyingTokenPlayerType = currentlySelectedToken.tokenPlayerType;
				if (boardList [tileId].currentGuardState != Tile.TileGuarded.guarded) 
				{
					boardList [tileId].currentGuardState = Tile.TileGuarded.taken;
					boardList [tileId].currentTilePlayerType = currentlySelectedToken.tokenPlayerType;
				}

				currentlySelectedTile.currentTileState = Tile.TileState.unselected;
				currentlySelectedTile.currentTileVisualState = Tile.TileVisualState.unselected;
				currentlySelectedTile.currentTileType = Tile.TileType.empty;
				currentlySelectedTile.occupyingTokenId = -1;
				currentlySelectedTile.occupyingTokenPlayerType = PlayerVO.PlayerType.none;

				currentlySelectedTile = boardList [tileId];

				UnhighlightBoard ();
				UnSelectAllTokensOnBoard();

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
//					Debug.Log("E");
					sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.endLoopTurn;
					ContinueInnerGameTurnAction();
				}
				else
				{
					ContinueInnerGameTurnAction();
				}

				returnValue = true;
			}
		}
		UpdateBoard();
		return returnValue;
	}

	public bool CheckIfPlayerCanMove()
	{
		if(currentPlayerTurn.hasMovedTokenFromBoard && 
		   (!currentPlayerTurn.HasAvailableMoves() || !doesPlayerHaveMoveableTokens(currentPlayerTurn)))
		{
			return true;
		}
		return false;
	}

	public bool CheckIfCanMoveToTileId(int tileId)
	{
		if (currentlySelectedTile.tileId != tileId && 
			boardList [tileId].currentTileType != Tile.TileType.occupied && 
			boardList [tileId].currentTileType != Tile.TileType.nothing && 
			boardList [tileId].currentTileVisualState == Tile.TileVisualState.highlighted) {
			return true;
		}
		return false;
	}

	public List<Vector3> GeneratePathSequence(int fromTileId, int toTileId)
	{
		AstarPathfinding asp = new AstarPathfinding();
		asp.GenerateBoard(boardListIntoBinaryListForPathFinding(fromTileId, toTileId), board_width, fromTileId, toTileId);
		asp.ComputePathSequence();
		List<int> pathSequenceList = asp.TraceBackPath();

		List<Vector3> pathPositionSequenceList = new List<Vector3>();
		for(int i=0; i<pathSequenceList.Count; i++)
		{
			pathPositionSequenceList.Add(boardList[pathSequenceList[i]].gameObject.transform.position);
		}
		return pathPositionSequenceList;
	}

	public Token getTokenFromTokenListWithIdAndType(int tokenId, PlayerVO.PlayerType token_pt)
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

	public bool TokenClicked(int tokenId, Token.TokenType tokt)
	{
		bool returnValue = false;
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
			returnValue = true;
		}
		return returnValue;
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

	public List<int> getTilesIdToMoveTo()
	{	
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (currentlySelectedTile.tileId), board_width, board_height, boardList [currentlySelectedTile.tileId].xPos, boardList [currentlySelectedTile.tileId].yPos); 
		return contiguousTiles;
	}

	public List<int> getPotentialTilesIdToMoveToExcludingTheTileTheTokenIsIn(int tileId)
	{	
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (tileId), board_width, board_height, boardList [tileId].xPos, boardList [tileId].yPos); 
		for(int i = contiguousTiles.Count-1; i>=0; i--)
		{
			if(contiguousTiles[i] == tileId)
			{
				contiguousTiles.RemoveAt(i);
			}
		}
		return contiguousTiles;
	}

	public List<int> getPotentialTilesIdToMoveToIncludingTilesWithOwnToken(int tileId)
	{	
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryExcludingThePresenceOfOtherPlayerPiecesList (tileId), board_width, board_height, boardList [tileId].xPos, boardList [tileId].yPos); 
		return contiguousTiles;
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
						tmptoken.currentTokenState = Token.TokenState.highlighted;
						tmptoken.UpdateState();
					}
				}
				else
				{
					tmptoken.currentTokenState = Token.TokenState.disabled;
					tmptoken.UpdateState();
				}
			}
		}
	}

	void UnSelectAllTokensOnBoard()
	{
		for(int i = 0; i<tokenList.Count; i++)
		{
			tokenList[i].currentTokenState = Token.TokenState.unselected;
			tokenList[i].UpdateState();
		}
	}


	void SelectOneTokenOnBoardById(Token tkn)
	{
		for(int i = 0; i<tokenList.Count; i++)
		{
			if(tkn.tokenId == tokenList[i].tokenId && tkn.tokenPlayerType == tokenList[i].tokenPlayerType)
			{
				//players selected token
				tokenList[i].currentTokenState = Token.TokenState.selected;
			}
			else
			{
				//other players tokens
				if(tkn.tokenPlayerType == tokenList[i].tokenPlayerType)
				{
					//can token move?
					if(!tokenList[i].hasTokenBeenMoved)
					{
						if(isThisTokenMoveable(tokenList[i]))
						{
							tokenList[i].currentTokenState = Token.TokenState.highlighted;
						}
						else
						{
							tokenList[i].currentTokenState = Token.TokenState.unselected;
						}
					}
					else
					{
						tokenList[i].currentTokenState = Token.TokenState.disabled;
					}
				}
				else
				{
					tokenList[i].currentTokenState = Token.TokenState.unselected;
				}
			}
			tokenList[i].UpdateState();
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

	public List<int> getFullBoardListIntoBinaryList()
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType != Tile.TileType.nothing)?1:0);
		}
		return tmpArray;
	}

	public List<int> boardListIntoBinaryList(int centerTileToCheck)//currentlySelectedTile.tileId
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == centerTileToCheck)?1:0);
		}
		return tmpArray;
	}

	public List<int> boardListIntoBinaryExcludingThePresenceOfOtherPlayerPiecesList(int centerTileToCheck)//currentlySelectedTile.tileId
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == centerTileToCheck || boardList[i].occupyingTokenPlayerType == boardList[centerTileToCheck].occupyingTokenPlayerType)?1:0);
		}
		return tmpArray;
	}

	public List<int> boardListIntoBinaryOfUntakenTiles(int centerTileToCheck)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add (((boardList [i].currentTilePlayerType == PlayerVO.PlayerType.none ||
//							boardList [i].currentTilePlayerType != boardList [centerTileToCheck].currentTilePlayerType ||
							i == centerTileToCheck)
							&& boardList [i].currentTileType != Tile.TileType.nothing) ? 1 : 0);
		}
		return tmpArray;
	}

	public List<int> boardListIntoBinaryOfSamePlayerType(int centerTileToCheck, PlayerVO.PlayerType playerType)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add ((boardList [i].currentTilePlayerType == playerType || i == centerTileToCheck) ? 1 : 0);
		}
		return tmpArray;
	}

	public List<int> boardListIntoBinaryListForPathFinding(int source_id, int target_id)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileType == Tile.TileType.empty || i == source_id || i == target_id)?0:1);
		}
		return tmpArray;
	}

	public string ListsToStrings(List<int> list)
	{
		string toPrint = "";
		for(int i=0; i<list.Count; i++)
		{
			if (i % board_width == 0) {
				toPrint = toPrint + "\n";
			}
			toPrint = toPrint + list[i] + ",";
		}
		return toPrint;
	}

	public void LoadBoardFormString(string boardString)
	{
		string [] info = boardString.Split(';');
		for(int i = 0; i<info.Length; i++)
		{
			if(info[i].IndexOf("=") != -1)
			{
				string sub = info[i].Substring(info[i].IndexOf("=")+1, (info[i].Length-1)-info[i].IndexOf("="));
//				Debug.Log (sub);

				if(info[i].IndexOf("Board") != -1)
				{
					string [] tilesInfo = sub.Split(',');
					for(int j = 0; j<boardList.Count; j++)
					{
//						Debug.Log("Board: " + (Tile.TileType)char.GetNumericValue(tilesInfo[j], 0) + " " + (PlayerVO.PlayerType)char.GetNumericValue(tilesInfo[j], 1));
						boardList[i].currentTileType = (Tile.TileType)char.GetNumericValue(tilesInfo[j], 0);
						boardList[i].currentTilePlayerType = (PlayerVO.PlayerType)char.GetNumericValue(tilesInfo[j], 1);
					}
				}
			}
//			Debug.Log (info[i] + " " + info[i].IndexOf("=") + " " + (info[i].Length));
		}


	}

	public string ConvertBoardToStrings(List<Tile> list)
	{
		string toPrint = "";
		toPrint += "Turn=";
		if(currentPlayerTurn.currentPlayerType == PlayerVO.PlayerType.friend)
		{
			toPrint +="B;";
		}
		else
		{
			toPrint +="R;";
		}
		toPrint += "B=" + player1.currentTurnPointCount + ";";
		toPrint += "R=" + player2.currentTurnPointCount + ";";

		toPrint += "Board=";
		for(int i=0; i<list.Count; i++)
		{
			toPrint += (int)list[i].currentTileType;
			toPrint += (int)list[i].currentTilePlayerType;
			toPrint += (int)list[i].occupyingTokenId;
			toPrint += ",";
		}
		toPrint += ";";
		return toPrint;
	}
}