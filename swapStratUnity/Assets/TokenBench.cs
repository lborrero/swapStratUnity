using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenBench : MonoBehaviour {
	
	public List<GameObject> benchedTokens;

	public enum PlayerType
	{
		friend = 0,
		enemy
	}
	public PlayerType currentPlayer;

	void Start()
	{
		if(currentPlayer == PlayerType.enemy)
			sBoardManager.Instance.enemyBench = this;
		else
			sBoardManager.Instance.friendlyBench = this;

		for(int i=0; i<benchedTokens.Count; i++)
		{
			benchedTokens[i].GetComponent<Token>().currentTokenType = (currentPlayer == PlayerType.friend) ? Token.TokenType.benchFriendly : Token.TokenType.benchEnemy;
			benchedTokens[i].GetComponent<Token>().SetTokenId(i);
			benchedTokens[i].GetComponent<Token>().UpdateState();
		}
	}

	public bool isTokenUsed(int tokenId)
	{
		bool isUsed = false;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			if(tokenId == benchedTokens[i].GetComponent<Token>().tokenId && !benchedTokens[i].GetComponent<Token>().hasTokenBeenUsed)
				isUsed = true;
		}
		return isUsed;
	}

	public void SelectAToken(int tokenId)
	{
		for(int i=0; i<benchedTokens.Count; i++)
		{
			if(benchedTokens[i].GetComponent<Token>().tokenId == tokenId)
			{
				benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.selected;
			}
			else
			{
				benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.unselected;
			}
			benchedTokens[i].GetComponent<Token>().UpdateState();
		}
	}
}
