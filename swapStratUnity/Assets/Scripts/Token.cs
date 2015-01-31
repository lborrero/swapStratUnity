using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Token : MonoBehaviour {

	public Image dot;
	public Image physicalToken;
	public Image selectionMarker;

	private List<Vector3> moveSequence = new List<Vector3>();
	private Vector3 destinationPosition = new Vector3();
	private Vector3 previousPosition = new Vector3();
	bool permissionToMove = false;
	float stepsToMoveToDesintation = 0.2f;
	void FixedUpdate()
	{
		if(permissionToMove)
		{
			float v3distance = Vector3.Distance(destinationPosition, this.gameObject.transform.position);
			
			if(!(v3distance < stepsToMoveToDesintation))
			{
				Vector3 v3 = new Vector3();
				v3.x = (destinationPosition.x - previousPosition.x)*stepsToMoveToDesintation;
				v3.y = (destinationPosition.y - previousPosition.y)*stepsToMoveToDesintation;
				v3.z = (destinationPosition.z - previousPosition.z)*stepsToMoveToDesintation;
				
				this.gameObject.transform.Translate(v3);
			}
			else
			{
				this.gameObject.transform.position = destinationPosition;
			}

			if(moveSequence.Count > 0)
			{
				if(v3distance < stepsToMoveToDesintation)
				{
					moveSequence.RemoveAt(0);
					if(moveSequence.Count > 0)
					{
						destinationPosition = moveSequence[0];
					}
					previousPosition = this.gameObject.transform.position;
				}
			}
			else
			{
				permissionToMove = false;
			}
		}
	}

	public void moveThrough(List<Vector3> pathPositionList)
	{
		moveSequence = pathPositionList;
		destinationPosition = moveSequence[0];
		permissionToMove = true;
		previousPosition = this.gameObject.transform.position;
	}


	private int _tokenId;
	public int tokenId{
		get { return this._tokenId; }
	}
	public int xPos;
	public int yPos;
	
	public Color blueColor;
	public Color disabledBlueColor;
	public Color redColor;
	public Color disabledRedColor;

	public Color selectedColor;
	public Color unselectedColor;
	public Color disabledUnselectedColor;
	
	public enum TokenState
	{
		unselected = 0,
		selected,
		highlighted,
		disabled,
		hideToken
	}
	public TokenState currentTokenState = TokenState.unselected;

	public bool hasTokenBeenMoved = false;
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
			dot.color = blueColor;
			setTokenState();
			break;
		case TokenType.enemy:
		case TokenType.benchEnemy:
			dot.color = redColor;
			setTokenState();
			break;
		}
	}

	void setTokenState()
	{
		switch(currentTokenState)
		{
		case TokenState.selected:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			selectionMarker.gameObject.SetActive(true);
			selectionMarker.color = selectedColor;
			physicalToken.color = selectedColor;
			break;
		case TokenState.unselected:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			selectionMarker.color = unselectedColor;
			physicalToken.color = unselectedColor;
			break;
		case TokenState.highlighted:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			dot.gameObject.SetActive(true);
			physicalToken.gameObject.SetActive(true);
			physicalToken.color = unselectedColor;
			selectionMarker.gameObject.SetActive(true);
			selectionMarker.color = unselectedColor;
			break;
		case TokenState.disabled:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			dot.gameObject.SetActive(true);
			if(currentTokenType == TokenType.benchEnemy)
			{
				dot.color = disabledRedColor;
			}
			if(currentTokenType == TokenType.benchFriendly)
			{
				dot.color = disabledBlueColor;
			}
			physicalToken.gameObject.SetActive(true);
			physicalToken.color = disabledUnselectedColor;
			selectionMarker.gameObject.SetActive(false);
			break;
		case TokenState.hideToken:
			dot.gameObject.SetActive(false);
			physicalToken.gameObject.SetActive(false);
			selectionMarker.gameObject.SetActive(false);
			break;
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
