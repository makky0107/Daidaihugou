using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
public class PlayStart : MonoBehaviour
{
    public static PlayStart instance;
    public int sCardNo;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void PlayStartButton()
    {
        PlayerPrefs.SetInt("SCardKey", sCardNo);
        PlayerPrefs.Save();

        SceneManager.LoadScene("StandardStandBy");
    }
}