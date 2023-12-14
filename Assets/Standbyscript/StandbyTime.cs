using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class StandbyTime : MonoBehaviourPunCallbacks
{
    int time;

    public Text countText;
    public Text memberText;
    public Text joinText;
    public Text gameStart;

    public float fadeInTime;
    public int playerCount;

    public bool isMatching;
    private bool canRun = false;

    public static StandbyTime instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(instance);
        }
    }

    private IEnumerator Start()
    {
        //Debug.Log("start");
        PhotonNetwork.ConnectUsingSettings();
        gameStart.gameObject.SetActive(false);
        yield return new WaitForSeconds(4f); // 4秒待つ
        canRun = true;
    }

    [PunRPC]
    public void StartInfo()
    {
        //Debug.Log("StartLog");
        StartCoroutine(StartInfoCoroutine());
    }

    IEnumerator StartInfoCoroutine()
    {
        //Debug.Log("StartInfoCoroutine");
        countText.gameObject.SetActive(false);
        memberText.gameObject.SetActive(false);
        joinText.gameObject.SetActive(false);
        gameStart.gameObject.SetActive(true);
        StartCoroutine(FadeIn(gameStart));
        yield return new WaitForSeconds(3);
        isMatching = true;
        SceneManager.LoadScene("PlayScene");
        //Debug.Log("Done");
    }

    IEnumerator FadeIn(Text text)
    {
        for (float t = 0.01f; t < fadeInTime; t += Time.deltaTime)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(0, 1, t / fadeInTime));
            yield return null;
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("timer"))
        {
            time = (int)propertiesThatChanged["timer"];
            countText.text = time.ToString();
        }
    }


    // マスターサーバーへの接続が成功した時に呼ばれるコールバック
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
        StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        //Debug.Log("CountDown");
        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("IsMasterClient");
            time = 60;
            countText.text = time.ToString();

            while (time > 0)
            {
                yield return new WaitForSeconds(1);
                time--;
                countText.text = time.ToString();
                ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
                {
                {"timer", time}
                };

                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }

            photonView.RPC("StartInfo", RpcTarget.All);
        }
    }

    private void Update()
    {
        if (canRun)
        {
            if (isMatching)
            {
                return;
            }
            else
            {
                playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

                joinText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/4";

                if (PhotonNetwork.CurrentRoom.PlayerCount == 4)
                {
                    photonView.RPC("TriggerStartInfo", RpcTarget.All);
                }
            }
        }
    }
}
