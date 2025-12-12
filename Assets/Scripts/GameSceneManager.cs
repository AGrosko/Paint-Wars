using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Photon.Realtime;


public class GameSceneManager : MonoBehaviourPunCallbacks
{
    // FIX #3: Initialized the list to prevent null reference errors.
    [SerializeField]
    List<string> messageList = new List<string>();

    [SerializeField]
    TextMeshProUGUI messageText;

    [SerializeField]
    TextMeshProUGUI PlayerCount;

    [SerializeField]
    TextMeshProUGUI GameTimerText;

    public double timerLength = 120;

    private double startTime;
    private bool timerRunning = false;

    private PhotonView _pv;
    
    [SerializeField]
    private TrailManager trailManager;

    public Dictionary<Player, bool> alivePlayerMap = new Dictionary<Player, bool>();

    public Transform[] TeamAspawnPoints;
    public Transform[] TeamBspawnPoints;

    void Start()
    {
        _pv = this.gameObject.GetComponent<PhotonView>();

        if (PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("LobbyScene");
            return;
        }
        else
        {
            //InitGame();
            StartCoroutine(DelayInit(0.5f));
        }


    }


    void Update()
    {
        if (!timerRunning) return;

        double elapsed = PhotonNetwork.Time - startTime;
        double remaining = timerLength - elapsed;

        if (remaining > 0)
        {
            GameTimerText.text = Mathf.CeilToInt((float)remaining).ToString();
        }
        else
        {
            GameTimerText.text = "0";
            timerRunning = false;
            OnTimerEnd();
        }
    }

    void OnTimerEnd()
    {
        Debug.Log("Timer Ended");

        var (TeamAScore, TeamBScore) = trailManager.GetScores();

        string message = $"Time's up! \nBlue Team Score: {TeamAScore} \nRed Team Score: {TeamBScore}";

        CallRpcSendMessageToAll(message);

        WaitForSeconds wait = new WaitForSeconds(7f);

        SceneManager.LoadScene("RoomScene");
    }


    IEnumerator DelayInit(float sec)
    {
        yield return new WaitForSeconds(sec);
        InitGame();
    }

    public void InitGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            photonView.RPC("StartTimer", RpcTarget.AllBuffered, startTime);
        }


        alivePlayerMap.Clear(); // Clear old data for a new round
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            alivePlayerMap[player] = true;
        }

        PlayerCount.text = "Players: " + alivePlayerMap.Count;


        float spawnPointX = Random.Range(-5, 5);
        float spawnPointY = 1;
        PhotonNetwork.Instantiate("Player", new Vector3(spawnPointX, spawnPointY, 0), Quaternion.identity);

 
    }

    [PunRPC]
    void StartTimer(double networkStartTime)
    {
        startTime = networkStartTime;
        timerRunning = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        alivePlayerMap[newPlayer] = true;
        PlayerCount.text = "Players: " + alivePlayerMap.Count;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // FIX #5: Cleaned up the logic for when a player leaves.
        if (alivePlayerMap.ContainsKey(otherPlayer))
        {
            alivePlayerMap.Remove(otherPlayer);
            PlayerCount.text = "Players: " + alivePlayerMap.Count;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1)
        {
            PhotonNetwork.LeaveRoom();
        }

        // The Master Client should check if the game is now over.
        if (PhotonNetwork.IsMasterClient)
        {
            //CheckGameOver();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    // This is called from the PlayerController when that player dies.
    public void CallRpcLocalPlayerDead()
    {
        Debug.Log("A player has died. Notifying all clients.");
        _pv.RPC("RpcLocalPlayerDead", RpcTarget.All);
    }

    [PunRPC]
    void RpcLocalPlayerDead(PhotonMessageInfo info)
    {
        // Use info.Sender to safely identify which player sent this "I'm dead" message.
        if (alivePlayerMap.ContainsKey(info.Sender))
        {
            alivePlayerMap[info.Sender] = false;
        }

        // Only the Master Client decides if the game is over.
        if (PhotonNetwork.IsMasterClient)
        {
            //CheckGameOver();
        }
    }

  /*  bool CheckGameOver()
    {
        int aliveCount = 0;
        foreach (bool isAlive in alivePlayerMap.Values)
        {
            if (isAlive) aliveCount++;
        }

        if (aliveCount <= 1)
        {
            Debug.Log("Game Over! Loading next level for everyone.");
            // FIX #2: Use PhotonNetwork.LoadLevel to sync scene changes for ALL players.
            //PhotonNetwork.LoadLevel("GameScene2"); // Assuming you have a "GameScene2"
            CallRpcReloadGame();
            return true;
        }




        return false;
    } */

    // --- Chat Message Functions ---
    public void CallRpcSendMessageToAll(string message)
    {
        _pv.RPC("RpcSendMessage", RpcTarget.All, message);
    }

    [PunRPC]
    void RpcSendMessage(string message, PhotonMessageInfo info)
    {
        if (messageList.Count >= 10)
        {
            messageList.RemoveAt(0);
        }

        // Add the sender's name to the message for clarity.
        messageList.Add($"[{info.Sender.NickName}]: {message}");
        UpdateMessage();
    }

    void UpdateMessage()
    {
        if (messageText != null)
        {
            messageText.text = string.Join("\n", messageList);
        }
    }


    void CallRpcReloadGame()
    {
        _pv.RPC("ReloadGame", RpcTarget.All);

    }
    [PunRPC]
    void ReloadGame(PhotonMessageInfo info)
    {
        //SceneManager.LoadScene("GameScene");
        PhotonNetwork.LoadLevel("GameScene");
    }
}