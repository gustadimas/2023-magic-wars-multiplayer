using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class codeHealth : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] Image fillImage;
    [SerializeField] float healthAmount;
    [SerializeField] codePlayer plMove;
    [SerializeField] GameObject playerCanvas;

    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;

    codeGame gameManager;

    private int respawnCounter = 0;

    void Awake()
    {
        if (photonView.IsMine)
            codeGame.instance.localPlayer = this.gameObject;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        sr = GetComponent <SpriteRenderer>();
        gameManager = FindObjectOfType<codeGame>();
    }

    [PunRPC]
    public void ReduceHealth(float amount)
    {
        ModifyHealth(amount);
    }

    void ModifyHealth(float amount)
    {
        if (photonView.IsMine)
        {
            healthAmount -= amount;
            fillImage.fillAmount -= amount;

            // Sincroniza a saúde entre os jogadores
            photonView.RPC("SyncHealth", RpcTarget.OthersBuffered, healthAmount);
        }
        else
        {
            healthAmount -= amount;
            fillImage.fillAmount -= amount;
        }

        CheckHealth();
    }

    [PunRPC]
    private void SyncHealth(float syncHealth)
    {
        // Sincroniza a saúde recebida dos outros jogadores
        healthAmount = syncHealth;
        fillImage.fillAmount = healthAmount / 30f;
        CheckHealth();
    }

    void CheckHealth()
    {
        fillImage.fillAmount = healthAmount / 30f;

        if (photonView.IsMine && healthAmount <= 0)
        {
            codeGame.instance.EnableRespawn();
            plMove.disableInputs = true;
            this.GetComponent<PhotonView>().RPC("Dead", RpcTarget.AllBuffered);

            gameManager.UpdateDeathsInLocalPlayer(PhotonNetwork.LocalPlayer.ActorNumber);

            // // Incrementa o contador de respawns
            // IncrementRespawnCounter();
            // // Sincroniza o contador de respawns entre os jogadores
            // photonView.RPC("SyncRespawnCounter", RpcTarget.OthersBuffered, respawnCounter);
        }
    }

    [PunRPC]
    private void Dead()
    {
        rb.gravityScale = 0;
        bc.enabled = false;
        sr.enabled = false;

        playerCanvas.SetActive(false);
    }

    [PunRPC]
    private void Respawn()
    {
        rb.gravityScale = 5;
        bc.enabled = true;
        sr.enabled = true;
        fillImage.fillAmount = 1f;
        healthAmount = 30f;
        plMove.disableInputs = false;

        playerCanvas.SetActive(true);
    }

    [PunRPC]
    private void SyncRespawnCounter(int syncCounter)
    {
        // Sincroniza o contador de respawns recebido dos outros jogadores
        respawnCounter = syncCounter;
    }

    public void IncrementRespawnCounter()
    {
        respawnCounter++;
    }

    public int GetRespawnCounter()
    {
        return respawnCounter;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            // Envia o número de respawns para os outros jogadores
            stream.SendNext(respawnCounter);
        else
            // Recebe o número de respawns dos outros jogadores
            respawnCounter = (int)stream.ReceiveNext();
    }
}