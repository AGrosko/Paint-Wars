using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Text;
using ExitGames.Client.Photon;

public class RoomSceneManager : MonoBehaviourPunCallbacks
{

    [SerializeField]
    public TextMeshProUGUI roomNameText;

    [SerializeField]
    private TextMeshProUGUI textTeamAList;

    [SerializeField]
    private TextMeshProUGUI textTeamBList;

    [SerializeField]
    private Button buttonStartGame;

    [SerializeField]
    public TextMeshProUGUI playerCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // If for some reason we are not in a room, go back to the lobby.
        if (PhotonNetwork.CurrentRoom == null)
        {
            SceneManager.LoadScene("LobbyScene");
            return; // THIS IS THE FIX. It stops the function here.
        }

        UpdatePlayerList();

        //Update UI to Reflect room name
        roomNameText.text = "Welcome to Room: " + PhotonNetwork.CurrentRoom.Name;
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    // This is called by Photon every time a player enters or leaves,
    // or when the Master Client changes.
    private void UpdatePlayerList()
    {
        playerCount.text = "Players: " + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

        SplitTeams();

        // Only the player who is the "Master Client" can start the game.
        if (buttonStartGame != null)
        {
            buttonStartGame.interactable = PhotonNetwork.IsMasterClient;
        }
    }

    //Will organize player evenly into teams
    private void SplitTeams()
    {
        bool isATeam = true;
        int teamCount = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if ( isATeam)
            {
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Team", "A" } });
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TeamNumber", teamCount } });
                isATeam = false;

            }
            else
            {
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Team", "B" } });
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "TeamNumber", teamCount } });
                isATeam = true;
                teamCount++;

            }
            
        }

    }

    // Gets player team value.
    private string GetPlayerTeam(Player player)
    {
        object team;
        if (player.CustomProperties.TryGetValue("Team", out team))
        {

            return team as string;
        }
        return "None";
    }

    //Updates team lists
    private void UpdateTeamList()
    {
        StringBuilder sbA = new StringBuilder();
        StringBuilder sbB = new StringBuilder();
        string playerTeam = null;

        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            playerTeam = GetPlayerTeam(player);
            Debug.Log("Player " + player.NickName + " is on Team " + playerTeam);
            if (playerTeam == "A")
            {
                sbA.AppendLine("- " + player.NickName);
            }
            else
            {
                sbB.AppendLine("- " + player.NickName);
            }

        }

        textTeamAList.text = sbA.ToString();
        textTeamBList.text = sbB.ToString();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        UpdateTeamList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdatePlayerList();
    }

    // This is called by the "Start Game" button's OnClick event.
    public void OnClickStartGame()
    {
        PhotonNetwork.LoadLevel("GameScene");
    }

    // This is called by the "Leave Room" button's OnClick event.
    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // This is automatically called by Photon after LeaveRoom() completes.
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
