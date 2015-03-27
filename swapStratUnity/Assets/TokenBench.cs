using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TokenBench : MonoBehaviour {
	
	public List<GameObject> benchedTokens;

	public PlayerVO.PlayerType currentPlayerType;

	public enum benchState
	{
		disabled = 0,
		suggestAToken
	}
	benchState currentBenchState;

	public void InitialitializeBench(PlayerVO.PlayerType plt)
	{
		currentPlayerType = plt;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			benchedTokens[i].GetComponent<Token>().currentTokenType = (currentPlayerType == PlayerVO.PlayerType.friend) ? Token.TokenType.benchFriendly : Token.TokenType.benchEnemy;
			benchedTokens[i].GetComponent<Token>().SetTokenId(i);
		}
		UpdateTokenBenchDisplay (benchState.disabled);
	}

	public void SetTokenAsUsed(int tokenId)
	{
		benchedTokens [tokenId].GetComponent<Token> ().isTokenOnBoard = true;
		benchedTokens [tokenId].GetComponent<Token> ().currentTokenState = Token.TokenState.hideToken;
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

	public int numberOfTokensOnBoard()
	{
		int numOfTokensOnBoard = 0;
		for(int i=0; i<benchedTokens.Count; i++)
		{
			if(benchedTokens[i].GetComponent<Token>().isTokenOnBoard)
			{
				numOfTokensOnBoard ++;
			}
		}
		return numOfTokensOnBoard;
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
//				if(benchedTokens[i].GetComponent<Token>().currentTokenState == Token.TokenState.disabled)
//				{
//					benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.disabled;
//				}
//				else if(
//
//				else
//				{
//					benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.unselected;
//				}
			}
			benchedTokens[i].GetComponent<Token>().UpdateState();
		}
	}

	public void UpdateTokenBenchDisplay(benchState bs)
	{
		currentBenchState = bs;
		switch(currentBenchState)
		{
		case benchState.suggestAToken:
			//select next token to play // highlight next playable token.
			bool aTokenHasBeenSuggested = false;
			for(int i=0; i<benchedTokens.Count; i++)
			{
				if(!benchedTokens[i].GetComponent<Token>().isTokenOnBoard && !aTokenHasBeenSuggested)
				{
					benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.highlighted;
					aTokenHasBeenSuggested = true;
				}
				else
				{
					if(benchedTokens[i].GetComponent<Token>().isTokenOnBoard)
					{
						benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.hideToken;
					}
					else
					{
						benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.disabled;
					}
				}
				benchedTokens[i].GetComponent<Token>().UpdateState();
			}
			break;
		case benchState.disabled:
			//diable all tokens
			for(int i=0; i<benchedTokens.Count; i++)
			{
				if(benchedTokens[i].GetComponent<Token>().isTokenOnBoard)
				{
					benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.hideToken;
				}
				else
				{
					benchedTokens[i].GetComponent<Token>().currentTokenState = Token.TokenState.disabled;
				}
				benchedTokens[i].GetComponent<Token>().UpdateState();
			}
			break;
		}
	}
}
