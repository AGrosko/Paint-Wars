using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class StartScript : MonoBehaviourPunCallbacks
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnClickStart()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        print("ClickStart");

    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("Connected");
        SceneManager.LoadScene("LobbyScene");
    }
}
