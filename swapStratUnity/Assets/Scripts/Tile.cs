using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tile : MonoBehaviour {

	public Image tileSupport;
	public Image tile;
	public Image tileShading;
	public Image highlight;
	public Image selectionMarker;

	public int occupyingTokenId = -1;
	public PlayerVO.PlayerType occupyingTokenPlayerType = PlayerVO.PlayerType.none;

	private int _tileId;
	public int tileId{
		get { return this._tileId; }
	}
	private int _xPos;
	private int _yPos;
	public int xPos{
		get { return this._xPos; }
	}
	public int yPos{
		get { return this._yPos; }
	}

	public PlayerVO.PlayerType currentTilePlayerType = PlayerVO.PlayerType.none;

	public Color nothingColor;
	
	public Color selectedColor;
	public Color unselectedColor;

	public Color emptyColor;
	public Color blueColor;
	public Color blueGuardedColor;
	public Color redColor;
	public Color redGuardedColor;

	public enum TileState
		{
		unselected = 0,
		selected
		}
	public TileState currentTileState = TileState.unselected;

	public enum TileVisualState
	{
		unselected = 0,
		selected,
		highlighted
	}
	public TileVisualState currentTileVisualState = TileVisualState.unselected;

	public enum TileType
	{
		nothing = 0,
		empty,
		occupied
	}
	public TileType currentTileType = TileType.empty;

	public enum TileGuarded
	{
		untaken = 0,
		taken,
		guarded
	}
	public TileGuarded currentGuardState = TileGuarded.untaken;

	public void SetTileId(int tildeId)
	{
		_tileId = tildeId;
	}

	public void SetCoordinates(int x, int y)
	{
		_xPos = x;
		_yPos = y;
	}

	void OnMouseUp()
	{
		sBoardManager.Instance.TileClicked (_tileId);
	}

	public bool DepositeTokenHere()
	{
		return sBoardManager.Instance.TileClicked (_tileId);
	}

	public void UpdateState()
	{
		switch(currentTileType)
		{
		case TileType.nothing:
			tileSupport.gameObject.SetActive(false);
			tile.gameObject.SetActive(false);
			tileShading.gameObject.SetActive(false);
			highlight.gameObject.SetActive(false);
			selectionMarker.gameObject.SetActive(false);
			break;
		case TileType.empty:
		case TileType.occupied:
			tile.color = emptyColor;
			tile.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);
			highlight.gameObject.SetActive(false);

			switch(currentTileVisualState)
			{
			case TileVisualState.selected:
//				selectionMarker.gameObject.SetActive(true); 
//				selectionMarker.color = selectedColor;
				break;
			case TileVisualState.unselected:
				break;
			case TileVisualState.highlighted:
				if(currentTileType == TileType.occupied)
				{
//					selectionMarker.gameObject.SetActive(true); 
//					selectionMarker.color = unselectedColor;
				}
				else
				{
					highlight.gameObject.SetActive(true);
					highlight.color = selectedColor;
				}
				break;
			}

			switch(currentTilePlayerType)
			{
			case PlayerVO.PlayerType.none:
				tile.color = emptyColor;
				break;
			case PlayerVO.PlayerType.friend:
				if (currentGuardState == TileGuarded.taken) 
				{
					tile.color = blueColor;
				} 
				else if (currentGuardState == TileGuarded.guarded)
				{
					tile.color = blueGuardedColor;
				}
				break;
			case PlayerVO.PlayerType.enemy:
				if (currentGuardState == TileGuarded.taken) 
				{
					tile.color = redColor;
				} 
				else if (currentGuardState == TileGuarded.guarded)
				{
					tile.color = redGuardedColor;
				}
				break;
			}
			break;
		}
	}
}
