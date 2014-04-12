using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwapBoard : MonoBehaviour {

	public GameObject swapTilePrefab;

	private int _width = 8;
	private int _height = 8;
	private float _xOffset;
	private float _yOffset;

	// Use this for initialization
	void Start () {
		sBoardManager.Instance.width = _width;
		sBoardManager.Instance.height = _height;
		_xOffset = _width / -2 + 0.5f;
		_yOffset = _height / -2 + 0.5f;

		int idCounter = 0;
		for(int i=0; i<_height; i++)
		{
			for(int j=0; j<_width; j++)
			{
				GameObject tmp = (GameObject)Instantiate(swapTilePrefab.gameObject, new Vector3(_xOffset + i,0,_yOffset + j) , transform.rotation);
				tmp.GetComponent<Tile>().SetCoordinates(j, i);
				tmp.GetComponent<Tile>().SetTileId(idCounter);
				sBoardManager.Instance.boardList.Add(tmp.GetComponent<Tile>());
				idCounter++;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
}
