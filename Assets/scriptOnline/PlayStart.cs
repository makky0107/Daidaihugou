using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

// MonoBehaviourPunCallbacksを継承して、PUNのコールバックを受け取れるようにする
public class PlayStart : MonoBehaviour
{
    public int sCardNo;

    public void PlayStartButton()
    {
        sCardNo = 0;

        PlayerPrefs.SetInt("SCardKey", sCardNo);
        PlayerPrefs.Save();

        SceneManager.LoadScene("StandardStandBy");
    }
}