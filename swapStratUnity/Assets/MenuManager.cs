using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	sGameManager sgm;
	sBoardManager sbm;
	public GameObject StartScreen;
	public GameObject EndScreen;
	public GameObject InfoScreen;
	public sGameManager.GeneralGameState defaultGameState;
	public Text winnerLabel;

	// Use this for initialization
	void Start () {
		sgm = sGameManager.Instance;
		sbm = sBoardManager.Instance;
		UpdateGameState ((int)defaultGameState);
	}

	public void backToPreviousGameState()
	{
		UpdateGameState (sgm.previousGeneralGameState);
	}

	public void UpdateGameState(int gameState)
	{
		ChangeGameState ((sGameManager.GeneralGameState)gameState);
	}

	public void UpdateGameState(sGameManager.GeneralGameState gameState)
	{
		ChangeGameState (gameState);
	}

	public void ChangeGameState(sGameManager.GeneralGameState gameState)
	{
		sgm.previousGeneralGameState = sgm.currentGeneralGameState;
		sgm.currentGeneralGameState = gameState;
		switch(sgm.currentGeneralGameState)
		{
		case sGameManager.GeneralGameState.startScreen:
			StartScreen.SetActive(true);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.infoScreen:
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(true);
			break;
		case sGameManager.GeneralGameState.gameMode:
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.endScreen:
			StartScreen.SetActive(false);
			InfoScreen.SetActive(false);
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

	public void ShowPortfolio()
	{
		Application.OpenURL("http://leonardobluz.com/");
	}
}

