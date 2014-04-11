using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public GameObject physicalTile;
	private int _tileId;
	private int _xPos;
	private int _yPos;

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
		print ("Tile OnMouseDown: tileId: " + _tileId + " coordinates: " + _xPos + "," + _yPos);
	}
}
