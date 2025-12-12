using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    private float timer;
    private PhotonView pv;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 1;
        pv = this.gameObject.GetComponent<PhotonView>();
        _rb = this.gameObject.GetComponent<Rigidbody>();

        if (!pv.IsMine)
        {
            Destroy(_rb);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(pv.IsMine)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                PhotonNetwork.Destroy(this.gameObject);
            }
        }
    }
}
