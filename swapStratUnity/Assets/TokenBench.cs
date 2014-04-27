using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenBench : MonoBehaviour {
	
	public List<GameObject> benchedTokens;

	public PlayerVO.PlayerType currentPlayerType;

	public void InitialitializeBench(PlayerVO.PlayerType plt)
	{
		currentPlayerType = plt;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			benchedTokens[i].GetComponent<Token>().currentTokenType = (currentPlayerType == PlayerVO.PlayerType.friend) ? Token.TokenType.benchFriendly : Token.TokenType.benchEnemy;
			benchedTokens[i].GetComponent<Token>().SetTokenId(i);
			benchedTokens[i].GetComponent<Token>().UpdateState();
		}
	}

	public void SetTokenAsUsed(int tokenId)
	{
		benchedTokens [tokenId].GetComponent<Token> ().isTokenOnBoard = true;
		benchedTokens [tokenId].GetComponent<Token> ().UpdateState ();
	}

	public bool HasAvailableTokens()
	{
		bool has = false;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			if(!benchedTokens[i].GetComponent<Token>().isTokenOnBoard)
			{
				has = true;
				break;
			}
		}
		return has;
	}
	
	public bool isTokenUsed(int tokenId)
	{
		bool isUsed = false;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			if(tokenId == benchedTokens[i].GetComponent<Token>().tokenId && !benchedTokens[i].GetComponent<Token>().hasTokenBeenMoved)
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

	public void UnSelectAllBenchTokens()
	{
		for(int i=0; i<benchedTokens.Count; i++)
		{
			benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.unselected;
			benchedTokens[i].GetComponent<Token>().UpdateState();
		}
	}
}
