using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwapBoard : MonoBehaviour {

	public GameObject swapTilePrefab;
	public GameObject swapTokenPrefab;

	private int _width = 8;
	private int _height = 8;
	private float _xOffset;
	private float _yOffset;

	// Use this for initialization
	void Start () {
		sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBench;
		sBoardManager.Instance.currentlySelectedTile = new Tile();
		sBoardManager.Instance.currentlySelectedToken = new Token();
		sBoardManager.Instance.width = _width;
		sBoardManager.Instance.height = _height;
		sBoardManager.Instance.boardView = this;
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

				if(i==0 || i==_height-1 || j==0 || j==_width-1)
				{
					tmp.GetComponent<Tile>().currentTileType = Tile.TileType.nothing;
				}
				else
				{
					tmp.GetComponent<Tile>().currentTileType = Tile.TileType.empty;
				}
				tmp.GetComponent<Tile>().UpdateState();

				sBoardManager.Instance.boardList.Add(tmp.GetComponent<Tile>());
				idCounter++;
			}
		}
	}

	public void AddTokenOnTile(int tileId, Token.TokenType tt)
	{
//		Debug.Log ("AddTokenOnTile");
		GameObject tmp = (GameObject)Instantiate (swapTokenPrefab.gameObject, sBoardManager.Instance.boardList [tileId].gameObject.transform.position, sBoardManager.Instance.boardList [tileId].gameObject.transform.rotation);
		tmp.GetComponent<Token> ().SetTokenId (sBoardManager.Instance.tokenList.Count);
		tmp.GetComponent<Token> ().currentTokenType = tt;
		tmp.GetComponent<Token> ().UpdateState ();
		sBoardManager.Instance.tokenList.Add(tmp.GetComponent<Token>());
	}
}
