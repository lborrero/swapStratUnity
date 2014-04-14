using UnityEngine;
using System.Collections;

public class Token : MonoBehaviour {

	public GameObject physicalToken;
	private int _tokenId;
	public int tokenId{
		get { return this._tokenId; }
	}
	public int xPos;
	public int yPos;
	
	public Material blueMaterial;
	public Material blueUsedMaterial;
	public Material blueSelectedMaterial;
	public Material redMaterial;
	public Material redUsedMaterial;
	public Material redSelectedMaterial;
	
	public enum TokenState
	{
		unselected = 0,
		selected
	}
	public TokenState currentTokenState = TokenState.unselected;

	public bool hasTokenBeenUsed = false;
	public bool isTokenOnBoard = false;
	
	public enum TokenType
	{
		normal = 0,
		friendly,
		benchFriendly,
		enemy,
		benchEnemy
	}
	public TokenType currentTokenType = TokenType.normal;

	public PlayerVO.PlayerType tokenPlayerType{
		get { 
			PlayerVO.PlayerType tmpppt = new PlayerVO.PlayerType();
			if(currentTokenType == TokenType.benchFriendly || currentTokenType == TokenType.friendly)
				tmpppt = PlayerVO.PlayerType.friend;
			else if (currentTokenType == TokenType.benchEnemy || currentTokenType == TokenType.enemy)
				tmpppt = PlayerVO.PlayerType.enemy;
			return tmpppt;
		}
	}
	
	public void SetTokenId(int tokenId)
	{
		_tokenId = tokenId;
	}
	
	public void SetCoordinates(int x, int y)
	{
		xPos = x;
		yPos = y;
	}

	public void UpdateState()
	{
		switch(currentTokenType)
		{
		case TokenType.friendly:
		case TokenType.benchFriendly:
			setBlueColor();
			break;
		case TokenType.enemy:
		case TokenType.benchEnemy:
			setRedColor();
			break;
		}
	}

	void setBlueColor()
	{
		if(isTokenOnBoard == true)
			physicalToken.gameObject.renderer.material = blueUsedMaterial;
		else
		{
			switch(currentTokenState)
			{
			case TokenState.selected:
				physicalToken.gameObject.renderer.material = blueSelectedMaterial;
				break;
			case TokenState.unselected:
				physicalToken.gameObject.renderer.material = blueMaterial;
				break;
			}
		}
	}

	void setRedColor()
	{
		if(isTokenOnBoard == true)
			physicalToken.gameObject.renderer.material = redUsedMaterial;
		else
		{
			switch(currentTokenState)
			{
			case TokenState.selected:
				physicalToken.gameObject.renderer.material = redSelectedMaterial;
				break;
			case TokenState.unselected:
				physicalToken.gameObject.renderer.material = redMaterial;
				break;
			}
		}
	}

	void OnMouseDown()
	{
		if((currentTokenType == TokenType.benchEnemy || currentTokenType == TokenType.benchFriendly) && !isTokenOnBoard)
		{
			sBoardManager.Instance.TokenClicked(_tokenId, currentTokenType);
		}
	}
}
