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

	public GameAI gameAi;

	public GoogleAnalyticsV3 googleAnalytics;
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
			if (googleAnalytics != null)
			{
				googleAnalytics.LogScreen("startScreen");
			}
			StartScreen.SetActive(true);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.infoScreen:
			if (googleAnalytics != null)
			{
				googleAnalytics.LogScreen("infoScreen");
			}
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(true);
			break;
		case sGameManager.GeneralGameState.gameMode:
			if(GoogleAnalyticsV3.instance)
			{
				switch((PlayerVO.PlayerType)gameAi.aiPt)
				{
				case PlayerVO.PlayerType.none:
					Debug.Log("HumanVsHuman");
					GoogleAnalyticsV3.instance.LogScreen("gameMode-HumanVsHuman");
					break;
				case PlayerVO.PlayerType.enemy:
					Debug.Log("BlueVsAi");
					GoogleAnalyticsV3.instance.LogScreen("gameMode-BlueVsAi");
					break;
				case PlayerVO.PlayerType.friend:
					Debug.Log("RedVsAi");
					GoogleAnalyticsV3.instance.LogScreen("gameMode-RedVsAi");
					break;
				}
			}
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			InfoScreen.SetActive(false);
			break;
		case sGameManager.GeneralGameState.endScreenWithPoints:
			StartScreen.SetActive(false);
			InfoScreen.SetActive(false);
			if(sbm.currentPlayerTurn.currentTurnMoveCount == sbm.currentPlayerTurn.currentTurnMoveLimit &&
			   sbm.currentPlayerTurn.currentTurnMoveLimit != 0)
			{
				if(sbm.currentPlayerTurn.currentPlayerType == PlayerVO.PlayerType.friend)
				{
					if(GoogleAnalyticsV3.instance)
					{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
							Debug.Log("HumanVsHuman");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-HvH-red");
							break;
						case PlayerVO.PlayerType.enemy:
							Debug.Log("BlueVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-BlueVsAi-PlayerLoses");
							break;
						case PlayerVO.PlayerType.friend:
							Debug.Log("RedVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-RedVsAi-PlayerWins");
							break;
						}
					}
					winnerLabel.text = "Red Wins!";
				}
				else
				{
					if(GoogleAnalyticsV3.instance)
					{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
							Debug.Log("HumanVsHuman");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-HvH-blue");
							break;
						case PlayerVO.PlayerType.enemy:
							Debug.Log("BlueVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-BlueVsAi-PlayerWins");
							break;
						case PlayerVO.PlayerType.friend:
							Debug.Log("RedVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-RedVsAi-PlayerLoses");
							break;
						}
					}
					winnerLabel.text = "Blue Wins!";
				}
			}
			else
			{
				if(sbm.player1.currentTurnPointCount > sbm.player2.currentTurnPointCount)
				{
					if(GoogleAnalyticsV3.instance)
					{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
							Debug.Log("HumanVsHuman");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-HvH-blue");
							break;
						case PlayerVO.PlayerType.enemy:
							Debug.Log("BlueVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-BlueVsAi-PlayerWins");
							break;
						case PlayerVO.PlayerType.friend:
							Debug.Log("RedVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-RedVsAi-PlayerLoses");
							break;
						}
					}
					winnerLabel.text = "Blue Wins!";
				}
				else if(sbm.player1.currentTurnPointCount < sbm.player2.currentTurnPointCount)
				{
					if(GoogleAnalyticsV3.instance)
					{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
							Debug.Log("HumanVsHuman");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-HvH-red");
							break;
						case PlayerVO.PlayerType.enemy:
							Debug.Log("BlueVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-BlueVsAi-PlayerLoses");
							break;
						case PlayerVO.PlayerType.friend:
							Debug.Log("RedVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-RedVsAi-PlayerWins");
							break;
						}
					}
					winnerLabel.text = "Red Wins!";
				}
				else
				{
					if(GoogleAnalyticsV3.instance)
					{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
							Debug.Log("HumanVsHuman");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-HvH-tie");
							break;
						case PlayerVO.PlayerType.enemy:
							Debug.Log("BlueVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-BlueVsAi-tie");
							break;
						case PlayerVO.PlayerType.friend:
							Debug.Log("RedVsAi");
							GoogleAnalyticsV3.instance.LogScreen("endScreen-RedVsAi-tie");
							break;
						}
					}
					winnerLabel.text = "Tie Game";
				}
			}
			EndScreen.SetActive(true);
			break;
		}
	}

	public void ShowAd()
	{
		if (Advertisement.IsReady())
		{
			if (googleAnalytics != null)
			{
				googleAnalytics.LogScreen("ads");
			}
			Advertisement.Show();
		}
	}

	public void ShowPortfolio()
	{
		if (googleAnalytics != null)
		{
			googleAnalytics.LogScreen("portfolio");
		}
		Application.OpenURL("http://leonardobluz.com/");
	}
}

