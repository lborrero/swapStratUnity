
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// https://medium.com/@div5yesh/turn-based-multiplayer-games-in-unity3d-using-unet-abcd8360ddd5
/// </summary>

public class NetworkManager : UnityEngine.Networking.NetworkManager
{
    public event Action<bool, MatchInfo> matchCreated;

    public event Action<bool, MatchInfo> matchJoined;

    private Action<bool, MatchInfo> NextMatchCreatedCallback;

    public List<NetworkPlayer> players;

    public static NetworkManager Instance;

    public SwapBoard boardView;

    public int connectedPlayerOrder = 0;
    public Text onlineORderLabel;

    int iActivePlayer = 0;

    int currentActivePlayer = 0;

    public int ActivePlayer
    {
        get
        {
            Debug.Log("ActivePlayer");
            return currentActivePlayer;
        }
    }

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
    }
    // Use this for initialization
    void Start()
    {
        players = new List<NetworkPlayer>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Use for button
    public void StartFakeGame()
    {
        CreateOrJoin("fakeMatch", null);
    }

    public void fakeMatchCreated()
    {
        Debug.Log("CreateOrJoin"); 
    }

    // Update is called once per frame
    void Update()
    {
        if (players.Count > 0)
        {
            CheckPlayersReady();
        }
    }

    bool CheckPlayersReady()
    {
        //Debug.Log("CheckPlayersReady");
        bool playersReady = true;
        foreach (var player in players)
        {
            playersReady &= player.ready;
        }

        if (playersReady)
        {
            //Debug.Log("CheckPlayersReady + playersReady");
            players[iActivePlayer].StartGame();
        }

        return playersReady;
    }

    public void ReTurn()
    {
        Debug.Log("turn::" + iActivePlayer);
        players[iActivePlayer].TurnStart();
    }

    public void AlterTurns()
    {
        Debug.Log("turn::" + iActivePlayer);

        players[iActivePlayer].TurnEnd();
        currentActivePlayer = (currentActivePlayer + 1) % 2;
        iActivePlayer = (iActivePlayer + 1) % 1; //players.Count;
        players[iActivePlayer].TurnStart();
    }

    public void UpdateScore(int score)
    {
        players[ActivePlayer].UpdateScore(score);
    }

    public void RegisterNetworkPlayer(NetworkPlayer player)
    {
        Debug.Log("RegisterNetworkPlayer");
        if (players.Count <= 2)
        {
            Debug.Log("RegisterNetworkPlayer + count");
            players.Add(player);
        }
    }

    public void DeregisterNetworkPlayer(NetworkPlayer player)
    {
        Debug.Log("DeregisterNetworkPlayer");
        players.Remove(player);
    }

    public void CreateOrJoin(string gameName, Action<bool, MatchInfo> onCreate)
    {
        Debug.Log("CreateOrJoin");
        StartMatchMaker();
        NextMatchCreatedCallback = onCreate;
        matchMaker.ListMatches(0, 10, "turnbasedgame", true, 0, 0, OnMatchList);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("OnSceneLoaded");
        if (scene.name == "GameScene")
        {
            NetworkServer.SpawnObjects();
        }
    }

    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matches)
    {
        Debug.Log("Matches:" + matches.Count);
        if (success && matches.Count > 0)
        {
            connectedPlayerOrder = 1;
            Debug.Log("Matches + A");
            matchMaker.JoinMatch(matches[0].networkId, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchJoined);
        }
        else
        {
            connectedPlayerOrder = 0;
            Debug.Log("Matches + B");
            CreateMatch("turnbasedgame");
        }
        onlineORderLabel.text = "" + connectedPlayerOrder;
    }

    public void CreateMatch(string matchName)
    {
        Debug.Log("CreateMatch");
        matchMaker.CreateMatch(matchName, 2, true, string.Empty, string.Empty, string.Empty, 0, 0, OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchCreate" + matchInfo.networkId);

        // Fire callback
        if (NextMatchCreatedCallback != null)
        {
            NextMatchCreatedCallback(success, matchInfo);
            NextMatchCreatedCallback = null;
        }

        // Fire event
        if (matchCreated != null)
        {
            matchCreated(success, matchInfo);
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        Debug.Log("OnMatchJoined" + matchInfo.networkId);

        // Fire callback
        if (NextMatchCreatedCallback != null)
        {
            NextMatchCreatedCallback(success, matchInfo);
            NextMatchCreatedCallback = null;
        }

        // Fire event
        if (matchJoined != null)
        {
            matchJoined(success, matchInfo);
        }
    }


    // match making callbacks:
    /*
    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnMatchCreate Success:{0}, ExtendedInfo:{1}, matchInfo:{2}", success, extendedInfo, matchInfo); }
        if (success)
            StartHost(matchInfo);
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnMatchJoined Success:{0}, ExtendedInfo:{1}, matchInfo:{2}", success, extendedInfo, matchInfo); }
        if (success)
            StartClient(matchInfo);
    }


    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnMatchList Success:{0}, ExtendedInfo:{1}, matchList.Count:{2}", success, extendedInfo, matchList.Count); }
        matches = matchList;
    }
    /**/

    public override void OnDestroyMatch(bool success, string extendedInfo)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnDestroyMatch Success:{0}, ExtendedInfo:{1}", success, extendedInfo); }
    }

    public override void OnDropConnection(bool success, string extendedInfo)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnDestroyMatch Success:{0}, ExtendedInfo:{1}", success, extendedInfo); }
    }

    public override void OnSetMatchAttributes(bool success, string extendedInfo)
    {
        if (LogFilter.logDebug) { Debug.LogFormat("NetworkManager OnDestroyMatch Success:{0}, ExtendedInfo:{1}", success, extendedInfo); }
    }
}