using UnityEngine;
//using UnityEngine.Advertisements;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {

	sGameManager sgm;
	sBoardManager sbm;
	public GameObject StartScreen;
    public GameObject SearchingScreen;
    public GameObject EndScreen;
	public GameObject InfoScreen;
	public sGameManager.GeneralGameState defaultGameState;
	public Text winnerLabel;

	public GameAI gameAi;

	// Use this for initialization
	void Start () {
		//Advertisement.Initialize ("1010373");
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
       case sGameManager.GeneralGameState.searchingForOtherPlayer:
                SearchingScreen.SetActive(true);
                break;
        case sGameManager.GeneralGameState.gameMode:
				switch((PlayerVO.PlayerType)gameAi.aiPt)
				{
				case PlayerVO.PlayerType.none:
//					Debug.Log("HumanVsHuman");
					break;
				case PlayerVO.PlayerType.enemy:
//					Debug.Log("BlueVsAi");
					break;
				case PlayerVO.PlayerType.friend:
//					Debug.Log("RedVsAi");
					break;
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
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
//							Debug.Log("HumanVsHuman");
							break;
						case PlayerVO.PlayerType.enemy:
//							Debug.Log("BlueVsAi");
							break;
						case PlayerVO.PlayerType.friend:
//							Debug.Log("RedVsAi");
							break;
						}
					winnerLabel.text = "Red Wins!";
				}
				else
				{
					switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
//							Debug.Log("HumanVsHuman");
							break;
						case PlayerVO.PlayerType.enemy:
//							Debug.Log("BlueVsAi");
							break;
						case PlayerVO.PlayerType.friend:
//							Debug.Log("RedVsAi");
							break;
						}
					winnerLabel.text = "Blue Wins!";
				}
			}
			else
			{
				if(sbm.player1.currentTurnPointCount > sbm.player2.currentTurnPointCount)
				{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
//							Debug.Log("HumanVsHuman");
							break;
						case PlayerVO.PlayerType.enemy:
//							Debug.Log("BlueVsAi");
							break;
						case PlayerVO.PlayerType.friend:
//							Debug.Log("RedVsAi");
							break;
						}
					winnerLabel.text = "Blue Wins!";
				}
				else if(sbm.player1.currentTurnPointCount < sbm.player2.currentTurnPointCount)
				{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
//							Debug.Log("HumanVsHuman");
							break;
						case PlayerVO.PlayerType.enemy:
//							Debug.Log("BlueVsAi");
							break;
						case PlayerVO.PlayerType.friend:
//							Debug.Log("RedVsAi");
							break;
						}

					winnerLabel.text = "Red Wins!";
				}
				else
				{
						switch((PlayerVO.PlayerType)gameAi.aiPt)
						{
						case PlayerVO.PlayerType.none:
//							Debug.Log("HumanVsHuman");
							break;
						case PlayerVO.PlayerType.enemy:
//							Debug.Log("BlueVsAi");
							break;
						case PlayerVO.PlayerType.friend:
//							Debug.Log("RedVsAi");
							break;
						}
					winnerLabel.text = "Tie Game";
				}
			}
			EndScreen.SetActive(true);
			break;
		}
	}

//	IEnumerator ShowAdWhenReady()
//	{
//		while (!Advertisement.IsReady())
//			yield return null;
//		
//		Advertisement.Show ();
//	}

	public void RateThisGame()
	{
		#if UNITY_ANDROID
		Application.OpenURL("market://details?id=YOUR_ID");
		#elif UNITY_IPHONE
		Application.OpenURL("itms-apps://itunes.apple.com/app/idYOUR_ID");
		#endif
	}

	public void ShowPortfolio()
	{
		Application.OpenURL("http://leonardobluz.com/");
	}

	public void QuiteGame()
	{
        Application.LoadLevel (Application.loadedLevel);
	}

	int counter = 0;
	public void ReloadScene()
	{
//		if(counter > 0)
//		{
//			StartCoroutine (ShowAdWhenReady ());
			Application.LoadLevel (Application.loadedLevel);
//		}
//		counter++;
	}
}

