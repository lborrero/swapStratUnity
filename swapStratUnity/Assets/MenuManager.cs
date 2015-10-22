using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class MenuManager : MonoBehaviour {

	sGameManager sgm;
	sBoardManager sbm;
	public GameObject StartScreen;
	public GameObject EndScreen;
	public sGameManager.GeneralGameState defaultGameState;
	public Text winnerLabel;
	BannerView bannerView;

	// Use this for initialization
	void Start () {
		sgm = sGameManager.Instance;
		sbm = sBoardManager.Instance;
		UpdateGameState ((int)defaultGameState);
		RequestBanner ();
	}

	public void UpdateGameState(int gameState)
	{
		sgm.currentGeneralGameState = (sGameManager.GeneralGameState)gameState;
		switch(sgm.currentGeneralGameState)
		{
		case sGameManager.GeneralGameState.startScreen:
			StartScreen.SetActive(true);
			EndScreen.SetActive(false);
			bannerView.Show();
			break;
		case sGameManager.GeneralGameState.gameMode:
			StartScreen.SetActive(false);
			EndScreen.SetActive(false);
			bannerView.Hide();
			break;
		case sGameManager.GeneralGameState.endScreen:
			StartScreen.SetActive(false);
			bannerView.Show();
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

	private void RequestBanner()
	{
		#if UNITY_ANDROID
		string adUnitId = "INSERT_ANDROID_BANNER_AD_UNIT_ID_HERE";
		#elif UNITY_IPHONE
		string adUnitId = "INSERT_IOS_BANNER_AD_UNIT_ID_HERE";
		#else
		string adUnitId = "unexpected_platform";
		#endif
		
		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder()
			.AddTestDevice(AdRequest.TestDeviceSimulator)       // Simulator.
			.AddTestDevice("0BC20B01631B96FBFB903107F1CFE7B4")  // My test device.
			.Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
	}
}
