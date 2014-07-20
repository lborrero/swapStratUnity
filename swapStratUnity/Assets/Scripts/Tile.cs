﻿using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public GameObject physicalTile;
	public GameObject physicalOverLay;

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

	public Material selectedMaterial;
	public Material unselectedMaterial;
	public Material highlightedMaterial;
	public Material nothingdMaterial;

	public Material blueMaterial;
	public Material blueHighlightMaterial;
	public Material blueSelectedMaterial;
	public Material redMaterial;
	public Material redHighlightMaterial;
	public Material redSelectedMaterial;

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
		highlighted,
		blocked
	}
	public TileVisualState currentTileVisualState = TileVisualState.unselected;

	public enum TileType
	{
		nothing = 0,
		empty,
		occupied
	}
	public TileType currentTileType = TileType.empty;

	public void SetTileId(int tildeId)
	{
		_tileId = tildeId;
	}

	public void SetCoordinates(int x, int y)
	{
		_xPos = x;
		_yPos = y;
	}

	void OnMouseDown()
	{
		sBoardManager.Instance.TileClicked (_tileId);
	}

	public void UpdateState()
	{
		switch(currentTileType)
		{
		case TileType.nothing:
			physicalTile.gameObject.renderer.material = nothingdMaterial;
			break;
		case TileType.empty:
		case TileType.occupied:
			switch(currentTileVisualState)
			{
			case TileVisualState.selected:
				switch(currentTilePlayerType)
				{
				case PlayerVO.PlayerType.none:
					physicalTile.gameObject.renderer.material = selectedMaterial;
					break;
				case PlayerVO.PlayerType.friend:
					physicalTile.gameObject.renderer.material = blueSelectedMaterial;
					break;
				case PlayerVO.PlayerType.enemy:
					physicalTile.gameObject.renderer.material = redSelectedMaterial;
					break;
				}
				break;
			case TileVisualState.unselected:
				switch(currentTilePlayerType)
				{
				case PlayerVO.PlayerType.none:
					physicalTile.gameObject.renderer.material = unselectedMaterial;
					break;
				case PlayerVO.PlayerType.friend:
					physicalTile.gameObject.renderer.material = blueMaterial;
					break;
				case PlayerVO.PlayerType.enemy:
					physicalTile.gameObject.renderer.material = redMaterial;
					break;
				}
				break;
			case TileVisualState.highlighted:
				switch(currentTilePlayerType)
				{
				case PlayerVO.PlayerType.none:
					physicalTile.gameObject.renderer.material = highlightedMaterial;
					break;
				case PlayerVO.PlayerType.friend:
					physicalTile.gameObject.renderer.material = blueHighlightMaterial;
					break;
				case PlayerVO.PlayerType.enemy:
					physicalTile.gameObject.renderer.material = redHighlightMaterial;
					break;
				}
				break;
			}
			break;
		}
	}
}
