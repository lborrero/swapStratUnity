using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class sBoardManager : MonoBehaviour
{
	private static sBoardManager instance;

	public SwapBoard boardView;

	public List<Tile> boardList;
	public int width;
	public int height;
	
	private sBoardManager() {}
	
	public static sBoardManager Instance
	{
		get 
		{
			if (instance == null)
			{
				GameObject go = new GameObject();
				instance = go.AddComponent<sBoardManager>();
				instance.boardList = new List<Tile>();
			}
			return instance;
		}
	}

	public void TileClicked(int tileId)
	{
//		if(sGameManager.Instance.currentGameState == sGameManager.GameState.placingTokens)
//		{
		boardView.AddTokenOnTile (tileId);
//		}
//		else
//		{
			SelectingTilesToMove(tileId);
//		}
		UpdateBoard();
	}

	void SelectingTilesToMove(int tileId)
	{
		if(boardList [tileId].currentTileState == Tile.TileState.selected)
		{
			boardList [tileId].currentTileState = Tile.TileState.unselected;
			boardList [tileId].currentTileVisualState = Tile.TileVisualState.unselected;
		}
		else
		{
			boardList [tileId].currentTileState = Tile.TileState.selected;
			boardList [tileId].currentTileVisualState = Tile.TileVisualState.unselected;
		}
		
		UnhighlightBoard ();
		
		List<int> contiguousTiles = ContiguousBlockSearch.returnContiguousFromTile (boardListIntoBinaryList (Tile.TileState.selected), width, height, boardList [tileId].xPos, boardList [tileId].yPos); 
		Debug.Log ("contiguousTiles: " + ListsToStrings(contiguousTiles));

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

	List<int> boardListIntoBinaryList(Tile.TileState tileState)
	{
		List<int> tmpArray = new List<int> ();
		for(int i = 0; i<boardList.Count; i++)
		{
			tmpArray.Add((boardList[i].currentTileState == tileState)?1:0);
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