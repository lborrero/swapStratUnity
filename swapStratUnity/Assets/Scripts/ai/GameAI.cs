using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameAI : MonoBehaviour {

	SwapBoard sb;
	List<string> Memory = new List<string>();

	// Use this for initialization
	void Start () {
	
	}

	void Update()
	{
		PercieveLoop ();
	}

	void PercieveLoop()
	{
		if (sGameManager.Instance.currentInnerGameLoop == sGameManager.InnerGameLoop.playerTwoTurn) 
		{
			ThinkLoop ("asdf");
		}
	}

	void ThinkLoop(string theGameObjectState)
	{
		switch (sb.sgm.currentTurnLoop) 
		{
		case sb.sgm.TurnLoop.selectATokenFromBench:
			System.Random rnd = new System.Random ();
			rnd.Next (0, sb.sbm.player2.benchedTokens - 1);
			int tokensLeft = sb.sbm.player2.playerTokenBench.benchedTokens.Count;
			for (int i = 0; i < tokensLeft; i++) 
			{
				Token tk = sb.sbm.player2.playerTokenBench.benchedTokens [i] as Token;
				if (tk.isTokenOnBoard) 
				{
					
				}
//				if(isTokenUsed(sBoardManager.Instance.player2.playerTokenBench.benchedTokens[i].token.Id
			}
			sBoardManager.Instance.TokenClicked ();
			break;
		case sGameManager.TurnLoop.placeSelectedTokenFromBench:
			break;
		case sGameManager.TurnLoop.selectATokenFromBoard:
			break;
		case sGameManager.TurnLoop.moveSelectedToken:
			break;
		case sGameManager.TurnLoop.endLoopTurn:
			break;
		}
	}
}
