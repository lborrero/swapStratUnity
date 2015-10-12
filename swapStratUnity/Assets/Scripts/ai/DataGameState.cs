using UnityEngine;
using System.Collections;

public class DataGameState {

	public DataGameState(int _f, int _e, string gameState)
	{
		scoreFriend = _f;
		scoreEnemy = _e;
		data = gameState;
	}

	public string data;
	public int scoreFriend;
	public int scoreEnemy;
	public string friendBenchTokens;
	public string enemyBenchTokens;
	public bool endGame;
	public int getScore(PlayerVO.PlayerType pt)
	{
		int returnValue;
		if(pt == PlayerVO.PlayerType.friend)
		{
			returnValue = scoreFriend - scoreEnemy;
		}
		else
		{
			returnValue = scoreEnemy - scoreFriend;
		}
		return returnValue;
	}
}
