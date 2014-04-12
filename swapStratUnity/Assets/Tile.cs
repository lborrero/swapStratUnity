using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public GameObject physicalTile;
	private int _tileId;
	private int _xPos;
	private int _yPos;
	public int xPos{
		get { return this._xPos; }
	}
	public int yPos{
		get { return this._yPos; }
	}

	public Material selectedMaterial;
	public Material unselectedMaterial;
	public Material highlightedMaterial;

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
	public TileType currentTileType = TileType.nothing;

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
		switch(currentTileVisualState)
		{
		case TileVisualState.selected:
			physicalTile.gameObject.renderer.material = selectedMaterial;
			break;
		case TileVisualState.unselected:
			physicalTile.gameObject.renderer.material = unselectedMaterial;
			break;
		case TileVisualState.highlighted:
			physicalTile.gameObject.renderer.material = highlightedMaterial;
			break;
		}
	}
}
