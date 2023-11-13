using System.Collections;
using UnityEngine;
using Photon.Pun;

public class codeBullet : MonoBehaviourPun
{
    [SerializeField] bool moveDir = false;
    [SerializeField] float moveSpeed, destroyTime, bulletDamage;

    void Awake()
    {
        StartCoroutine("DestroyByTime");
    }

    IEnumerator DestroyByTime()
    {
        yield return new WaitForSeconds(destroyTime);
        this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ChangeDir_Left()
    {
        moveDir = true;
    }

    [PunRPC]
    void DestroyObject()
    {
        Destroy(this.gameObject);
    }

    void Update()
    {
        if (!moveDir)
            transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
        else
            transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D cl)
    {
        if (!photonView.IsMine)
            return;

        PhotonView target = cl.gameObject.GetComponent<PhotonView>();

        if(target != null && (!target.IsMine || target.IsRoomView))
        {
            if (target.tag == "Player")
                target.RPC("ReduceHealth", RpcTarget.AllBuffered, bulletDamage);
            
            this.GetComponent<PhotonView>().RPC("DestroyObject", RpcTarget.AllBuffered);
        }
    }
}
