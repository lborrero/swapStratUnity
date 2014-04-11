	using UnityEngine;
using System.Collections;

public class SwapBoard : MonoBehaviour {

	public GameObject swapTilePrefab;

	private int _width = 6;
	private int _height = 6;
	private float _xOffset;
	private float _yOffset;

	// Use this for initialization
	void Start () {
		_xOffset = _width / -2 + 0.5f;
		_yOffset = _height / -2 + 0.5f;

		int idCounter = 1;
		for(int i=0; i<_height; i++)
		{
			for(int j=0; j<_width; j++)
			{
				GameObject tmp = (GameObject)Instantiate(swapTilePrefab.gameObject, new Vector3(_xOffset + i,0,_yOffset + j) , transform.rotation);
				tmp.GetComponent<Tile>().SetCoordinates(i, j);
				tmp.GetComponent<Tile>().SetTileId(idCounter);
				idCounter++;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
}
