using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SwapBoard : MonoBehaviour {

	public GameObject swapTilePrefab;
	public GameObject swapTokenPrefab;

	public Text playerMoveCounterFriend;
	public Text playerMoveCounterEnemy;

	public Text playerPointCounterFriend;
	public Text playerPointCounterEnemy;
	
	public TokenBench friendlyBench;
	public TokenBench enemyBench;

	public Text gameActionLabel;

	public int width = 6;
	public int height = 6;
	private float _xOffset;
	private float _yOffset;

	// Use this for initialization
	void Start () {
		InitializeBoard ();
	}

	public void SetBoardAtSpecificState()
	{
		sGameManager.Instance.currentInnerGameLoop = sGameManager.InnerGameLoop.playerOneTurn;
		sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBench;
		sBoardManager.Instance.currentlySelectedTile = new Tile();
		sBoardManager.Instance.currentlySelectedToken = new Token();

		sBoardManager.Instance.player1.InitializePlayCount (PlayerVO.PlayerType.friend, friendlyBench);
		sBoardManager.Instance.player2.InitializePlayCount (PlayerVO.PlayerType.enemy, enemyBench);
		sBoardManager.Instance.currentPlayerTurn = sBoardManager.Instance.player1;
	}

	public void InitializeBoard()
	{
		sGameManager.Instance.currentInnerGameLoop = sGameManager.InnerGameLoop.playerOneTurn;
		sGameManager.Instance.currentTurnLoop = sGameManager.TurnLoop.selectATokenFromBench;
		sBoardManager.Instance.currentlySelectedTile = new Tile();
		sBoardManager.Instance.currentlySelectedToken = new Token();

		sBoardManager.Instance.board_width = width;
		sBoardManager.Instance.board_height = height;
		sBoardManager.Instance.boardView = this;

		sBoardManager.Instance.player1.InitializePlayCount (PlayerVO.PlayerType.friend, friendlyBench);
		sBoardManager.Instance.player2.InitializePlayCount (PlayerVO.PlayerType.enemy, enemyBench);
		sBoardManager.Instance.currentPlayerTurn = sBoardManager.Instance.player1;
		UpdateCounters ();

		_xOffset = width / -2 + 0.5f;
		_yOffset = height / -2 + 0.5f;
		
		int idCounter = 0;
		for(int i=0; i<height; i++)
		{
			for(int j=0; j<width; j++)
			{
				GameObject tmp = (GameObject)Instantiate(swapTilePrefab.gameObject, new Vector3(_xOffset + i,0,_yOffset + j) , transform.rotation);
				tmp.GetComponent<Tile>().SetCoordinates(j, i);
				tmp.GetComponent<Tile>().SetTileId(idCounter);
				
				if(i==0 || i==height-1 || j==0 || j==width-1)
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
		
		//begin match visual
		friendlyBench.UpdateTokenBenchDisplay (TokenBench.benchState.suggestAToken);
	}

//	public moveTokenAnimation()
//	{
//		AstarPathfinding.Instance.GenerateBoard (sBoardManager.Instance., 5, 0, 24);
//		asp.ComputePathSequence ();
//	}

	public void AddTokenOnTileWithTokenId(int tileId, int tokenId, PlayerVO playerPlaying)
	{
		//		Debug.Log ("AddTokenOnTile");
		sBoardManager sb = sBoardManager.Instance;
		Tile tmpTile = sb.boardList [tileId];
		
		//occupy tile
		tmpTile.currentTileType = Tile.TileType.occupied;
		
		//set selected bench token as used
		playerPlaying.playerTokenBench.UpdateTokenBenchDisplay(TokenBench.benchState.disabled);
		playerPlaying.playerTokenBench.SetTokenAsUsed(tokenId);
		
		//define placed token
		GameObject tmp = (GameObject)Instantiate (swapTokenPrefab.gameObject, tmpTile.gameObject.transform.position, tmpTile.gameObject.transform.rotation);
		Token tmpToken = tmp.GetComponent<Token> ();
		tmpToken.SetTokenId (tokenId);
		tmpToken.currentTokenType = sb.currentlySelectedToken.currentTokenType;
		tmpToken.xPos = tmpTile.xPos;
		tmpToken.yPos = tmpTile.yPos;
		tmpToken.UpdateState ();
		sBoardManager.Instance.tokenList.Add(tmp.GetComponent<Token>());
	}

	public void AddTokenOnTile(int tileId)
	{
//		Debug.Log ("AddTokenOnTile");
		sBoardManager sb = sBoardManager.Instance;
		Tile tmpTile = sb.boardList [tileId];

		//occupy tile
		tmpTile.currentTileType = Tile.TileType.occupied;

		//set selected bench token as used
		sb.currentPlayerTurn.playerTokenBench.UpdateTokenBenchDisplay(TokenBench.benchState.disabled);
		sb.currentPlayerTurn.playerTokenBench.SetTokenAsUsed(sb.currentlySelectedToken.tokenId);

		//define placed token
		GameObject tmp = (GameObject)Instantiate (swapTokenPrefab.gameObject, tmpTile.gameObject.transform.position, tmpTile.gameObject.transform.rotation);
		Token tmpToken = tmp.GetComponent<Token> ();
		tmpToken.SetTokenId (sb.currentlySelectedToken.tokenId);
		tmpToken.currentTokenType = sb.currentlySelectedToken.currentTokenType;
		tmpToken.xPos = tmpTile.xPos;
		tmpToken.yPos = tmpTile.yPos;
		tmpToken.UpdateState ();
		sBoardManager.Instance.tokenList.Add(tmp.GetComponent<Token>());
	}

	public void UpdateCounters()
	{
		playerPointCounterFriend.text = sBoardManager.Instance.player1.currentTurnPointCount.ToString ();
		playerPointCounterEnemy.text = sBoardManager.Instance.player2.currentTurnPointCount.ToString ();
		playerMoveCounterFriend.text = sBoardManager.Instance.player1.currentTurnMoveCount.ToString();
		playerMoveCounterEnemy.text = sBoardManager.Instance.player2.currentTurnMoveCount.ToString();
	}

	public void SetGameActionLabel(string str)
	{
		gameActionLabel.text = str;
	}
}
