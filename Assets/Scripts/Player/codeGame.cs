using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.PunBasics;
using System.Collections.Generic;

public class codeGame : MonoBehaviourPunCallbacks
{
    [HideInInspector] public static codeGame instance;

    [Header("Interface:")]
    [SerializeField] TMP_Text ping;

    [Header("Objetos:")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject gameCanvas;
    [SerializeField] GameObject sceneCamera;
    [SerializeField] GameObject disconnectInterface;
    [SerializeField] GameObject feedPlayer;
    [SerializeField] GameObject feedGrid;

    [Header("Controladores:")]
    //[SerializeField] bool disconnect = false;

    [Header("Renascimento:")]
    [HideInInspector] public GameObject localPlayer;
    [SerializeField] TMP_Text respawnText;
    [SerializeField] GameObject respawnMenu;
    [SerializeField] float timerAmount = 5f;
    [SerializeField] bool spawnTimer = false;

    Dictionary<int, int> playersDeaths = new Dictionary<int, int>();

    void Awake()
    {
        instance = this;
        gameCanvas.SetActive(true);
    }

    void Update()
    {
        //CheckInput();
        ping.text = "Ping: " + PhotonNetwork.GetPing();

        if (spawnTimer)
            StartRespawn();
    }

    public void EnableRespawn()
    {
        timerAmount = 5f;
        spawnTimer = true;
        respawnMenu.SetActive(true);
    }

    void StartRespawn()
    {
        timerAmount -= Time.deltaTime;
        respawnText.text = "Renascendo em... " + timerAmount.ToString("F0");

        if (timerAmount <= 0)
        {
            localPlayer.GetComponent<PhotonView>().RPC("Respawn", RpcTarget.AllBuffered);
            RespawnLocation();
            respawnMenu.SetActive(false);
            spawnTimer = false;                        
        }
    }
    
    public bool VerifyGameResult(int playerPhotonID)
    {
        // Percorre o dicionário de mortes para ver qual jogador morreu mais vezes
        int playerID = 0;
        int playerDeaths = 0;

        foreach(KeyValuePair<int, int> player in playersDeaths)
        {
            if(player.Value > playerDeaths)
            {
                playerID = player.Key;
                playerDeaths = player.Value;
            }
        }

        if (playerID == playerPhotonID) 
            return false;
        else 
            return true;
        
    }

    public void RespawnLocation()
    {
        float randomValue = Random.Range(-3f, 5f);
        localPlayer.transform.localPosition = new Vector2(randomValue, 3f);
    }

    public void UpdateDeathsInLocalPlayer(int playerPhotonID)
    {
        if (playersDeaths.ContainsKey(playerPhotonID))
            playersDeaths[playerPhotonID]++;
         else 
            playersDeaths.Add(playerPhotonID, 1);

        print("[LOCAL] Jogador " + playerPhotonID + " morreu " + playersDeaths[playerPhotonID] + " vezes");

        photonView.RPC("UpdatePlayerDeathRPC", RpcTarget.AllBuffered, playerPhotonID, playersDeaths[playerPhotonID]);
    }

    [PunRPC]
    void UpdatePlayerDeathRPC(int playerPhotonID, int deathAmount)
    {
        if (!playersDeaths.ContainsKey(playerPhotonID))
            playersDeaths.Add(playerPhotonID, 1);
        else
            playersDeaths[playerPhotonID] = deathAmount;

        print("[RPC] Jogador " + playerPhotonID + " morreu " + playersDeaths[playerPhotonID] + " vezes");
    }

    //void CheckInput()
    //{
    //    if(disconnect && Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        disconnectInterface.SetActive(false);
    //        disconnect = false;
    //    }
    //    else if (!disconnect && Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        disconnectInterface.SetActive(true);
    //        disconnect = true;
    //    }
    //}

    public void SpawnPlayer()
    {
        float randomValue = Random.Range(-1f, 1f);

        PhotonNetwork.Instantiate(playerPrefab.name, new Vector2(this.transform.position.x * randomValue,
            this.transform.position.y), Quaternion.identity, 0);

        gameCanvas.SetActive(false);
        sceneCamera.SetActive(true);

        GetComponent<Temporizador>()?.StartTemporizador();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("sceneLobby");
    }

    public override void OnPlayerEnteredRoom(Player player)
    {
        GameObject obj = Instantiate(feedPlayer, new Vector2(0, 0), Quaternion.identity);
        obj.transform.SetParent(feedGrid.transform, false);
        obj.GetComponent<TMP_Text>().text = player.NickName + " entered the game!";
        obj.GetComponent<TMP_Text>().color = Color.red;
    }

    public override void OnPlayerLeftRoom(Player player)
    {
        GameObject obj = Instantiate(feedPlayer, new Vector2(0, 0), Quaternion.identity);
        obj.transform.SetParent(feedGrid.transform, false);
        obj.GetComponent<TMP_Text>().text = player.NickName + " left the game!";
        obj.GetComponent<TMP_Text>().color = Color.yellow;
    }
}