using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour {

	public GameObject physicalToken;
	private int _tokenId;
	private int _xPos;
	private int _yPos;
	public int xPos{
		get { return this._xPos; }
	}
	public int yPos{
		get { return this._yPos; }
	}
	
	public Material blueMaterial;
	public Material redMaterial;
	
	public enum TokenState
	{
		unselected = 0,
		selected
	}
	public TokenState currentTokenState = TokenState.unselected;
	
	public enum TokenVisualState
	{
		unselected = 0,
		selected,
		highlighted
	}
	public TokenVisualState currentTokenVisualState = TokenVisualState.unselected;
	
	public enum TokenType
	{
		normal = 0,
		friendly,
		enemy
	}
	public TokenType currentTokenType = TokenType.normal;
	
	public void SetTokenId(int tokenId)
	{
		_tokenId = tokenId;
	}
	
	public void SetCoordinates(int x, int y)
	{
		_xPos = x;
		_yPos = y;
	}

	public void UpdateState()
	{
		Debug.Log ("currentTokenType: " + currentTokenType);
		switch(currentTokenType)
		{
		case TokenType.friendly:
			physicalToken.gameObject.renderer.material = blueMaterial;
			break;
		case TokenType.enemy:
			physicalToken.gameObject.renderer.material = redMaterial;
			break;
		}
	}
}
