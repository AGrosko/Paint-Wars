using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;

public class LobbySceneManager : MonoBehaviourPunCallbacks
{

    [SerializeField]
    public TMP_InputField inputRoomName;

    [SerializeField]
    public TextMeshProUGUI roomListText;

    [SerializeField]
    private TMP_InputField inputPlayerName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if (PhotonNetwork.IsConnected == false)
        {
            SceneManager.LoadScene("StartScene");
            return;
        }
        else
        {

            PhotonNetwork.JoinLobby();
        }

    }

    // Update is called once per frame
    public override void OnJoinedLobby()
    {
        print("Lobby Joined Successfully");
    }

    public string GetRoomName()
    {
        string roomName = inputRoomName.text;
        return roomName.Trim();
    }

    public string GetPlayerName()
    {
        string playerName = inputPlayerName.text;
        return playerName.Trim();
    }

    public void OnClickCreateRoom()
    {

        string playerName = GetPlayerName(); // A helper function to get name from input field
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("Player Name is invalid.");
            return;
        }

        // Set the NickName *before* creating the room
        PhotonNetwork.LocalPlayer.NickName = playerName;

        string roomName = GetRoomName();
        if (roomName.Length > 0)
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 6;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            Debug.Log("Room Name is Invalid");
        }

    }

    public void OnClickJoinRoom()
    {

        string playerName = GetPlayerName(); // A helper function to get name from input field
        if (string.IsNullOrEmpty(playerName))
        {
            Debug.LogError("Player Name is invalid.");
            return;
        }

        // Set the NickName *before* joining the room
        PhotonNetwork.LocalPlayer.NickName = playerName;


        string roomName = GetRoomName();
        if (roomName.Length > 0)
        {
            PhotonNetwork.JoinRoom(roomName);
        }
        else
        {
            Debug.Log("Room Name is Invalid");
        }
    }

    public override void OnJoinedRoom()
    {
        print("Room Joined Successfully");
        SceneManager.LoadScene("RoomScene");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        StringBuilder sb = new StringBuilder();
        foreach (RoomInfo room in roomList)
        {
            if (room.PlayerCount > 0)
            {
                sb.AppendLine($"RoomName: {room.Name}  PlayerCount: {room.PlayerCount}");
            }

            roomListText.text = sb.ToString();
        }
    }

}
