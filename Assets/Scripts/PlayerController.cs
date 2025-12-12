using Photon.Pun;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;
using System.Text;
using Image = UnityEngine.UI.Image;
using System.Collections.Generic;

public class PlayerController : MonoBehaviourPunCallbacks
{

    [SerializeField]
    List<string> messageList = new List<string>();

    [SerializeField]
    TextMeshProUGUI messageText;

    private Transform _transform;
    public PhotonView _pv;
    private Rigidbody _rb;

    GameSceneManager _gm;
    TrailManager _tm;

    public float bulletPower;

    private float lastMoveX;
    private float lastMoveZ;
    private int playerHealth;



    [SerializeField]
    private TextMeshProUGUI name_Text;
    

    //team resources
    
    public Camera TeamACam;
    public Camera TeamBCam;

    public Material teamAMaterial;
    public Material teamBMaterial;

    string team;
    int teamNumber;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = 5;
        _pv = this.gameObject.GetComponent<PhotonView>();
        _rb = this.gameObject.GetComponentInChildren<Rigidbody>();
        _gm = GameObject.Find("GameSceneManager").GetComponent<GameSceneManager>();
        _tm = GameObject.Find("TrailManager").GetComponent<TrailManager>();

        _transform = _rb.transform;

        //gathering team resources

        TeamACam = GameObject.Find("cameraA").GetComponent<Camera>();
        TeamBCam = GameObject.Find("cameraB").GetComponent<Camera>();

        teamAMaterial = Resources.Load<Material>("TeamAMat");
        teamBMaterial = Resources.Load<Material>("TeamBMat");

        //gets players team values

        if (_pv.Owner.CustomProperties.TryGetValue("Team", out object teamObj))
        {
            team = teamObj.ToString();
            
        }
        if (_pv.Owner.CustomProperties.TryGetValue("TeamNumber", out object numObj))
        {
            teamNumber = (int)numObj;
            
        }
        Debug.Log("Player Team: " + team + " Team Number: " + teamNumber);

        //finds spawn point based on team and moves player there
        //Enables correct camera and material based on team



            if (team == "A")
            {


                if (_pv.IsMine)
                {
                TeamACam.enabled = true;
                TeamBCam.enabled = false;
                 }

                GetComponentInChildren<Renderer>().material = teamAMaterial;

                _transform.position = _gm.TeamAspawnPoints[teamNumber].position;

            _tm.RegisterPlayer(this.gameObject, "A");

            Debug.Log("Player Spawned at Team A Spawn");
            }
            else if (team == "B")
            {


            if (_pv.IsMine)
            {
                TeamACam.enabled = false;
                TeamBCam.enabled = true;
            }

                GetComponentInChildren<Renderer>().material = teamBMaterial;

                _transform.position = _gm.TeamBspawnPoints[teamNumber].position;

            _tm.RegisterPlayer(this.gameObject, "B");
            Debug.Log("Player Spawned at Team B Spawn");
            }
            else
            {
                Debug.LogError("Player has no team assigned!");
            }
        





        bulletPower = 0.00005f;

        name_Text.text = _pv.Owner.NickName;
    }
    public float moveSpeed = 5f;
    // Update is called once per frame
    void Update()
    {
        
        
        if (_pv.IsMine) 
        {
            
            if (_transform.position.y < -5)
            {
                Debug.Log("Player dead");
                Dead(_pv.Owner.NickName, "Fall");
            }


            Control(); 
        
        }



    }

    public void CallRpcSendMessage(string message)
    {
        _pv.RPC("RPCSendMessage", RpcTarget.All, message);
    }

    [PunRPC]
    void RPCSendMessage(string message, PhotonMessageInfo info)
    {
        if (messageList.Count >= 10)
        {
            messageList.RemoveAt(0);
        }

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


    void Control()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Camera activeCam = team == "A" ? TeamACam : TeamBCam;

        // Build movement vector in camera space
        Vector3 camForward = activeCam.transform.forward;
        Vector3 camRight = activeCam.transform.right;

        // Flatten so you don't move up/down with camera tilt
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 movement = (camForward * moveZ + camRight * moveX).normalized;

        Vector3 newPosition = _rb.position + movement * moveSpeed * Time.deltaTime;
        _rb.MovePosition(newPosition);



        _rb.MovePosition(newPosition);



        //Added logic for bullet direction based on player movenment direction        
     


        

    }


    public void HandleCollision(Collision other)
    {
        Debug.Log("Collision detected in Player Controller");
        if ( _pv != null && _pv.IsMine)
        {
            if (other.gameObject.CompareTag("PaintBullet"))
            {
                PhotonView otherPv = other.gameObject.GetComponent<PhotonView>();

                if(!otherPv.IsMine)
                {
                    HashTable table = new HashTable();
                    playerHealth -= 1;

                    table.Add("Health", playerHealth);
                    PhotonNetwork.LocalPlayer.SetCustomProperties(table);
                    Debug.Log("Player hit! Health: " + playerHealth);



                    string attackerName = otherPv.Owner.NickName;
                    string myName = _pv.Owner.NickName;
                    _gm.CallRpcSendMessageToAll($"{attackerName} hit {myName}");

                    if (playerHealth <= 0)
                    {
                        Dead(myName, attackerName);
                    }
                }
            }
            
        }
    }

    public void Dead(string myName, string attackerName)
    {
        PhotonNetwork.Destroy(this.gameObject);
        _gm.CallRpcSendMessageToAll($"{attackerName} eliminated {myName}");
        _gm.CallRpcLocalPlayerDead();
        Debug.Log("Player dead");
    }

    public void UpdateHpBar()
    {
        float percent = (float)playerHealth / 5;
        //hp_image.transform.localScale = new Vector3(percent, hp_image.transform.localScale.y, hp_image.transform.localScale.z);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, HashTable changedProps)
    {
        if (targetPlayer == _pv.Owner)
        {

            playerHealth = (int)changedProps["Health"];
            print(targetPlayer.NickName + ":" + playerHealth.ToString());
            UpdateHpBar();
        }
    }
}
