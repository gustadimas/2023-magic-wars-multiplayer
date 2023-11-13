using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Photon.Pun;

public class Temporizador : MonoBehaviourPunCallbacks, IPunObservable
{
    public TextMeshProUGUI temporizadorTexto;
    public  int temporizadorDuracao = 180;
    double tempoInicialLocal;
    double tempoInicialRemoto;
    double temporizador;

    [SerializeField] GameObject resultWin, resultLose;

    codeGame gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<codeGame>();
    }

    public void StartTemporizador()
    {
        if (photonView.IsMine)
        {
            InicializarTempo();
            photonView.RPC("InicializarTempoRPC", RpcTarget.OthersBuffered, tempoInicialLocal);
        }
    }


    void InicializarTempo()
    {
        tempoInicialLocal = PhotonNetwork.Time;
        tempoInicialRemoto = tempoInicialLocal;
        temporizador = temporizadorDuracao;
        InvokeRepeating("AtualizarTempo", 0f, 1f);
    }

    [PunRPC]
    void InicializarTempoRPC(double tempoInicial)
    {
        tempoInicialRemoto = tempoInicial;
        tempoInicialLocal = PhotonNetwork.Time;
        AtualizarTempo();
    }

    void AtualizarTempo()
    {
        double tempoAtual = PhotonNetwork.Time;
        double diferencaTempo = tempoAtual - tempoInicialLocal;

        temporizador = temporizadorDuracao - diferencaTempo;

        if (temporizador > 0)
        {
            AtualizarTempoTexto();

            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("SyncTempo", RpcTarget.OthersBuffered, tempoInicialLocal, temporizador);
        }
        else
        {
            temporizadorTexto.text = "Timer is Over!!!";
            CancelInvoke("AtualizarTempo");

            bool playerWin = gameManager.VerifyGameResult(PhotonNetwork.LocalPlayer.ActorNumber);

            print(playerWin == true ? "[LOCAL] You Win!!!" : "[LOCAL] You Lose!!!");

            photonView.RPC("DisplayResult", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, playerWin);

            //if (photonView.IsMine)
            //{
            //    photonView.RPC("CarregarCenaRPC", RpcTarget.AllBuffered);
            //}
        }
    }

    [PunRPC]
    void DisplayResult(int playerActorNumber, bool playerWin)
    {
        resultWin.SetActive(false);
        resultLose.SetActive(false);

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerActorNumber && playerWin)
        {
            print("[RPC] You Win!!!");
            resultWin.SetActive(true);
        }
        else
        {
            print("[RPC] You Lose!!!");
            resultLose.SetActive(true);            
        }
    }


    [PunRPC]
    //void CarregarCenaRPC()
    //{
    //    SceneManager.LoadScene("sceneResult");
    //}

    void AtualizarTempoTexto()
    {
        int minutos = Mathf.FloorToInt((float)temporizador / 60);
        int segundos = Mathf.FloorToInt((float)temporizador % 60);
        temporizadorTexto.text = string.Format("Timer: " + "{0:00}:{1:00}", minutos, segundos);
    }

    [PunRPC]
    void SyncTempo(double tempoInicial, double temporizador)
    {
        tempoInicialRemoto = tempoInicial;
        this.temporizador = temporizador;
        AtualizarTempoTexto();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tempoInicialLocal);
            stream.SendNext(temporizador);
        }
        else
        {
            double tempoRemotoRecebido = (double)stream.ReceiveNext();
            double temporizadorRemotoRecebido = (double)stream.ReceiveNext();

            tempoInicialRemoto = tempoRemotoRecebido;
            temporizador = temporizadorRemotoRecebido;

            AtualizarTempoTexto();
        }
    }

    public void Menu()
    {
        SceneManager.LoadScene("sceneMenu");
    }
}