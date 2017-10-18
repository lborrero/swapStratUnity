﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class Token : MonoBehaviour {

	public Image TokenShadow;
	public Image TokenSupport;
	public Image TokenSupportShadow;
	public Image selectionMarker;
	public Image physicalToken;
	public Image DotSupport;
	public Image dot;
	public Image TokenLight;

	public GameObject dotedLine;

	Animator anim;
	int ShakeAnimationTrigger = Animator.StringToHash("ShakeAnimationTrigger");
	int NothingTrigger = Animator.StringToHash("NothingTrigger");
	Animation animation;

	private List<Vector3> moveSequence = new List<Vector3>();
	private Vector3 destinationPosition = new Vector3();
	private Vector3 previousPosition = new Vector3();
	bool permissionToMove = false;
	bool inInteraction = false;
	float stepsToMoveToDesintation = 0.2f;
	Vector3 startPosition;
	Collider collider;

	List<GameObject> pathLine = new List<GameObject>();

	void Start()
	{
		anim = GetComponent<Animator>();
		animation = GetComponent<Animation> ();
		startPosition = gameObject.transform.position;
		collider = GetComponent<Collider> ();
	}

	void FixedUpdate()
	{
		if(permissionToMove && !inInteraction)
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
	public int lockedTime = 1;
	public int lockedCounter;
	
	public Color blueColor;
	public Color disabledBlueColor;
	public Color redColor;
	public Color disabledRedColor;

	public Color selectedColor;
	public Color unselectedColor;
	public Color disabledUnselectedColor;
	public Color lockedColor;

	public Color selectionMarkerColor;

	public enum TokenState
	{
		unselected = 0,
		selected,
		highlighted,
		disabled,
		hideToken,
		lockedToken
	}
	public TokenState currentTokenState = TokenState.unselected;
	public TokenState CurrentTokenState
	{
		get
		{
			return currentTokenState;
		}
		set
		{ 
			if (isTokenLocked()) 
			{
				currentTokenState = TokenState.lockedToken;
			}
			else
			{
				currentTokenState = value;
			}
		}
	}

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

	public bool isTokenLocked()
	{
		return (lockedCounter > 0);
	}

	public void StartLockToken ()
	{
		lockedCounter = lockedTime;
	}

	public void UpdateLockedTime()
	{
		if (lockedCounter > 0) {
			lockedCounter--;
		}
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
			if(anim != null)
			{
				anim.ResetTrigger(NothingTrigger);
				anim.SetTrigger (NothingTrigger);
			}
			break;
		case TokenState.unselected:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			selectionMarker.color = selectionMarkerColor;
			physicalToken.color = unselectedColor;
			TokenSupport.color = unselectedColor;
			break;
		case TokenState.highlighted:
			physicalToken.gameObject.SetActive(true);
			selectionMarker.gameObject.SetActive(false);

			dot.gameObject.SetActive(true);
			physicalToken.gameObject.SetActive(true);
			physicalToken.color = unselectedColor;
			TokenSupport.color = unselectedColor;
			selectionMarker.gameObject.SetActive(true);
			if(anim != null)
			{
				if(sGameManager.Instance.currentTurnLoop == sGameManager.TurnLoop.selectATokenFromBoard &&
				   sGameManager.Instance.TurnCount < 4)
				{
					anim.ResetTrigger(ShakeAnimationTrigger);
					anim.SetTrigger (ShakeAnimationTrigger);
				}
				else
				{
					anim.ResetTrigger(NothingTrigger);
					anim.SetTrigger (NothingTrigger);
				}
			}
			selectionMarker.color = selectionMarkerColor;
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
			TokenSupport.color = disabledUnselectedColor;
			selectionMarker.gameObject.SetActive(false);
			break;
		case TokenState.lockedToken:
			physicalToken.color = lockedColor;
			TokenSupport.color = lockedColor;
			break;
		case TokenState.hideToken:
			dot.gameObject.SetActive(false);
			physicalToken.gameObject.SetActive(false);
			selectionMarker.gameObject.SetActive(false);
			TokenShadow.gameObject.SetActive(false);
			TokenSupport.gameObject.SetActive(false);
			TokenSupportShadow.gameObject.SetActive(false);
			DotSupport.gameObject.SetActive(false);
			TokenLight.gameObject.SetActive(false);
			break;
		}
	}

	void OnMouseDrag()
	{
		if (tokenPlayerType == sBoardManager.Instance.currentPlayerTurn.currentPlayerType &&
		    currentTokenState != TokenState.disabled &&
		    currentTokenState != TokenState.hideToken) 
		{
			inInteraction = true; 

			collider.enabled = false;

			var v3 = Input.mousePosition;
			v3.z = 20f;
			v3 = Camera.main.ScreenToWorldPoint(v3);
			gameObject.transform.position = v3;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			Tile hitTile = new Tile();
			bool drawTheLines = false;
			if (Physics.Raycast(ray, out hit, 30)) {
//				Debug.DrawLine(ray.origin, hit.point);
//				Debug.Log(hit.collider.gameObject.name);
				hitTile = hit.collider.gameObject.GetComponent<Tile>();
				if(hitTile != null && sBoardManager.Instance.CheckIfCanMoveToTileId(hitTile.tileId))
				{
					drawTheLines = true;
				}
			}

			if(drawTheLines && isTokenABenchTokenAndNotOnBoard())
			{
				List<Vector3> path = sBoardManager.Instance.GeneratePathSequence(sBoardManager.Instance.currentlySelectedTile.tileId, hitTile.tileId);
//				pathLine.SetWidth(startWidth, endWidth);
				DestroyPathLine();

				for(int i = 0; i < path.Count-1; i ++)
				{
					GameObject tmp = (GameObject)Instantiate(dotedLine, new Vector3(path[i].x, (path[i].y + 1f), path[i].z), transform.rotation);
					pathLine.Add(tmp);
				}
			}
			else
			{
				if(isTokenABenchTokenAndNotOnBoard())
				{
					DestroyPathLine();
				}
			}
		}
	}

	void DestroyPathLine()
	{
		for(int i = 0; i < pathLine.Count; i ++)
		{
			Destroy(pathLine[i].gameObject);
		}
		pathLine.Clear();
	}

	bool isTokenABenchTokenAndNotOnBoard()
	{
		return (!((currentTokenType == TokenType.benchEnemy || currentTokenType == TokenType.benchFriendly) && !isTokenOnBoard));
	}

	void OnMouseUp()
	{
		if (tokenPlayerType == sBoardManager.Instance.currentPlayerTurn.currentPlayerType &&
		    currentTokenState != TokenState.disabled &&
		    currentTokenState != TokenState.hideToken) 
		{
			collider.enabled = false;

			if(isTokenABenchTokenAndNotOnBoard())
			{
				DestroyPathLine();
			}
			
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 30)) {
//				Debug.DrawLine(ray.origin, hit.point);
//				Debug.Log(hit.collider.gameObject.name);
				Tile hitTile = hit.collider.gameObject.GetComponent<Tile>();
				if(hitTile != null && !hitTile.DepositeTokenHere())
				{
				}
			}
			collider.enabled = true;

			if(!permissionToMove)
			{
				gameObject.transform.position = startPosition;
			}
			else
			{
				List<Vector3> pathPositionList = new List<Vector3>();
				pathPositionList.Add(moveSequence[moveSequence.Count-1]);
				moveSequence.Clear();
				moveThrough(pathPositionList);
			}
			inInteraction = false;
		}
	}

	void OnMouseDown()
	{
		if (tokenPlayerType == sBoardManager.Instance.currentPlayerTurn.currentPlayerType &&
		    currentTokenState != TokenState.disabled &&
		    currentTokenState != TokenState.hideToken) 
		{
			inInteraction = true; 

			collider.enabled = false;

			startPosition = gameObject.transform.position;
		
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 30)) 
			{
				Tile hitTile = hit.collider.gameObject.GetComponent<Tile> ();
				if (hitTile != null && !hitTile.DepositeTokenHere ()) 
				{
				}
			}
			collider.enabled = true;

			if ((currentTokenType == TokenType.benchEnemy || currentTokenType == TokenType.benchFriendly) && !isTokenOnBoard) 
			{
				sBoardManager.Instance.TokenClicked (_tokenId, currentTokenType);
			}
		}
	}
}
