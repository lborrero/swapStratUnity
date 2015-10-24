using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	sGameManager sgm;
	sBoardManager sbm;
	public GameObject StartScreen;
	public GameObject EndScreen;
	public sGameManager.GeneralGameState defaultGameState;
	public Text winnerLabel;

	// Use this for initialization
	void Start () {
		sgm = sGameManager.Instance;
		sbm = sBoardManager.Instance;
		UpdateGameState ((int)defaultGameState);
	}

	public void UpdateGameState(int gameState)
	{
		sgm.currentGeneralGameState = (sGameManager.GeneralGameState)gameState;
		switch(sgm.currentGeneralGameState)
		{
		case sGameManager.GeneralGameState.startScreen:
			ShowAd();
			StartScreen.SetActive(true);
			EndScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.gameMode:
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.endScreen:
			StartScreen.SetActive(false);
			if(sbm.player1.currentTurnPointCount > sbm.player2.currentTurnPointCount)
			{
				winnerLabel.text = "Blue Wins!";
			}
			else if(sbm.player1.currentTurnPointCount < sbm.player2.currentTurnPointCount)
			{
				winnerLabel.text = "Red Wins!";
			}
			else
			{
				winnerLabel.text = "Tie Game";
			}
			EndScreen.SetActive(true);
			break;
		}
	}

	public void ShowAd()
	{
		if (Advertisement.IsReady())
		{
			Advertisement.Show();
		}
	}
}

